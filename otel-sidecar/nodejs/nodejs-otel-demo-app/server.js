const express = require('express');
const http = require('http');
const logger = require('./logger');

const app = express();
const port = process.env.PORT || 8080;

app.get('/', (req, res) => {
  logger.info('Root endpoint accessed');
  res.json({ message: 'Hello from Node.js container app!' });
});

app.get('/health', (req, res) => {
  logger.info('Health check performed');
  res.json({ status: 'healthy' });
});

app.get('/test', (req, res) => {
  logger.info('Starting test endpoint - making HTTP call to httpbin.org');

  let responseSent = false;
  const safeSend = (statusCode, payload) => {
    if (responseSent) {
      logger.warn('Attempted to send multiple responses for /test; ignoring.');
      return;
    }

    responseSent = true;
    if (res.headersSent) {
      logger.warn('Headers already sent for /test response; skipping payload.');
      return;
    }

    if (statusCode) {
      res.status(statusCode);
    }

    res.json(payload);
  };

  const https = require('https');
  const request = https.get('https://httpbin.org/get', { timeout: 5000 }, (response) => {
    logger.info(`HTTP call completed with status code: ${response.statusCode}`);

    response.on('data', () => {});

    response.on('end', () => {
      safeSend(200, { message: 'Test completed', statusCode: response.statusCode });
    });

    response.on('aborted', () => {
      logger.error('HTTP call aborted');
      safeSend(502, { message: 'Test failed', error: 'Request aborted' });
    });
    response.on('error', (error) => {
      logger.error('Error on HTTP response', { error: error.message });
      safeSend(500, { message: 'Test failed', error: error.message });
    });
    response.on('close', () => {
      logger.error('HTTP response closed unexpectedly');
      safeSend(502, { message: 'Test failed', error: 'Response closed unexpectedly' });
    });
  });

  request.on('error', (error) => {
    logger.error('HTTP call failed', { error: error.message });
    safeSend(500, { message: 'Test failed', error: error.message });
  });

  request.on('timeout', () => {
    logger.error('HTTP call timed out');
    request.destroy(new Error('Request timeout'));
    safeSend(504, { message: 'Test failed', error: 'Request timeout' });
  });
});

app.listen(port, () => logger.info(`Listening on port ${port}`));
