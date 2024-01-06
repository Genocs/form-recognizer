#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src
COPY ["src/Genocs.Integration.CognitiveServices", "Genocs.Integration.CognitiveServices/"]
COPY ["src/Genocs.FormRecognizer.Worker", "Genocs.FormRecognizer.Worker/"]

COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["stylecop.json", "stylecop.json"]
COPY ["LICENSE", "LICENSE"]
COPY ["icon.png", "icon.png"]

WORKDIR "/src/Genocs.FormRecognizer.Worker"

RUN dotnet restore "Genocs.FormRecognizer.Worker.csproj"

RUN dotnet build "Genocs.FormRecognizer.Worker.csproj" -c Debug -o /app/build

FROM build-env AS publish
RUN dotnet publish "Genocs.FormRecognizer.Worker.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.FormRecognizer.Worker.dll"]