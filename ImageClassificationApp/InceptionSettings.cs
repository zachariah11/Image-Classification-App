﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassificationApp
{
    public struct InceptionSettings
    {
        public const int ImageHeight = 224;
        public const int ImageWidth = 224;
        public const float Mean = 117;
        public const float Scale = 1;
        public const bool ChannelsList = true;
    }
}
