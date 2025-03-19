# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Stage 2: Create a runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the build output from stage 1
COPY --from=build /app/out ./

# Datadog specific
RUN mkdir -p /datadog/tracer
RUN mkdir -p /home/LogFiles/dotnet

ADD https://github.com/DataDog/dd-trace-dotnet/releases/download/v2.49.0/datadog-dotnet-apm-2.49.0.tar.gz /datadog/tracer
RUN cd /datadog/tracer && tar -zxf datadog-dotnet-apm-2.49.0.tar.gz

# Set the entry point for the application
ENTRYPOINT ["dotnet", "devShopDNC.dll"]