using Android.Graphics;

namespace EightBitLab.Com.BlurView
{
    public interface IBlurController : IBlurViewFacade
    {
        const float DEFAULT_SCALE_FACTOR = 1f;
        const float DEFAULT_BLUR_RADIUS = 16f;

        /**
         * Draws blurred content on given canvas
         *
         * @return true if BlurView should proceed with drawing itself and its children
         */
        bool Draw(Canvas canvas);

        /**
         * Must be used to notify Controller when BlurView's size has changed
         */
        void UpdateBlurViewSize();

        /**
         * Frees allocated resources
         */
        void Destroy();
    }
}