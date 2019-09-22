﻿using Microsoft.ML;
using System;
using System.IO;
using System.Linq;
namespace ImageClassificationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var context = new MLContext();

                var data = context.Data.LoadFromTextFile<ImageData>("./labels.csv", separatorChar: ',');

                var pipeline = context.Transforms.Conversion.MapValueToKey("LabelKey", "Label")
                    .Append(context.Transforms.LoadImages("input", "images", nameof(ImageData.ImagePath)))
                    .Append(context.Transforms.ResizeImages("input", InceptionSettings.ImageWidth, InceptionSettings.ImageHeight, "input"))
                    .Append(context.Transforms.ExtractPixels("input", interleavePixelColors: InceptionSettings.ChannelsList,
                        offsetImage: InceptionSettings.Mean))
                    .Append(context.Model.LoadTensorFlowModel("./model/tensorflow_inception_graph.pb")
                        .ScoreTensorFlowModel(new[] { "softmax2_pre_activation" }, new[] { "input" }, addBatchDimensionInput: true))
                    .Append(context.MulticlassClassification.Trainers.LbfgsMaximumEntropy("LabelKey", "softmax2_pre_activation"))
                    .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"));

                var model = pipeline.Fit(data);

                var imageData = File.ReadAllLines("./labels.csv")
                    .Select(l => l.Split(','))
                    .Select(l => new ImageData { ImagePath = Path.Combine(Environment.CurrentDirectory, "images", l[0]) });
                var imageDataView = context.Data.LoadFromEnumerable(imageData);

                var predictions = model.Transform(imageDataView);

                var imagePredictions = context.Data.CreateEnumerable<ImagePrediction>(predictions, reuseRowObject: false, ignoreMissingColumns: true);

                // Evaluate
                Console.WriteLine("\n------------Evaluate-----------------");

                var evalPredictions = model.Transform(data);

                var metrics = context.MulticlassClassification.Evaluate(evalPredictions, labelColumnName: "LabelKey",
                    predictedLabelColumnName: "PredictedLabel");

                // Log loss should be close to 0 for accurate predictions
                Console.WriteLine($"Log Loss - {metrics.LogLoss}");

                // Predict single
                var predictionFunc = context.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);

                var singlePrediction = predictionFunc.Predict(new ImageData
                {
                    ImagePath = Path.Combine(Environment.CurrentDirectory, "images", "cup2.jpg")
                });

                Console.WriteLine("\n------------Single prediction-----------------");
                Console.WriteLine($"Image {Path.GetFileName(singlePrediction.ImagePath)} was predicted as a {singlePrediction.PredictedLabelValue} " +
                    $"with a score of {singlePrediction.Score.Max()}");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
            
            Console.ReadKey();
        }
    }
}
