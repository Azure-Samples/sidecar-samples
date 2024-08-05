FROM ollama/ollama
 
EXPOSE 11434
 
COPY startup.sh /
 
RUN chmod +x /startup.sh
 
ENTRYPOINT ["./startup.sh"]