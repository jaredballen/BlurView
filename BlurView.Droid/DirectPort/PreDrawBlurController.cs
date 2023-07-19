using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using BlurView.Droid.DirectPort;
using BlurView.Droid.Extensions;
using FFImageLoading;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using View = Xamarin.Forms.View;

namespace EightBitLab.Com.BlurView
{
    internal class PreDrawBlurController : IBlurController
    {
        private readonly BlurViewLibrary.BlurViewRenderer _blurViewRenderer;
        private readonly Android.Views.View _rootView;
        private readonly IBlurAlgorithm _blurAlgorithm;
        private readonly ViewTreeObserver.IOnPreDrawListener _preDrawListener;
        
        private BlurViewCanvas? _internalCanvas;
        private Bitmap? _internalBitmap;
        
        private Color _overlayColor = Color.Transparent;
        
        private readonly int[] _rootViewLocation = new int[2];
        private readonly int[] _blurViewLocation = new int[2];
        
        private bool _disposed = false;

        public Context Context => _blurViewRenderer.Context ?? Xamarin.Essentials.Platform.CurrentActivity;
        
        private float _blurRadius = IBlurController.DefaultBlurRadius;
        public float BlurRadius
        {
            get => _blurRadius;
            set
            {
                if (Math.Abs(value - _blurRadius) < float.Epsilon) return;
                _blurRadius = Math.Max(1, Math.Min(value, 25));
            }
        }
        
