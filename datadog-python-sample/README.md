# Sample Python app to use with Datadog Sidecar Extension
This repository contains sample python app which you can deploy to run with Datadog Sidecar Extension

Steps to run this app with Datadog Sidecar Extension:
- Create a python app in Azure App Service (Linux)
- Deploy the code from this sample to your app service.
- You will need to update your startup command to - ddtrace-run python app.py
- Add Datadog Sidecar Extension to this app from portal (Deployment Center) with your Datadog details.
 
In case you want to try with a custom container app, use following steps:
- Create a Python app image using given Dockerfile and push it to you ACR.
- Create a new web app from Azure Portal with publish type as Container and Operating System as Linux.
- Choose Sidecar support as enabled in Containers Tab.
- Pick your image from the ACR (where you pushed image) and set port as 8000.
- Add Datadog Sidecar Extension to this app from portal (Deployment Center) with your Datadog details.
