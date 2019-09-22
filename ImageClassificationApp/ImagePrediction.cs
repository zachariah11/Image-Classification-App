using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassificationApp
{
    public class ImagePrediction : ImageData
    {
        public float[] Score { get; set; }

        public string PredictedLabelValue { get; set; }
    }
}
