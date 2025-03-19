#!/bin/bash

# Create directories for Datadog tracer and .NET logs
mkdir -p /datadog/tracer
mkdir -p /home/LogFiles/dotnet

# Download the Datadog tracer tarball
wget -O /datadog/tracer/datadog-dotnet-apm-2.49.0.tar.gz https://github.com/DataDog/dd-trace-dotnet/releases/download/v2.49.0/datadog-dotnet-apm-2.49.0.tar.gz

# Navigate to the tracer directory, extract the tarball, and return to the original directory
pushd /datadog/tracer
tar -zxf datadog-dotnet-apm-2.49.0.tar.gz
popd

dotnet /home/site/wwwroot/devShopDNC.dll
