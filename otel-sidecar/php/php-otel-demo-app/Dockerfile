FROM mcr.microsoft.com/oryx/php:8.3-fpm-debian-bookworm-20250505.1

RUN apt-get update && apt-get install -y \
    autoconf \
    build-essential \
    git \
    curl \
    zip \
    unzip \
    nginx \
    supervisor \
    && rm -rf /var/lib/apt/lists/*

RUN curl -sS https://getcomposer.org/installer | php -- --install-dir=/usr/local/bin --filename=composer

RUN pecl install opentelemetry \
    && docker-php-ext-enable opentelemetry \
    && php -m | grep opentelemetry

WORKDIR /var/www/html

COPY . .
RUN composer install --no-dev --optimize-autoloader
COPY php.ini /usr/local/etc/php/conf.d/99-opentelemetry.ini

ENV OTEL_SERVICE_NAME=php-otel-demo-app
ENV OTEL_TRACES_EXPORTER=otlp
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://127.0.0.1:4318
ENV OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
ENV OTEL_PROPAGATORS=baggage,tracecontext
ENV OTEL_PHP_AUTOLOAD_ENABLED=true

COPY nginx.conf /etc/nginx/sites-available/default
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

RUN mkdir -p /var/log/supervisor
RUN chown -R www-data:www-data /var/www/html \
    && chmod -R 755 /var/www/html

EXPOSE 80
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
