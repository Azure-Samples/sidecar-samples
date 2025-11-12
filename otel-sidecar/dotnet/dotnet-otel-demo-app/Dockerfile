# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV OTEL_SERVICE_NAME=dotnet-otel-demo-app
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4318
ENV OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf

EXPOSE 80

ENTRYPOINT ["dotnet", "dotnet-otel-demo-app.dll"]
