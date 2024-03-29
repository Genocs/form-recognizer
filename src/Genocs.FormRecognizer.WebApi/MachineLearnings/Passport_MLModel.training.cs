﻿// This file was auto-generated by ML.NET Model Builder.
using Microsoft.ML;

namespace Genocs.FormRecognizer.WebApi.MachineLearnings;

public partial class Passport_MLModel_MLModel
{

    /// <summary>
    /// Retrains model using the pipeline generated as part of the training process.
    /// </summary>
    /// <param name="mlContext"></param>
    /// <param name="trainData"></param>
    /// <returns></returns>
    public static ITransformer RetrainModel(MLContext mlContext, IDataView trainData)
    {
        var pipeline = BuildPipeline(mlContext);
        var model = pipeline.Fit(trainData);

        return model;
    }


    /// <summary>
    /// build the pipeline that is used from model builder. Use this function to retrain model.
    /// </summary>
    /// <param name="mlContext"></param>
    /// <returns></returns>
    public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
    {
        // Data process configuration with pipeline data transformations
        var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: @"Label", inputColumnName: @"Label", addKeyValueAnnotationsAsText: false)
                                .Append(mlContext.MulticlassClassification.Trainers.ImageClassification(labelColumnName: @"Label", scoreColumnName: @"Score", featureColumnName: @"ImageSource"))
                                .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: @"PredictedLabel", inputColumnName: @"PredictedLabel"));

        return pipeline;
    }
}

