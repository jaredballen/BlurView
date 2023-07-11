using System;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using BlurView.Droid.DirectPort;
using BlurView.Droid.Extensions;
using FFImageLoading;
using Xamarin.Forms.Platform.Android;
using View = Xamarin.Forms.View;

namespace EightBitLab.Com.BlurView
{
    internal class PreDrawBlurController : IBlurController
    {
        public const int TRANSPARENT = 0;

        private float blurRadius = IBlurController.DEFAULT_BLUR_RADIUS;
        
        private readonly BlurViewLibrary.BlurView _blurView;
        private readonly ViewGroup _rootView;
        private readonly IBlurAlgorithm _blurAlgorithm;
        private readonly ViewTreeObserver.IOnPreDrawListener _preDrawListener;
        
        private BlurViewCanvas? _internalCanvas;
        private Bitmap? _internalBitmap;

        private readonly int[] _rootViewLocation = new int[2];
        private readonly int[] _blurViewLocation = new int[2];

        private bool _blurEnabled = true;
        private bool _initialized;

        public PreDrawBlurController(BlurViewLibrary.BlurView blurView, ViewGroup rootView, IBlurAlgorithm algorithm)
        {
            _blurView = blurView;
            _rootView = rootView;
            _blurAlgorithm = algorithm;
            
            if (algorithm is RenderEffectBlur renderEffectBlur)
                renderEffectBlur.SetContext(blurView.Context ?? Xamarin.Essentials.Platform.AppContext);
            
            _preDrawListener = new ViewTreeObserverPreDrawListener(this);

            Init(blurView.MeasuredWidth,blurView.MeasuredHeight);
        }

        private void Init(int measuredWidth, int measuredHeight)
        {
            SetBlurAutoUpdate(true);
            
            var sizeScaler = new SizeScaler(_blurAlgorithm.ScaleFactor());
            if (sizeScaler.IsZeroSized(measuredWidth, measuredHeight))
            {
                // Will be initialized later when the View reports a size change
                _blurView.SetWillNotDraw(true);
                return;
            }

            _blurView.SetWillNotDraw(false);
            var bitmapSize = sizeScaler.Scale(measuredWidth, measuredHeight);

            _internalCanvas.TryDispose();
            _internalBitmap.TryCleanup();
            
            _internalBitmap = Bitmap.CreateBitmap(bitmapSize.Width, bitmapSize.Height, _blurAlgorithm.GetSupportedBitmapConfig());
            _internalCanvas = new BlurViewCanvas(_internalBitmap, _blurView);
            _initialized = true;
            // Usually it's not needed, because `onPreDraw` updates the blur anyway.
            // But it handles cases when the PreDraw listener is attached to a different Window, for example
            // when the BlurView is in a Dialog window, but the root is in the Activity.
            // Previously it was done in `draw`, but it was causing potential side effects and Jetpack Compose crashes
            UpdateBlur();
        }

        private void UpdateBlur()
        {
            if (!_blurEnabled || !_initialized || _internalBitmap is null || _internalCanvas is null)  return;

            _internalBitmap.EraseColor(Color.Transparent);
            
            _internalCanvas.Save();
            SetupInternalCanvasMatrix();
            _rootView.Draw(_internalCanvas);
            _internalCanvas.Restore();

            BlurAndSave();
        }

        private void SetupInternalCanvasMatrix()
        {
            _rootView.GetLocationOnScreen(_rootViewLocation);
            _blurView.GetLocationOnScreen(_blurViewLocation);

            var left = _blurViewLocation[0] - _rootViewLocation[0];
            var top = _blurViewLocation[1] - _rootViewLocation[1];
            
            var scaleFactorH = (float)_blurView.Height / _internalBitmap.Height;
            var scaleFactorW = (float)_blurView.Width / _internalBitmap.Width;

            var scaledLeftPosition = -left / scaleFactorW;
            var scaledTopPosition = -top / scaleFactorH;

            _internalCanvas.Translate(scaledLeftPosition, scaledTopPosition);
            _internalCanvas.Scale(1 / scaleFactorW, 1 / scaleFactorH);
        }

        public bool Draw(Canvas canvas)
        {
            if (!_blurEnabled || !_initialized || _internalBitmap is null) return true;
            
            if (canvas is BlurViewCanvas otherBlurViewCanvas)
            {
                // Do not draw to self. This is VERY BAD!!!! It WILL cause drawing to NEVER STOP!!!!
                // 
                if (ReferenceEquals(otherBlurViewCanvas, _internalCanvas))
                {
                    return false;
                }

                // No need to draw if the blur views do not overlap. It is inefficient to draw if the views overlap but
                // it won't cause infinite redraws.
                //
                if (!otherBlurViewCanvas.BlurView.Intersects(_internalCanvas.BlurView))
                {
                    Log.Debug("BlurView", "SKIPPED: {0} does NOT intersect {1}", otherBlurViewCanvas.BlurView.ContentDescription, _internalCanvas.BlurView.ContentDescription);
                    return false;
                }
                
                // No need to draw if the other blur view is NOT above this one. As drawing this view would not affect
                // the appearance of the a blue view below itself.
                //
                if (!(((otherBlurViewCanvas.BlurView as IVisualElementRenderer).Element as View)?.Above((_internalCanvas.BlurView as IVisualElementRenderer).Element as View) ?? true))
                {
                    Log.Debug("BlurView", "SKIPPED: {0} is above {1}", _internalCanvas.BlurView.ContentDescription, otherBlurViewCanvas.BlurView.ContentDescription);
                    return false;
                }
            }

            canvas.Save();
            canvas.Scale((float)_blurView.Width / _internalBitmap.Width, (float)_blurView.Height / _internalBitmap.Height);
            _blurAlgorithm.Render(canvas, _internalBitmap);
            canvas.Restore();
            
            return true;
        }

        private void BlurAndSave()
        {
            if (_internalBitmap is null) return;
            _internalBitmap = _blurAlgorithm.Blur(_internalBitmap, blurRadius);
            
            if (_blurAlgorithm.CanModifyBitmap() || _internalCanvas is null) return;
            _internalCanvas.SetBitmap(_internalBitmap);
        }

        public void UpdateBlurViewSize()
            => Init(_blurView.MeasuredWidth, _blurView.MeasuredHeight);

        public void Destroy()
        {
            SetBlurAutoUpdate(false);
            _blurAlgorithm.Destroy();
            _initialized = false;
        }

        public IBlurViewFacade SetBlurRadius(float radius)
        {
            blurRadius = radius;
            return this;
        }

        public IBlurViewFacade SetBlurEnabled(bool enabled)
        {
            _blurEnabled = enabled;
            SetBlurAutoUpdate(enabled);
            _blurView.Invalidate();
            return this;
        }

        public IBlurViewFacade SetBlurAutoUpdate(bool enabled)
        {
            _rootView.ViewTreeObserver?.RemoveOnPreDrawListener(_preDrawListener);
            if (enabled)
            {
                _rootView.ViewTreeObserver?.AddOnPreDrawListener(_preDrawListener);
            }
            return this;
        }

        private class ViewTreeObserverPreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
        {
            private readonly PreDrawBlurController? _controller;

            public ViewTreeObserverPreDrawListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
            
            public ViewTreeObserverPreDrawListener(PreDrawBlurController controller) => _controller = controller;

            public bool OnPreDraw()
            {
                if (_controller is null) return false;
                _controller.UpdateBlur();
                return true;
            }
        }
    }
}
