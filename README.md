![Form Recognizer Library](https://raw.githubusercontent.com/genocs/clean-architecture-template/master/images/genocs-icon.png) form-recognizer
====
Web Api service built with .NET Core 5. It allows to extract tag coming from images.
The ML model is built using Azure Cognitive services.

----

[![.NET](https://github.com/Genocs/qrcode/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/Genocs/qrcode/actions/workflows/dotnet.yml) [![Build Status](https://travis-ci.com/Genocs/qrcode.svg?branch=master)](https://travis-ci.com/Genocs/qrcode) <a href="https://www.nuget.org/packages/Genocs.QRCodeLibrary/" rel="Genocs.QRCodeLibrary">![NuGet](https://buildstats.info/nuget/genocs.qrcodelibrary)</a> <a href="https://hub.docker.com/repository/docker/genocs/qrcode.api/" rel="Genocs.QRCodeLibrary">![Docker Automated build](https://img.shields.io/docker/automated/genocs/qrcode.api)</a> [![Gitter](https://img.shields.io/badge/chat-on%20gitter-blue.svg)](https://gitter.im/genocs/)

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


``` PS
  "AzureStorageConfig": {
    "AccountName": "{{AccountName}}",
    "AccountKey": "{{AccountKey}}",
    "UploadContainer": "{{UploadContainer}}",
    "TrainingSetContainerUrl": "{{TrainingSetContainerUrl}}",
    "ThumbnailContainer": "{{ThumbnailContainer}}",
    "InspectingFileUrl": "{{InspectingFileUrl}}"
  },
  "ImageClassifierConfig": {
    "Endpoint": "{{Endpoint}}",
    "PredictionKey": "{{PredictionKey}}",
    "ModelId": "{{ModelId}}"
  },
  "FormRecognizerConfig": {
    "Endpoint": "{{Endpoint}}",
    "PredictionKey": "{{PredictionKey}}"
  },
  "RedisConfig": {
    "ConnectionString": "{{ConnectionString}}"
  }
```  
  

## Docker image

``` PS
docker build -t genocs/formrecognizer -f .\src\Genocs.FormRecognizer.WebApi\Dockerfile .
docker tag genocs/formrecognizer genocs/formrecognizer:2.0
docker push genocs/formrecognizer:2.0
``` 