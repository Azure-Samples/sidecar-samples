const { NodeSDK } = require('@opentelemetry/sdk-node');
const { getNodeAutoInstrumentations } = require('@opentelemetry/auto-instrumentations-node');
const { OTLPTraceExporter } = require('@opentelemetry/exporter-trace-otlp-http');
const { OTLPMetricExporter } = require('@opentelemetry/exporter-metrics-otlp-http');
const { OTLPLogExporter } = require('@opentelemetry/exporter-logs-otlp-http');
const { PeriodicExportingMetricReader } = require('@opentelemetry/sdk-metrics');
const { LoggerProvider, SimpleLogRecordProcessor, BatchLogRecordProcessor } = require('@opentelemetry/sdk-logs');
const { diag, DiagConsoleLogger, DiagLogLevel } = require('@opentelemetry/api');

diag.setLogger(new DiagConsoleLogger(), DiagLogLevel.DEBUG);

const loggerProvider = new LoggerProvider();
loggerProvider.addLogRecordProcessor(
  new BatchLogRecordProcessor(new OTLPLogExporter({
    url: process.env.OTEL_EXPORTER_OTLP_ENDPOINT 
      ? `${process.env.OTEL_EXPORTER_OTLP_ENDPOINT}/v1/logs`
      : 'http://127.0.0.1:4318/v1/logs',
  }))
);

const sdk = new NodeSDK({
  traceExporter: new OTLPTraceExporter(),
  metricReader: new PeriodicExportingMetricReader({
    exporter: new OTLPMetricExporter(),
  }),
  logRecordProcessor: new BatchLogRecordProcessor(new OTLPLogExporter()),
  instrumentations: [getNodeAutoInstrumentations({
    '@opentelemetry/instrumentation-winston': {
      enabled: true,
      loggerProvider: loggerProvider,
    },
  })],
});

sdk.start();
console.log('OpenTelemetry SDK started with diagnostic logging');

module.exports = { loggerProvider };
