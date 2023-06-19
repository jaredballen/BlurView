using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using AndroidX.Annotations;

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
            internalCanvas = new BlurViewCanvas(internalBitmap);
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
            if (canvas is BlurViewCanvas otherBlurViewCanvas && ReferenceEquals(otherBlurViewCanvas, internalCanvas))
            {
                return false;
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
