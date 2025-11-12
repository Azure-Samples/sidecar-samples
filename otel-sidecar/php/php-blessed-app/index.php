<?php

use Psr\Http\Message\ResponseInterface as Response;
use Psr\Http\Message\ServerRequestInterface as Request;
use Slim\Factory\AppFactory;
use GuzzleHttp\Client;
use GuzzleHttp\Psr7\HttpFactory;
use Monolog\Logger;
use OpenTelemetry\API\Globals;

require __DIR__ . '/vendor/autoload.php';

$loggerProvider = Globals::loggerProvider();
$handler = new \OpenTelemetry\Contrib\Logs\Monolog\Handler(
    $loggerProvider,
    Logger::INFO,
    true
);
$logger = new Logger('php-blessed-app', [$handler]);

$app = AppFactory::create();

$app->get('/', function (Request $request, Response $response) use ($logger) {
    $logger->info('Root endpoint called');
    $response->getBody()->write(json_encode([
        'status' => 'ok',
        'php_version' => PHP_VERSION
    ]));
    return $response->withHeader('Content-Type', 'application/json');
});

$app->get('/health', function (Request $request, Response $response) use ($logger) {
    $logger->info('Health check endpoint called');
    $response->getBody()->write(json_encode(['status' => 'healthy']));
    return $response->withHeader('Content-Type', 'application/json');
});

$app->get('/test', function (Request $request, Response $response) use ($logger) {
    $logger->info('Test endpoint called - making HTTP request to httpbin.org');
    
    $client = new Client(['timeout' => 10]);
    $requestFactory = new HttpFactory();
    $httpRequest = $requestFactory->createRequest('GET', 'https://httpbin.org/get');
    $httpResponse = $client->sendRequest($httpRequest);
    
    $logger->info('HTTP request completed', [
        'status_code' => $httpResponse->getStatusCode(),
        'api' => 'httpbin.org'
    ]);
    
    $response->getBody()->write(json_encode([
        'status' => 'ok',
        'api_status' => $httpResponse->getStatusCode()
    ]));
    return $response->withHeader('Content-Type', 'application/json');
});

try {
    $app->run();
} catch (Exception $e) {
    error_log("Slim error: " . $e->getMessage());
    http_response_code(500);
    echo json_encode(['error' => 'Application error']);
}