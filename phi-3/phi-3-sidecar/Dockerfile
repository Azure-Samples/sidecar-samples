# Step 1: Use an official Python runtime as a base image
FROM python:3.9-slim

# Step 2: Set environment variables to prevent Python from writing pyc files to disk and buffering stdout and stderr
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Step 3: Install system dependencies that are necessary for your application
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Step 4: Install Python dependencies, including ONNX Runtime, Transformers, FastAPI, and Uvicorn
RUN pip install --no-cache-dir --upgrade pip \
    && pip install --no-cache-dir torch onnxruntime onnxruntime-tools transformers fastapi uvicorn \
    && pip install --pre onnxruntime-genai

# Step 5: Set up the working directory inside the container to /app
WORKDIR /app

# Step 6: Copy the entire directory containing the ONNX model and its data files
COPY ./Phi-3-mini-4k-instruct-onnx/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4 /app/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4

# Step 7: Copy the API script into the container
COPY ./model_api.py /app/

# Step 8: List the contents of the /app/microsoft directory to verify the ONNX model files are copied
#RUN ls -l /app/microsoft

# Step 9: Expose the port the app runs on
EXPOSE 8000

# Step 10: Run the API using Uvicorn
CMD ["uvicorn", "model_api:app", "--host", "0.0.0.0", "--port", "8000"]