        private float _scaleFactor = IBlurController.DefaultScaleFactor;
        public float ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                if (Math.Abs(value - _scaleFactor) < float.Epsilon) return;
                _scaleFactor = value;
                Init(_blurViewRenderer.MeasuredWidth, _blurViewRenderer.MeasuredHeight);
            }
        }

        public Color OverlayColor { get; set; } = Color.Transparent;
        
        public PreDrawBlurController(BlurViewLibrary.BlurViewRenderer blurViewRenderer)
        {
            try
            {
                _blurViewRenderer = blurViewRenderer;
            
                _rootView = (ViewGroup)(Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() ??
                                        Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() ??
                                        Application.Current.MainPage ??
                                        Shell.Current.CurrentPage).GetRenderer().View;
            
                _blurAlgorithm = Build.VERSION.SdkInt >= BuildVersionCodes.S
                    ? new RenderEffectBlur(Context)
                    : new RenderScriptBlur(Context);
            
                _preDrawListener = new ViewTreeObserverPreDrawListener(this);

                Init(_blurViewRenderer.MeasuredWidth, _blurViewRenderer.MeasuredHeight);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void Init(int measuredWidth, int measuredHeight)
        {
            try
            {
                if (_disposed) return;
            
                _rootView.ViewTreeObserver?.RemoveOnPreDrawListener(_preDrawListener);
                _rootView.ViewTreeObserver?.AddOnPreDrawListener(_preDrawListener);
            
                var sizeScaler = new SizeScaler(ScaleFactor);
                if (sizeScaler.IsZeroSized(measuredWidth, measuredHeight))
                {
                    // Will be initialized later when the View reports a size change
                    _blurViewRenderer.SetWillNotDraw(true);
                    return;
                }

                _blurViewRenderer.SetWillNotDraw(false);
                var bitmapSize = sizeScaler.Scale(measuredWidth, measuredHeight);

                _internalCanvas.TryDispose();
                _internalBitmap.TryCleanup();
            
                _internalBitmap = Bitmap.CreateBitmap(bitmapSize.Width, bitmapSize.Height, _blurAlgorithm.GetSupportedBitmapConfig());
                _internalCanvas = new BlurViewCanvas(_internalBitmap, _blurViewRenderer);
            
                // Usually it's not needed, because `onPreDraw` updates the blur anyway.
                // But it handles cases when the PreDraw listener is attached to a different Window, for example
                // when the BlurView is in a Dialog window, but the root is in the Activity.
                // Previously it was done in `draw`, but it was causing potential side effects and Jetpack Compose crashes
                UpdateBlur();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void UpdateBlur()
        {
            try
            {
                if (_disposed || _internalBitmap is null || _internalCanvas is null)  return;

                _internalBitmap.EraseColor(Color.Transparent);
            
                _internalCanvas.Save();
                SetupInternalCanvasMatrix();
                _rootView.Draw(_internalCanvas);
                _internalCanvas.Restore();

                if (_internalBitmap is null) return;
                _internalBitmap = _blurAlgorithm.Blur(_internalBitmap, BlurRadius);
            
                if (_blurAlgorithm.CanModifyBitmap() || _internalCanvas is null) return;
                _internalCanvas.SetBitmap(_internalBitmap);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void SetupInternalCanvasMatrix()
        {
            try
            {
                _rootView.GetLocationOnScreen(_rootViewLocation);
                _blurViewRenderer.GetLocationOnScreen(_blurViewLocation);

                var left = _blurViewLocation[0] - _rootViewLocation[0];
                var top = _blurViewLocation[1] - _rootViewLocation[1];
            
                var scaleFactorH = (float)_blurViewRenderer.Height / _internalBitmap.Height;
                var scaleFactorW = (float)_blurViewRenderer.Width / _internalBitmap.Width;

                var scaledLeftPosition = -left / scaleFactorW;
                var scaledTopPosition = -top / scaleFactorH;

                _internalCanvas.Translate(scaledLeftPosition, scaledTopPosition);
                _internalCanvas.Scale(1 / scaleFactorW, 1 / scaleFactorH);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool Draw(Canvas canvas)
        {
            try
            {
                if (_disposed || _internalBitmap is null) return true;
            
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
                    if (!otherBlurViewCanvas.BlurViewRenderer.Intersects(_internalCanvas.BlurViewRenderer))
                    {
                        Log.Debug("BlurView", "SKIPPED: {0} does NOT intersect {1}", otherBlurViewCanvas.BlurViewRenderer.ContentDescription, _internalCanvas.BlurViewRenderer.ContentDescription);
                        return false;
                    }
                
                    // No need to draw if the other blur view is NOT above this one. As drawing this view would not affect
                    // the appearance of the a blue view below itself.
                    //
                    if (!(((otherBlurViewCanvas.BlurViewRenderer as IVisualElementRenderer).Element as View)?.Above((_internalCanvas.BlurViewRenderer as IVisualElementRenderer).Element as View) ?? true))
                    {
                        Log.Debug("BlurView", "SKIPPED: {0} is above {1}", _internalCanvas.BlurViewRenderer.ContentDescription, otherBlurViewCanvas.BlurViewRenderer.ContentDescription);
                        return false;
                    }
                }

                canvas.Save();
                canvas.Scale((float)_blurViewRenderer.Width / _internalBitmap.Width, (float)_blurViewRenderer.Height / _internalBitmap.Height);
                _blurAlgorithm.Render(canvas, _internalBitmap);
                canvas.Restore();
            
                if (OverlayColor != Color.Transparent)
                    canvas.DrawColor(OverlayColor);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public void Resize() => Init(_blurViewRenderer.MeasuredWidth, _blurViewRenderer.MeasuredHeight);
        
        public void Dispose()
        {
            try
            {
                _disposed = true;
            
                _blurAlgorithm.Destroy();
                _preDrawListener.Dispose();
                _internalCanvas?.TryDispose();
                _internalBitmap?.TryCleanup();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private class ViewTreeObserverPreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
        {
            private readonly PreDrawBlurController? _controller;

            public ViewTreeObserverPreDrawListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }
            
            public ViewTreeObserverPreDrawListener(PreDrawBlurController controller) => _controller = controller;

            public bool OnPreDraw()
            {
                _controller?.UpdateBlur();
                return true;
            }
        }
    }
}
