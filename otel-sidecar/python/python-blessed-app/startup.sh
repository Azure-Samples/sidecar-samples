#!/bin/bash

# Install OpenTelemetry packages and auto-instrumentation
pip install -r requirements.txt
opentelemetry-bootstrap -a install

# DO NOT set OTEL_LOG_LEVEL or OTEL_PYTHON_LOG_LEVEL - causes recursion with auto-instrumentation
# Only export traces and metrics (no logs to avoid recursion)
opentelemetry-instrument \
    --traces_exporter otlp \
    --metrics_exporter otlp \
    flask run --host=0.0.0.0 --port=8000