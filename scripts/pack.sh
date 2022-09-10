#!/bin/bash
echo Executing after success scripts on branch $TRAVIS_BRANCH
echo Triggering NuGet package build

dotnet pack ./src/Genocs.Integration.CognitiveServices/Genocs.Integration.CognitiveServices.csproj -o . -p:Version=2.0.$TRAVIS_BUILD_NUMBER

echo Uploading Genocs.Integration.CognitiveServices package to NuGet using branch $TRAVIS_BRANCH

case "$TRAVIS_BRANCH" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
    ;;
esac
