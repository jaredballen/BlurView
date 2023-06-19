using Android.Graphics;

namespace EightBitLab.Com.BlurView
{
    public interface IBlurAlgorithm
    {
        /**
         * @param bitmap     bitmap to be blurred
         * @param blurRadius blur radius
         * @return blurred bitmap
         */
        Bitmap Blur(Bitmap bitmap, float blurRadius);

        /**
         * Frees allocated resources
         */
        void Destroy();

        /**
         * @return true if this algorithm returns the same instance of bitmap as it accepted
         * false if it creates a new instance.
         * <p>
         * If you return false from this method, you'll be responsible to swap bitmaps in your
         * {@link BlurAlgorithm#blur(Bitmap, float)} implementation
         * (assign input bitmap to your field and return the instance algorithm just blurred).
         */
        bool CanModifyBitmap();

        /**
         * Retrieve the {@link Android.Graphics.Bitmap.Config} on which the {@link BlurAlgorithm}
         * can actually work.
         *
         * @return bitmap config supported by the given blur algorithm.
         */
        Bitmap.Config GetSupportedBitmapConfig();

        float ScaleFactor();

        void Render(Canvas canvas, Bitmap bitmap);
    }
}