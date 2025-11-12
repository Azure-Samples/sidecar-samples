FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install -r requirements.txt

RUN opentelemetry-bootstrap -a install

COPY . .

ENV OTEL_SERVICE_NAME=python-otel-demo-app
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4317
ENV OTEL_EXPORTER_OTLP_PROTOCOL=grpc
ENV OTEL_PYTHON_LOGGING_AUTO_INSTRUMENTATION_ENABLED=true

EXPOSE 80
CMD ["opentelemetry-instrument", "--traces_exporter", "otlp", "--metrics_exporter", "otlp", "--logs_exporter", "otlp", "flask", "run", "--host=0.0.0.0", "--port=80"]
