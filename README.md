![Form Recognizer Library](https://raw.githubusercontent.com/Genocs/form-recognizer/master/icon.png) form-recognizer
====
Web Api service built with .NET7. It allows to extract tag coming from images.
The ML model is built using Azure Cognitive services.

----

[![.NET](https://github.com/Genocs/form-recognizer/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Genocs/form-recognizer/actions/workflows/dotnet.yml) [![Build Status](https://app.travis-ci.com/Genocs/form-recognizer.svg?branch=master)](https://app.travis-ci.com/Genocs/form-recognizer.svg?branch=master) <a href="https://www.nuget.org/packages/Genocs.Integration.ML.CognitiveServices/" rel="Genocs.Integration.ML.CognitiveServices">![NuGet](https://buildstats.info/nuget/Genocs.Integration.ML.CognitiveServices)</a> <a href="https://hub.docker.com/repository/docker/genocs/formrecognizer/" rel="Genocs.Integration.ML.CognitiveServices">![Docker Automated build](https://img.shields.io/docker/automated/genocs/formrecognizer)</a> [![Docker Image CI](https://github.com/Genocs/form-recognizer/actions/workflows/docker-image.yml/badge.svg?branch=master)](https://github.com/Genocs/form-recognizer/actions/workflows/docker-image.yml) [![Gitter](https://img.shields.io/badge/chat-on%20gitter-blue.svg)](https://gitter.im/genocs/)


----


## Azure 
The project require to have an Azure subscription with
- Storage account
- Cognitive Services
- Redis Cache

## Distributed cache
The project use a cache to store the classification modelId.
You have two options:
1. Use in memory cache
2. Use Redis cache  

The system by default use the singleton object to store the classification key value pair data.

To use the library remember to setup the classification modelId calling:
POST {{root_url}}/api/Settings

## Setup the environment by using environment variables


``` bash
  "AppSettings": {
    "PassportModel": "{{PassportModelUrl}}"
  },
  "AzureStorage": {
    "AccountName": "{{accountName}}",
    "AccountKey": "{{AccountKey}}",
    "UploadContainer": "{{UploadContainer}}",
    "TrainingSetContainerUrl": "{{TrainingSetContainerUrl}}",
    "ThumbnailContainer": "{{ThumbnailContainer}}",
    "InspectingFileUrl": "{{InspectingFileUrl}}"
  },
  "ImageClassifier": {
    "Endpoint": "{{Endpoint}}",
    "PredictionKey": "{{PredictionKey}}",
    "ModelId": "{{ModelId}}"
  },
  "FormRecognizer": {
    "Endpoint": "{{Endpoint}}",
    "PredictionKey": "{{PredictionKey}}"
  },
  "RedisSettings": {
    "ConnectionString": "{{ConnectionString}}"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
```  
  

# Build and Test

``` bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run WebApi
dotnet run --project ./src/Genocs.FormRecognizer.WebApi

# Run worker
dotnet run --project ./src/Genocs.FormRecognizer.Worker

# Build docker image
docker build -f webapi.dockerfile -t genocs/formrecognizer-webapi:4.0.0 -t genocs/formrecognizer-webapi:latest .
docker build -f worker.dockerfile -t genocs/formrecognizer-worker:4.0.0 -t genocs/formrecognizer-worker:latest .

# Push image on dockerhub
docker push genocs/formrecognizer-webapi:4.0.0
docker push genocs/formrecognizer-webapi:latest

docker push genocs/formrecognizer-worker:4.0.0
docker push genocs/formrecognizer-worker:latest

# Build using docker compose
docker-compose -f docker-compose.yml build

# Run using docker compose
docker-compose -f docker-compose.yml up -d
```

# Contribute


## Support

api-workbench.rest

Use this file inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin

