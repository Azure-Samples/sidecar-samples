#!/bin/bash

# Install OpenTelemetry extension if needed
if ! php -m | grep -q opentelemetry; then
    echo "Installing OpenTelemetry extension..."
    pecl install opentelemetry
    echo "extension=opentelemetry.so" > /usr/local/etc/php/conf.d/99-opentelemetry.ini
    echo "OpenTelemetry extension installed successfully"
fi

# Start PHP-FPM
echo "Starting PHP-FPM..."
php-fpm