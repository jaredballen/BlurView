using System;
using Android.Graphics;

namespace BlurView.Droid.Extensions;

public static class ColorExtensions
{
    public static Color WithAlpha(this Color color, double alpha)
        => new Color(color.R, color.G, color.B, (byte)(byte.MaxValue * Math.Max(Math.Min(alpha, 1), 0)));
}