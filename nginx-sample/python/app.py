from flask import Flask
app = Flask(__name__)
 
@app.route("/")
def index():
    print("Welcome endpoint called!")
    return "Welcome from Python App"

if __name__ == "__main__":
    print("Starting python app")
    app.run(host='0.0.0.0', port=3000)
