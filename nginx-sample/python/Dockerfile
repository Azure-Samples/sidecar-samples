FROM python:3.12
 
WORKDIR /src
COPY . /src
 
RUN pip install -r requirements.txt

EXPOSE 3000 
ENTRYPOINT ["python"]
CMD ["app.py"]
