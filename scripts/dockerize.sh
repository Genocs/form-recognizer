#!/bin/bash
TAG=''
VERSION_TAG=

case "$TRAVIS_BRANCH" in
  "master")
    TAG=latest
    VERSION_TAG=1.0.$TRAVIS_BUILD_NUMBER
    ;;
  "develop")
    TAG=dev
    VERSION_TAG=$TAG-$TRAVIS_BUILD_NUMBER
    ;;
esac

REPOSITORY=$DOCKER_CONTAINER/formrecognizer

docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD
docker build -t $REPOSITORY:$TAG -t $REPOSITORY:$VERSION_TAG -f ./src/Genocs.FormRecognizer.WebApi/Dockerfile .
docker push $REPOSITORY:$TAG
docker push $REPOSITORY:1.0.$VERSION_TAG