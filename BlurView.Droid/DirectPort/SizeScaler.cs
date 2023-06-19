using System;

namespace EightBitLab.Com.BlurView
{
    internal class SizeScaler
    {
        // Bitmap size should be divisible by ROUNDING_VALUE to meet stride requirement.
        // This will help avoiding an extra bitmap allocation when passing the bitmap to RenderScript for blur.
        // Usually it's 16, but on Samsung devices it's 64 for some reason.
        private const int ROUNDING_VALUE = 64;
        private readonly float scaleFactor;

        public SizeScaler(float scaleFactor)
        {
            this.scaleFactor = scaleFactor;
        }

        public Size Scale(int width, int height)
        {
            int nonRoundedScaledWidth = DownscaleSize(width);
            int scaledWidth = RoundSize(nonRoundedScaledWidth);
            // Only width has to be aligned to ROUNDING_VALUE
            float roundingScaleFactor = (float)width / scaledWidth;
            // Ceiling because rounding or flooring might leave empty space on the View's bottom
            int scaledHeight = (int)Math.Ceiling(height / roundingScaleFactor);

            return new Size(scaledWidth, scaledHeight, roundingScaleFactor);
        }

        public bool IsZeroSized(int measuredWidth, int measuredHeight)
        {
            return DownscaleSize(measuredHeight) == 0 || DownscaleSize(measuredWidth) == 0;
        }

        /**
         * Rounds a value to the nearest divisible by {@link #ROUNDING_VALUE} to meet stride requirement
         */
        private int RoundSize(int value)
        {
            if (value % ROUNDING_VALUE == 0)
            {
                return value;
            }
            return value - (value % ROUNDING_VALUE) + ROUNDING_VALUE;
        }

        private int DownscaleSize(float value)
        {
            return (int)Math.Ceiling(value / scaleFactor);
        }

        internal class Size
        {
            public int Width { get; }
            public int Height { get; }
            public float ScaleFactor { get; }

            public Size(int width, int height, float scaleFactor)
            {
                Width = width;
                Height = height;
                ScaleFactor = scaleFactor;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                    return true;
                if (obj == null || GetType() != obj.GetType())
                    return false;

                Size size = (Size)obj;

                if (Width != size.Width)
                    return false;
                if (Height != size.Height)
                    return false;
                return Math.Abs(size.ScaleFactor - ScaleFactor) < float.Epsilon;
            }

            public override int GetHashCode()
            {
                int result = Width;
                result = 31 * result + Height;
                result = 31 * result + ScaleFactor.GetHashCode();
                return result;
            }

            public override string ToString()
            {
                return "Size{" +
                       "width=" + Width +
                       ", height=" + Height +
                       ", scaleFactor=" + ScaleFactor +
                       '}';
            }
        }
    }
}
