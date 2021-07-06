# form-recognizer
Web Api service built with .NET Core 5. It allows to extract tag coming from images.
The ML model is built using Azure Cognitive services. 

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