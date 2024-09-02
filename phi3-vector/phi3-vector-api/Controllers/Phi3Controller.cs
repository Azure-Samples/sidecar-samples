using Microsoft.AspNetCore.Mvc;
using Build5Nines.SharpVector;
using Build5Nines.SharpVector.Data;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace phi3_vector_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Phi3Controller : ControllerBase
    {
        private static BasicMemoryVectorDatabase _vectorDatabase = null!;
        private static Model _model = null!;
        private Tokenizer _tokenizer = null!;
        private static string _systemPrompt = null!;


        public Phi3Controller()
        {
            if (_vectorDatabase == null)
            {
                _vectorDatabase = new BasicMemoryVectorDatabase();
                _model = new Model("/app/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4");
                
                Console.WriteLine("Model and tokenizer loaded.");
                _systemPrompt = "You are a knowledgeable and friendly assistant store assistant. Answer the following question as clearly and concisely as possible, providing any relevant information and examples.";

                LoadAdditionalDocuments("/app/storedocs").Wait();
            }
            _tokenizer = new Tokenizer(_model);
        }

        [HttpPost("generate-response")]
        public async Task GenerateResponse([FromBody] string userPrompt)
        {
            Console.WriteLine("Generate method called with : " + userPrompt);
            string resultText = await SearchVectorDatabase(_vectorDatabase, userPrompt);
            Console.WriteLine("Vector search returned : " + resultText);
            var fullPrompt = $"{_systemPrompt}\n\n{resultText}\n\n{userPrompt}";

            Response.ContentType = "text/plain";
            await foreach (var token in GenerateAiResponse(fullPrompt))
            {
                if(token == null || token == "") {
                    break;
                }
                await Response.WriteAsync(token);
                await Response.Body.FlushAsync(); // Flush the response stream to send the token immediately
            }
        }

        
        private async Task LoadAdditionalDocuments(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                                             f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                                             f.EndsWith(".mdx", StringComparison.OrdinalIgnoreCase)).ToArray();

            var vectorDataLoader = new TextDataLoader<int, string>(_vectorDatabase);
            var tasks = files.Select(async file =>
            {
                Console.WriteLine($"Loading {file}");
                if (System.IO.File.Exists(file))
                {
                    var fileContents = await System.IO.File.ReadAllTextAsync(file);
                    await vectorDataLoader.AddDocumentAsync(fileContents, new TextChunkingOptions<string>
                    {
                        Method = TextChunkingMethod.Paragraph,
                        RetrieveMetadata = (chunk) => file
                    });
                }
            });

            await Task.WhenAll(tasks);
        }

        private async Task<string> SearchVectorDatabase(BasicMemoryVectorDatabase vectorDatabase, string userPrompt)
        {
            var vectorDataResults = await vectorDatabase.SearchAsync(
                userPrompt,     // User Prompt
                pageCount: 8,  // Number of results to return
                threshold: 0.3f // Threshold for the vector comparison
            );

            string result = "";

            foreach (var resultItem in vectorDataResults.Texts)
            {
                result += resultItem.Text + "\n\n";
            }

            return result;
        }
        private async IAsyncEnumerable<string> GenerateAiResponse(string fullPrompt)
        {
            var tokens = _tokenizer.Encode(fullPrompt);
            var generatorParams = new GeneratorParams(_model);
            generatorParams.SetSearchOption("max_length", 4096);
            generatorParams.SetInputSequences(tokens);
            var generator = new Generator(_model, generatorParams);
            Console.WriteLine("Generator created.");

            while (!generator.IsDone())
            {
                
                generator.ComputeLogits();
                generator.GenerateNextToken();
                var output = GetOutputTokens(generator, _tokenizer);
                Console.WriteLine("Generating next token..."+output);
                if (output == null || output=="")
                {
                    break;
                }
                yield return output; // Yield each token as it's generated
            }
        }

        private string GetOutputTokens(Generator generator, Tokenizer tokenizer)
        {
            var outputTokens = generator.GetSequence(0);
            var newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
            return tokenizer.Decode(newToken);
        }
    }

}
