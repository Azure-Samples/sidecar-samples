using Azure.Identity;
using devShopDNC.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection.Metadata;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure;
using Azure.Search.Documents.Models;
using System.Collections.Frozen;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using Azure.Messaging;
using Microsoft.CodeAnalysis;
using OpenAI.Chat;
using OpenAI;
using Azure.AI.OpenAI;
using System.ClientModel;


namespace devShopDNC.Controllers
{
    public class ProductDetailsController : Controller
    {
        
        ProductsDB ProductsDB = new ProductsDB();
        private readonly IConfiguration _configuration;
        
        // Session State
        public const string SessionKeyName = "_chatSessionHistory";
        public const string SessionKeyID = "_productID";
        public const string Data = "_dbData";
        
        // In Memory Cache
        private IMemoryCache _cache;

        public IActionResult ProductDetails(int ProductId)
        {

            List<ProductDetails> singleProductDetail = new List<ProductDetails>();

            singleProductDetail.Add(ProductsDB.GetProductDetails(ProductId));

            var product = ProductsDB.GetProductDetails(ProductId);
         
            string productID = ProductId.ToString();

            HttpContext.Session.SetString(SessionKeyID, productID);

            Console.WriteLine();
            Console.WriteLine("PRODUCT ID=" + productID);
            Console.WriteLine();

            return View(singleProductDetail);
        }

        [Route("/Chat")]
        public IActionResult Index()
        {
            return View("Chat");
        }

        private readonly ILogger<ProductDetailsController> _logger;

        public ProductDetailsController(ILogger<ProductDetailsController> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }
   

        [HttpPost]
        public async Task<IActionResult> GetResponse(string userMessage)
        {
            string product_ID;
            string productName = "";
            string productDescription = "";

            // Retrieve the endpoint and deployment name from configuration
            string endpoint = _configuration["ENDPOINT"] ?? string.Empty;
            string deploymentName = _configuration["DEPLOYMENT_NAME"] ?? string.Empty;

            #region promptcontext
            // Retrieve product ID from session
            product_ID = HttpContext.Session.GetString(SessionKeyID) ?? string.Empty;
            if (!string.IsNullOrEmpty(product_ID))
            {
                var product = ProductsDB.GetProductDetails(int.Parse(product_ID));
                productName = product.ProductName;
                productDescription = product.ProductDescription;
            }
            #endregion

            // Build the prompt with product details
            string prompt = $"{userMessage} Product: {productName}. Description: {productDescription}";

            string messageContent = string.Empty;

            var credential = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions {
                });

            // Create a ChatClient using the AzureOpenAIClient
            AzureOpenAIClient azureClient = new(
            new Uri(endpoint),
            credential);
            ChatClient chatClient = azureClient.GetChatClient(deploymentName);
            
            // Create a completion request
            ChatCompletion completion = chatClient.CompleteChat(
            [
                // System messages represent instructions or other guidance about how the assistant should behave
                //Adjust the prompt and other parameters as needed for your specific use case.
                #region systemmessages
                //new SystemChatMessage("You are an AI assistant that helps people find concise information about products. Keep your responses brief and focus on key points. Respond in a Shakespearean style."),
                new SystemChatMessage("You are an AI assistant that helps people find concise information about products. Keep your responses brief and focus on key points."),
                #endregion
                // User messages represent user input, whether historical or the most recent input
                new UserChatMessage(prompt),
              
            ]);

            messageContent = FormatResponse(completion.Content[0].Text);

            Console.WriteLine("Response from AI model: " + messageContent);
            
            return Json(new { Response = messageContent });
        }

        // Helper function to format the response for better display
        private string FormatResponse(string messageContent)
        {
            messageContent = messageContent.Replace("**", "<strong>").Replace("**", "</strong>"); // Example: Markdown to HTML
            messageContent = messageContent.Replace("- ", "<li>").Replace("\n", "<br/>"); // Convert to list and line breaks

            return $"<p>{messageContent}</p>";
        }


    }
}
