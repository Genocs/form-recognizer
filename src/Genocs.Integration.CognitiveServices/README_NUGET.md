# .NET Core Cognitive Services Integration library

This package contains a set of base functionalities to use Azure Cognitive Services, designed by Genocs.
The libraries are built using .NET standard 2.1.


## Description

Core NuGet package contains Azure Cognitive Services functionalities to be used on DDD services.


## Support

Please check the GitHub repository getting more info.



### DataProvider Settings
Following is about how to setup **AzureCognitiveServices** **AzureStorage** **ImageClassifier**

``` json
  "AzureCognitiveServicesSettings": {
    "Endpoint": "{{Endpoint}}",
    "SubscriptionKey": "{{SubscriptionKey}}"
  },
  "AzureStorageSettings": {
    "AccountName": "{{accountName}}",
    "AccountKey": "{{AccountKey}}",
    "UploadContainer": "{{UploadContainer}}",
    "TrainingSetContainer": "{{TrainingSetContainerUrl}}",
    "ThumbnailContainer": "{{ThumbnailContainer}}"
  },
  "ImageClassifierSettings": {
    "Endpoint": "{{Endpoint}}",
    "PredictionKey": "{{PredictionKey}}",
    "ModelId": "{{ModelId}}"
  }

```

## Release notes

### [2023-01-21] 4.0.0-rc1.0
- Added KYC support

