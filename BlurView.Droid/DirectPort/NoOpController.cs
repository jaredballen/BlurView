using Android.Graphics;
using Android.Graphics.Drawables;
using AndroidX.Annotations;

namespace EightBitLab.Com.BlurView
{
    internal class NoOpController : IBlurController
    {
        public bool Draw(Canvas canvas)
        {
            return true;
        }

        public void UpdateBlurViewSize()
        {
        }

        public void Destroy()
        {
        }

        public IBlurViewFacade SetBlurRadius(float radius)
        {
            return this;
        }

        public IBlurViewFacade SetOverlayColor(int overlayColor)
        {
            return this;
        }

        public IBlurViewFacade SetFrameClearDrawable(Drawable windowBackground)
        {
            return this;
        }

        public IBlurViewFacade SetBlurEnabled(bool enabled)
        {
            return this;
        }

        public IBlurViewFacade SetBlurAutoUpdate(bool enabled)
        {
            return this;
        }
    }
}