#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Genocs.FormRecognizer.WebApi/Genocs.FormRecognizer.WebApi.csproj", "src/Genocs.FormRecognizer.WebApi/"]
COPY ["src/Genocs.Integration.CognitiveServices/Genocs.Integration.CognitiveServices.csproj", "src/Genocs.Integration.CognitiveServices/"]

RUN dotnet restore "src/Genocs.FormRecognizer.WebApi/Genocs.FormRecognizer.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Genocs.FormRecognizer.WebApi"
RUN dotnet build "Genocs.FormRecognizer.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Genocs.FormRecognizer.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.FormRecognizer.WebApi.dll"]