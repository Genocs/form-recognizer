# form-recognizer
Web Api service built with .NET Core 5. It allows to extract tag coming from images.
The ML model is built using Azure Cognitive services. 

## Azure 
The project require to have an Azure subscription with
- Storage account
- Cognitive Services



## All the variables can be setup using environment variables


``` PS
  "AzureStorageConfig": {
    "AccountName": "{{AccountName}}",
    "AccountKey": "{{AccountKey}}",
    "ImageContainer": "{{ImageContainer}}",
    "ThumbnailContainer": "{{ThumbnailContainer}}"
  },
  "AzureCognitiveServicesConfig": {
    "Endpoint": "{{Endpoint}}",
    "SubscriptionKey": "{{SubscriptionKey}}"
    "ModelId": "{{ModelId}}"	
  },
  "AzureCognitiveServicesImageClassifierConfig": {
    "Endpoint": "{{Endpoint}}",
    "PredictionKey": "{{PredictionKey}}"
  }
```  
  

## Docker image

``` PS
docker build -t genocs/formrecognizer -f .\src\Genocs.FormRecognizer.WebApi\Dockerfile .
docker push genocs/formrecognizer
``` 