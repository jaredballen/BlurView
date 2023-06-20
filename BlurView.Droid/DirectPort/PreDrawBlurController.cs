using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Annotations;
using BlurView.Droid.Extensions;
using Xamarin.Forms.Platform.Android;
using View = Xamarin.Forms.View;

namespace EightBitLab.Com.BlurView
{
    internal class PreDrawBlurController : IBlurController
    {
        public const int TRANSPARENT = 0;

        private float blurRadius = IBlurController.DEFAULT_BLUR_RADIUS;

        private readonly IBlurAlgorithm blurAlgorithm;
        private BlurViewCanvas internalCanvas;
        private Bitmap internalBitmap;

        public readonly BlurViewLibrary.BlurView BlurView;
        private int overlayColor;
        private readonly ViewGroup rootView;
        private readonly int[] rootLocation = new int[2];
        private readonly int[] blurViewLocation = new int[2];

        private readonly ViewTreeObserver.IOnPreDrawListener drawListener;

        private bool blurEnabled = true;
        private bool initialized;

        [Nullable]
        private Drawable frameClearDrawable;

        public PreDrawBlurController(BlurViewLibrary.BlurView blurView, ViewGroup rootView, int overlayColor, IBlurAlgorithm algorithm)
        {
            this.rootView = rootView;
            BlurView = blurView;
            this.overlayColor = overlayColor;
            blurAlgorithm = algorithm;
            if (algorithm is RenderEffectBlur renderEffectBlur)
            {
                renderEffectBlur.SetContext(blurView.Context);
            }

            int measuredWidth = blurView.MeasuredWidth;
            int measuredHeight = blurView.MeasuredHeight;

            drawListener = new ViewTreeObserverPreDrawListener(this);

            Init(measuredWidth, measuredHeight);
        }

        void Init(int measuredWidth, int measuredHeight)
        {
            SetBlurAutoUpdate(true);
            SizeScaler sizeScaler = new SizeScaler(blurAlgorithm.ScaleFactor());
            if (sizeScaler.IsZeroSized(measuredWidth, measuredHeight))
            {
                // Will be initialized later when the View reports a size change
                BlurView.SetWillNotDraw(true);
                return;
            }

            BlurView.SetWillNotDraw(false);
            SizeScaler.Size bitmapSize = sizeScaler.Scale(measuredWidth, measuredHeight);
            internalBitmap = Bitmap.CreateBitmap(bitmapSize.Width, bitmapSize.Height, blurAlgorithm.GetSupportedBitmapConfig());
            internalCanvas = new BlurViewCanvas(internalBitmap, BlurView);
            initialized = true;
            // Usually it's not needed, because `onPreDraw` updates the blur anyway.
            // But it handles cases when the PreDraw listener is attached to a different Window, for example
            // when the BlurView is in a Dialog window, but the root is in the Activity.
            // Previously it was done in `draw`, but it was causing potential side effects and Jetpack Compose crashes
            UpdateBlur();
        }

        void UpdateBlur()
        {
            if (!blurEnabled || !initialized)
            {
                return;
            }

            if (frameClearDrawable == null)
            {
                internalBitmap.EraseColor(Color.Transparent);
            }
            else
            {
                frameClearDrawable.Draw(internalCanvas);
            }

            internalCanvas.Save();
            SetupInternalCanvasMatrix();
            rootView.Draw(internalCanvas);
            internalCanvas.Restore();

            BlurAndSave();
        }

        private void SetupInternalCanvasMatrix()
        {
            rootView.GetLocationOnScreen(rootLocation);
            BlurView.GetLocationOnScreen(blurViewLocation);

            int left = blurViewLocation[0] - rootLocation[0];
            int top = blurViewLocation[1] - rootLocation[1];

            // https://github.com/Dimezis/BlurView/issues/128
            float scaleFactorH = (float)BlurView.Height / internalBitmap.Height;
            float scaleFactorW = (float)BlurView.Width / internalBitmap.Width;

            float scaledLeftPosition = -left / scaleFactorW;
            float scaledTopPosition = -top / scaleFactorH;

            internalCanvas.Translate(scaledLeftPosition, scaledTopPosition);
            internalCanvas.Scale(1 / scaleFactorW, 1 / scaleFactorH);
        }

