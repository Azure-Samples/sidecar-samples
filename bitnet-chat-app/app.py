from flask import Flask, render_template, request, Response
import requests
import json

app = Flask(__name__)

ENDPOINT = "http://localhost:11434/v1/chat/completions"

@app.route('/')
def index():
    return render_template('chat.html')

@app.route('/chat', methods=['POST'])
def chat():
    user_message = request.json.get("message", "")
    payload = {
        "messages": [
            {"role": "system", "content": "You are a helpful assistant."},
            {"role": "user", "content": user_message}
        ],
        "stream": True,
        "cache_prompt": False,
        "n_predict": 300
    }

    headers = {"Content-Type": "application/json"}

    def stream_response():
        with requests.post(ENDPOINT, headers=headers, json=payload, stream=True) as resp:
            for line in resp.iter_lines():
                if line:
                    text = line.decode("utf-8")
                    if text.startswith("data: "):
                        try:
                            data_str = text[len("data: "):]
                            data_json = json.loads(data_str)
                            for choice in data_json.get("choices", []):
                                content = choice.get("delta", {}).get("content")
                                if content:
                                    yield content
                        except json.JSONDecodeError:
                            pass

    return Response(stream_response(), content_type='text/event-stream')

if __name__ == '__main__':
    app.run(debug=True)