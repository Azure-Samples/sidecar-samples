const express = require("express");
const request = require('request');

const app = express();

const PORT = process.env.PORT || 8080;


app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
});

app.use(express.static('build'));

app.use(express.json());

app.post("/api/generate", (req, res) => {
    request.post('http://localhost:11434/api/generate', { json : {
        "model": "phi3",
        "prompt": req.body.prompt,
        "stream": false,
        "options": {
          "num_keep": 5,
          "num_predict": 150,
          "seed": 42,
          "top_k": 1,
          "top_p": 0.9,
          "tfs_z": 0.5,
          "typical_p": 0.7,
          "repeat_last_n": 33,
          "temperature": 0.8,
          "repeat_penalty": 1.2,
          "presence_penalty": 1.5,
          "frequency_penalty": 1.0,
          "mirostat": 1,
          "mirostat_tau": 0.8,
          "mirostat_eta": 0.6,
          "penalize_newline": true,
          "stop": ["<*end*>"],
          "num_thread": 8
        }
    }}
    , (error, response, body) => {
        if (error) {
          console.error(error)
          return
        }
        
        res.json(body);
    })
});