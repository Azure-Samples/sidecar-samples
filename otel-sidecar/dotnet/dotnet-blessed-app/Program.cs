using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "dotnet-blessed-app";
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://127.0.0.1:4318";

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
    logging.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(otlpEndpoint + "/v1/logs");
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
    });
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint + "/v1/traces");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint + "/v1/metrics");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }));

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", (ILogger<Program> log) =>
{
    log.LogInformation("Root endpoint accessed");
    return new
    {
        message = ".NET Blessed App",
        service = serviceName,
        endpoints = new[] { "/health", "/test" }
    };
});

app.MapGet("/health", (ILogger<Program> log) =>
{
    log.LogInformation("Health check performed");
    return new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow.ToString("o")
    };
});

app.MapGet("/test", async (IHttpClientFactory httpClientFactory, ILogger<Program> log) =>
{
    try
    {
        log.LogInformation("Starting test endpoint - making HTTP call to httpbin.org");
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://httpbin.org/json");
        log.LogInformation("HTTP call completed with status code: {StatusCode}", (int)response.StatusCode);
        return Results.Ok(new
        {
            status = "success",
            code = (int)response.StatusCode
        });
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error occurred during test endpoint execution");
        return Results.Json(new
        {
            status = "error",
            message = ex.Message
        }, statusCode: 500);
    }
});

app.Run();
