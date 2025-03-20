import os
from flask import Flask, jsonify
from store import Store

app = Flask(__name__)
store = Store()
store.init()

@app.route("/")
def index():
    print("Welcome endpoint called!")
    data = [value for value in store.get_all()]
    response = {"Data from Redis Cache (to see the change, restart the app to initialize the redis)": "Welcome from Python App", "redis_values": data}
    return jsonify(response)

if __name__ == "__main__":
    print("Starting python app")
    app.run(host='0.0.0.0', port=3000)
