FROM node:20-alpine

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install --production

# Copy application files
COPY . .

# Expose port
EXPOSE 80

# Start the application with OpenTelemetry instrumentation loaded first
CMD ["node", "--require", "./tracing.js", "server.js"]
