# .NET Container App with OpenTelemetry

ASP.NET Core 8.0 Minimal API containerized with OpenTelemetry auto-instrumentation.

## Features

- ASP.NET Core 8.0 Minimal API
- Multi-stage Docker build (SDK + runtime)
- OpenTelemetry auto-instrumentation via NuGet packages
- OTLP exporter to sidecar collector (http://127.0.0.1:4318)
- Simple JSON endpoints

## Endpoints

- `GET /` - App info and service name
- `GET /health` - Health check with timestamp
- `GET /test` - External API call (httpbin.org) with auto-tracing

## Local Development

```bash
dotnet restore
dotnet run
```

## Build and Deploy

```bash
# Build Docker image
docker build -t sitecontainerssampleacr.azurecr.io/dotnet-otel-demo-app:latest .

# Push to ACR
docker push sitecontainerssampleacr.azurecr.io/dotnet-otel-demo-app:latest

# Deploy to Azure App Service
az webapp config container set \
  --name <app-name> \
  --resource-group <rg> \
  --docker-custom-image-name sitecontainerssampleacr.azurecr.io/dotnet-otel-demo-app:latest
```

## Required App Settings

```
OTEL_SERVICE_NAME=dotnet-otel-demo-app
OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4318
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
APPLICATIONINSIGHTS_CONNECTION_STRING=<your-connection-string>
```

## Auto-Instrumentation

Automatic tracing for:
- ASP.NET Core requests (incoming HTTP)
- HttpClient calls (outgoing HTTP)

No manual span creation needed.