        public bool Draw(Canvas canvas)
        {
            if (!blurEnabled || !initialized)
            {
                return true;
            }
            // Not blurring itself or other BlurViews to not cause recursive draw calls
            // Related: https://github.com/Dimezis/BlurView/issues/110
            if (canvas is BlurViewCanvas otherBlurViewCanvas)
            {
                // Do not draw to self. This is VERY BAD!!!! It WILL cause drawing to NEVER STOP!!!!
                // 
                if (ReferenceEquals(otherBlurViewCanvas, internalCanvas))
                {
                    return false;
                }

                // No need to draw if the blur views do not overlap. It is inefficient to draw if the views overlap but
                // it won't cause infinite redraws.
                //
                if (!otherBlurViewCanvas._view.Intersects(internalCanvas._view))
                {
                    Log.Debug("BlurView", "SKIPPED: {0} does NOT intersect {1}", otherBlurViewCanvas._view.ContentDescription, internalCanvas._view.ContentDescription);
                    return false;
                }
                
                // No need to draw if the other blur view is NOT above this one. As drawing this view would not affect
                // the appearance of the a blue view below itself.
                //
                if (!(((otherBlurViewCanvas._view as IVisualElementRenderer).Element as View)?.Above((internalCanvas._view as IVisualElementRenderer).Element as View) ?? true))
                {
                    Log.Debug("BlurView", "SKIPPED: {0} is above {1}", internalCanvas._view.ContentDescription, otherBlurViewCanvas._view.ContentDescription);
                    return false;
                }
            }
            

            // https://github.com/Dimezis/BlurView/issues/128
            float scaleFactorH = (float)BlurView.Height / internalBitmap.Height;
            float scaleFactorW = (float)BlurView.Width / internalBitmap.Width;

            canvas.Save();
            canvas.Scale(scaleFactorW, scaleFactorH);
            blurAlgorithm.Render(canvas, internalBitmap);
            canvas.Restore();
            if (overlayColor != TRANSPARENT)
            {
                canvas.DrawColor(overlayColor);
            }
            return true;
        }

        private void BlurAndSave()
        {
            internalBitmap = blurAlgorithm.Blur(internalBitmap, blurRadius);
            if (!blurAlgorithm.CanModifyBitmap())
            {
                internalCanvas.SetBitmap(internalBitmap);
            }
        }

        public void UpdateBlurViewSize()
        {
            int measuredWidth = BlurView.MeasuredWidth;
            int measuredHeight = BlurView.MeasuredHeight;

            Init(measuredWidth, measuredHeight);
        }

        public void Destroy()
        {
            SetBlurAutoUpdate(false);
            blurAlgorithm.Destroy();
            initialized = false;
        }

        public IBlurViewFacade SetBlurRadius(float radius)
        {
            blurRadius = radius;
            return this;
        }

        public IBlurViewFacade SetFrameClearDrawable(Drawable frameClearDrawable)
        {
            this.frameClearDrawable = frameClearDrawable;
            return this;
        }

        public IBlurViewFacade SetBlurEnabled(bool enabled)
        {
            blurEnabled = enabled;
            SetBlurAutoUpdate(enabled);
            BlurView.Invalidate();
            return this;
        }

        public IBlurViewFacade SetBlurAutoUpdate(bool enabled)
        {
            rootView.ViewTreeObserver.RemoveOnPreDrawListener(drawListener);
            if (enabled)
            {
                rootView.ViewTreeObserver.AddOnPreDrawListener(drawListener);
            }
            return this;
        }

        public IBlurViewFacade SetOverlayColor(int overlayColor)
        {
            if (this.overlayColor != overlayColor)
            {
                this.overlayColor = overlayColor;
                BlurView.Invalidate();
            }
            return this;
        }

        private class ViewTreeObserverPreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
        {
            private readonly PreDrawBlurController? controller;

            public ViewTreeObserverPreDrawListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
            
            public ViewTreeObserverPreDrawListener(PreDrawBlurController controller) : base()
            {
                this.controller = controller;
            }

            public bool OnPreDraw()
            {
                if (controller is null) return false;
                controller.UpdateBlur();
                return true;
            }
        }
    }
}
