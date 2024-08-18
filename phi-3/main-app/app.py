from flask import Flask, render_template, request, Response
import requests
import time

app = Flask(__name__)

# URL of the FastAPI backend
API_URL = "http://localhost:8000/predict"

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/send_message', methods=['POST'])
def send_message():
    input_text = request.form['input_text']

    def generate():
        start_time = time.time()  # Start the timer
        response = requests.post(API_URL, json={"input_text": input_text}, stream=True)
        end_time = time.time()  # End the timer

        time_taken = end_time - start_time  # Calculate the time taken
        print(f"Time taken for API response: {time_taken:.2f} seconds")  # Print the time taken

        if response.status_code == 200:
            for chunk in response.iter_content(chunk_size=1):
                if chunk:
                    yield chunk.decode('utf-8')
        else:
            yield 'Error: ' + response.text

    return Response(generate(), content_type='text/plain')

if __name__ == '__main__':
    app.run(debug=True)