<!-- PROJECT SHIELDS -->
[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
[![Downloads][downloads-shield]][downloads-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Discord][discord-shield]][discord-url]
[![Gitter][gitter-shield]][gitter-url]
[![Twitter][twitter-shield]][twitter-url]
[![Twitterx][twitterx-shield]][twitterx-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

[license-shield]: https://img.shields.io/github/license/Genocs/form-recognizer?color=2da44e&style=flat-square
[license-url]: https://github.com/Genocs/form-recognizer/blob/main/LICENSE
[build-shield]: https://github.com/Genocs/form-recognizer/actions/workflows/build_and_test.yml/badge.svg?branch=main
[build-url]: https://github.com/Genocs/form-recognizer/actions/workflows/build_and_test.yml
[downloads-shield]: https://img.shields.io/nuget/dt/Genocs.Microservice.Template.svg?color=2da44e&label=downloads&logo=nuget
[downloads-url]: https://www.nuget.org/packages/Genocs.Microservice.Template
[contributors-shield]: https://img.shields.io/github/contributors/Genocs/form-recognizer.svg?style=flat-square
[contributors-url]: https://github.com/Genocs/form-recognizer/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Genocs/form-recognizer?style=flat-square
[forks-url]: https://github.com/Genocs/form-recognizer/network/members
[stars-shield]: https://img.shields.io/github/stars/Genocs/form-recognizer.svg?style=flat-square
[stars-url]: https://img.shields.io/github/stars/Genocs/form-recognizer?style=flat-square
[issues-shield]: https://img.shields.io/github/issues/Genocs/form-recognizer?style=flat-square
[issues-url]: https://github.com/Genocs/form-recognizer/issues
[discord-shield]: https://img.shields.io/discord/1106846706512953385?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square
[discord-url]: https://discord.com/invite/fWwArnkV
[gitter-shield]: https://img.shields.io/badge/chat-on%20gitter-blue.svg
[gitter-url]: https://gitter.im/genocs/
[twitter-shield]: https://img.shields.io/twitter/follow/genocs?color=1DA1F2&label=Twitter&logo=Twitter&style=flat-square
[twitter-url]: https://twitter.com/genocs
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=flat-square&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/
[twitterx-shield]: https://img.shields.io/twitter/url/https/twitter.com/genocs.svg?style=social
[twitterx-url]: https://twitter.com/genocs


<p align="center">
    <img src="./assets/genocs-library-logo.png" alt="icon">
</p>



Nuget package & Web Api service built with .NET8.
This repo contains the source code for the Genocs.FormRecognizer project. 

Genocs.FormRecognizer is a .NET library that allows to extract tag coming from images. The ML model is built using Azure Cognitive services. 

**NOTE:** Version 5.x.x doesn't support Microsoft Face Api any more


## Prerequisites 

The project requires to have an Azure subscription with
- Storage account
- Cognitive Services
- Redis Cache

### Distributed cache
The project use a cache to store the classification modelId.
You have two options:
1. Use in memory cache
2. Use Redis cache  

The system by default use the singleton object to store the classification key value pair data.

To use the library remember to setup the classification modelId calling:
POST {{root_url}}/api/Settings

### Setup the environment by using environment variables

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
    "Endpoint": "https://westeurope.api.cognitive.microsoft.com",
    "PredictionKey": "{{PredictionKey}}",
    "ModelId": "{{ModelId}}"
  },
  "FormRecognizer": {
    "Endpoint": "https://westeurope.api.cognitive.microsoft.com",
    "PredictionKey": "{{PredictionKey}}"
  },
  "AzureVision": {
    "Endpoint": "https://westeurope.cognitiveservices.azure.com",
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
  
## Integration Events

The system use RabbitMQ to publish events. The following events are published:

`FormDataExtractionCompleted`

``` csharp
namespace Genocs.Integration.CognitiveServices.IntegrationEvents;

/// <summary>
/// The event to raised when Form Data extraction is completed.
/// </summary>
public class FormDataExtractionCompleted
{
    /// <summary>
    /// The reference id. Client can use it to keep external reference.
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// The request id.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// The context id.
    /// </summary>
    public string? ContextId { get; set; }

    /// <summary>
    /// The processed resource url.
    /// </summary>
    public string ResourceUrl { get; set; } = default!;

    /// <summary>
    /// The classification object.
    /// </summary>
    public Classification? Classification { get; set; }

    /// <summary>
    /// The dynamic data about the results.
    /// </summary>
    public List<dynamic>? ContentData { get; set; }
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


api-workbench.rest

Use this file inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin


## License

This project is licensed with the [MIT license](LICENSE).

## Changelogs

View Complete [Changelogs](https://github.com/Genocs/form-recognizer/blob/main/CHANGELOGS.md).

## Community

- Discord [@genocs](https://discord.com/invite/fWwArnkV)
- Facebook Page [@genocs](https://facebook.com/Genocs)
- Youtube Channel [@genocs](https://youtube.com/c/genocs)


## Support

Has this Project helped you learn something New? or Helped you at work?
Here are a few ways by which you can support.

- ⭐ Leave a star! 
- 🥇 Recommend this project to your colleagues.
- 🦸 Do consider endorsing me on LinkedIn for ASP.NET Core - [Connect via LinkedIn](https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/) 
- ☕ If you want to support this project in the long run, [consider buying me a coffee](https://www.buymeacoffee.com/genocs)!
  

[![buy-me-a-coffee](https://raw.githubusercontent.com/Genocs/form-recognizer/main/assets/buy-me-a-coffee.png "buy-me-a-coffee")](https://www.buymeacoffee.com/genocs)

## Code Contributors

This project exists thanks to all the people who contribute. [Submit your PR and join the team!](CONTRIBUTING.md)

[![genocs contributors](https://contrib.rocks/image?repo=Genocs/form-recognizer "genocs contributors")](https://github.com/genocs/form-recognizer/graphs/contributors)

## Financial Contributors

Become a financial contributor and help me sustain the project. [Support the Project!](https://opencollective.com/genocs/contribute)

<a href="https://opencollective.com/genocs"><img src="https://opencollective.com/genocs/individuals.svg?width=890"></a>


## Acknowledgements









