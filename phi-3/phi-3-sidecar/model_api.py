from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
import onnxruntime as ort
import numpy as np
import onnxruntime_genai as og

app = FastAPI()

# Load the ONNX model
model_path = "/app/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4"
model = og.Model(model_path)

# Load the tokenizer from onnxruntime_genai
tokenizer = og.Tokenizer(model)
tokenizer_stream = tokenizer.create_stream()
print("Model", model)


class InputData(BaseModel):
    input_text: str

@app.post("/predict")
def predict(data: InputData):
    try:
        # Preprocess the input data
        input_text = data.input_text

        chat_template = '<|user|>\n{input} <|end|>\n<|assistant|>'
        prompt = f'{chat_template.format(input=input_text)}'
        print("Prompt", prompt)
        input_tokens = tokenizer.encode(prompt)

        params = og.GeneratorParams(model)
        params.set_search_options(max_length=2048)
        params.set_search_options(do_sample=False)
        params.input_ids = input_tokens

        print("Input tokens", input_tokens)
        generator = og.Generator(model, params)
        
        def token_generator():
            generated_text = ""
            print("Starting generator", generator.is_done())
            while not generator.is_done():
                generator.compute_logits()
                generator.generate_next_token()
            
                new_token = generator.get_next_tokens()[0]
                generated_text += tokenizer_stream.decode(new_token)
                yield tokenizer_stream.decode(new_token) 
                
        return StreamingResponse(token_generator(), media_type="text/plain")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))