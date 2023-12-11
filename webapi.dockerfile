#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src
COPY ["src/Genocs.FormRecognizer.WebApi", "src/Genocs.FormRecognizer.WebApi/"]

COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["stylecop.json", "stylecop.json"]


COPY ["LICENSE", "LICENSE"]
COPY ["icon.png", "icon.png"]

WORKDIR "/src/src/Genocs.FormRecognizer.WebApi"

RUN dotnet restore "Genocs.FormRecognizer.WebApi.csproj"

RUN dotnet build "Genocs.FormRecognizer.WebApi.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "Genocs.FormRecognizer.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.FormRecognizer.WebApi.dll"]