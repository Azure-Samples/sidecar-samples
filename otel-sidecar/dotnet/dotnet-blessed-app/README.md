# .NET Blessed App with OpenTelemetry

ASP.NET Core 8.0 Minimal API with OpenTelemetry auto-instrumentation for Azure App Service blessed image deployment.

## Features

- ASP.NET Core 8.0 Minimal API
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
# Publish release build
dotnet publish -c Release -o ./publish

# Create ZIP for deployment
cd publish
Compress-Archive -Path * -DestinationPath ../dotnet-blessed-app.zip -Force

# Deploy to Azure
az webapp deploy --resource-group <rg> --name <app-name> --src-path ../dotnet-blessed-app.zip --type zip
```

## Required App Settings

```
OTEL_SERVICE_NAME=dotnet-blessed-app
OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4318
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
APPLICATIONINSIGHTS_CONNECTION_STRING=<your-connection-string>
```

## Auto-Instrumentation

Automatic tracing for:
- ASP.NET Core requests (incoming HTTP)
- HttpClient calls (outgoing HTTP)

No manual span creation needed.
