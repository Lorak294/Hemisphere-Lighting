﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public static class ColorConverter
    {
        public static NormalizedColor RGBToStandarized(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            return new NormalizedColor(r, g, b);
        }

        public static Color StandarizedToRGB(NormalizedColor colorStandarized)
        {
            return Color.FromArgb(
                Math.Min((int)(colorStandarized.r * 255.0), 255), 
                Math.Min((int)(colorStandarized.g * 255.0), 255),
                Math.Min((int)(colorStandarized.b * 255.0), 255));
        }
    }
}
