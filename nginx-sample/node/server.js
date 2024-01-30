'use strict';

const express = require('express');

// Constants
const PORT = 4000;
const HOST = '0.0.0.0';

// App
const app = express();
app.get('/', (req, res) => {
  console.info("Welcome endpoint called!")
  res.send("Welcome from Nodejs App");
});

app.listen(PORT, HOST, () => {
  console.log(`Node app running on http://${HOST}:${PORT}`);
});
