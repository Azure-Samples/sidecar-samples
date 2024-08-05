#!/usr/bin/env bash
 
# Start Ollama in the background
ollama serve &
sleep 5
 
# Pull and run phi3
ollama pull phi3
 
# Restart ollama and run it in to foreground.
pkill -f "ollama"
ollama serve