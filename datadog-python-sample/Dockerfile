FROM python:3.13
 
WORKDIR /src
COPY . /src
 
RUN pip install -r requirements.txt

EXPOSE 8000 
ENTRYPOINT ["ddtrace-run", "python", "app.py"]
