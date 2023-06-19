using Android.Graphics.Drawables;

namespace EightBitLab.Com.BlurView
{
    public interface IBlurViewFacade
    {
        IBlurViewFacade SetBlurEnabled(bool enabled);

        IBlurViewFacade SetBlurAutoUpdate(bool enabled);

        IBlurViewFacade SetFrameClearDrawable(Drawable frameClearDrawable);

        IBlurViewFacade SetBlurRadius(float radius);

        IBlurViewFacade SetOverlayColor(int overlayColor);
    }
}