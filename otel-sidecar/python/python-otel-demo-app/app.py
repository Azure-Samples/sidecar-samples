import os
import logging
from datetime import datetime
from flask import Flask, jsonify
import requests

logging.basicConfig(level=logging.NOTSET)
logging.getLogger().setLevel(logging.INFO)

logger = logging.getLogger(__name__)
app = Flask(__name__)

@app.route('/')
def home():
    logger.info("Home endpoint called")
    return jsonify({
        'message': 'Python OTEL Demo App',
        'endpoints': ['/health', '/test']
    })

@app.route('/health')
def health():
    logger.info("Health check endpoint called")
    return jsonify({
        'status': 'healthy',
        'timestamp': datetime.utcnow().isoformat()
    })

@app.route('/test')
def test_external():
    logger.info("Test endpoint called - making external request")
    try:
        response = requests.get('https://httpbin.org/json', timeout=10)
        logger.info(f"External request successful: status={response.status_code}")
        return jsonify({'status': 'success', 'code': response.status_code})
    except Exception as e:
        logger.error(f"External request failed: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=80)
