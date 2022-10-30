#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using BlurView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using Rect = Android.Graphics.Rect;
using Region = Android.Graphics.Region;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(BlurView.BlurView), typeof(BlurViewRenderer))]
namespace BlurView.Droid
{
    public class BlurViewRenderer : ViewRenderer, ViewTreeObserver.IOnPreDrawListener
    {
        private BlurController? _blurController;

        private Color BackgroundColor => ((Element as BlurView)?.BackgroundColor ?? BlurView.DefaultBackgroundColor).ToAndroid();
        
        private float BlurRadius => (float)((Element as BlurView)?.BlurRadius ?? BlurView.DefaultBlurRadius);

        public BlurViewRenderer(IntPtr javaReference, JniHandleOwnership transfer) : base(Xamarin.Essentials.Platform.AppContext) { }

        public BlurViewRenderer(Context context) : base(context) { }
        
        public bool OnPreDraw()
        {
            _blurController?.OnPreDraw();
            return true;
        }

        public override void Draw(Canvas canvas)
        {
            try
            {
                ViewTreeObserver.RemoveOnPreDrawListener(this);

                if (!(_blurController?.Draw(canvas) ?? false)) return;
                
                //if (this.path != null) {
                //    canvas.clipPath(this.path);
                //}
                base.Draw(canvas);
                SetBackgroundColor(Color.Transparent);
            }
            finally
            {
                ViewTreeObserver.AddOnPreDrawListener(this);
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            var currentPage = Application.Current?.MainPage.Navigation.ModalStack.LastOrDefault() ??
                              Application.Current?.MainPage.Navigation.NavigationStack.LastOrDefault() ??
                              Application.Current?.MainPage ??
                              Shell.Current.CurrentPage;

            _blurController = new BlurController(this, currentPage?.GetRenderer()?.View)
            {
                BackgroundColor = BackgroundColor,
                BlurRadius = BlurRadius
            };
            
            ViewTreeObserver.AddOnPreDrawListener(this);
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            
            ViewTreeObserver.RemoveOnPreDrawListener(this);
            
            try { _blurController?.Dispose(); } catch { /* do nothing */ }
            _blurController = null;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            
            //// this.path = new Path();
            //// this.path.addRoundRect(new RectF(0, 0, width, height), cornerRadius, cornerRadius, Path.Direction.CW);
        }
        
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (string.Equals(e.PropertyName, BlurView.BackgroundColorProperty.PropertyName) ||
                string.Equals(e.PropertyName, BlurView.BlurRadiusProperty.PropertyName))
            {
                _blurController.BackgroundColor = BackgroundColor;
                _blurController.BlurRadius = BlurRadius;
            }
        }
    }

    internal class BlurController : IDisposable
    {
        private readonly BlurViewRenderer _blurView;
        private readonly View _rootView;
        
        private readonly int[] _rootViewLocation = { -1, -1 };
        private readonly int[] _blurViewLocation = new int[2];
        
        private int _rootViewWidth;
        private int _rootViewHeight;
        private readonly RenderScript _renderScript;
        private readonly ScriptIntrinsicBlur _blur;
        
        private Bitmap? _internalBitmap;
        private Bitmap? _internalBlurredBitmap;
        private Canvas? _internalCanvas;

        private Allocation? _internalAllocation;
        private Allocation? _internalBlurredAllocation;
        
        private bool _isDrawing;

        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                _blurView.PostInvalidate();
            }
        }

        private float _blurRadius;
        public float BlurRadius
        {
            get => _blurRadius;
            set
            {
                if (_blurRadius == value) return;
                _blurRadius = value;
                _blurView.PostInvalidate();
            }
        }

        public BlurController(BlurViewRenderer blurView, View rootView) : base()
        {
            _blurView = blurView;
            _rootView = rootView;
            
            _renderScript = RenderScript.Create(_blurView.Context);
            _blur = ScriptIntrinsicBlur.Create(_renderScript, Android.Renderscripts.Element.U8_4(_renderScript));
        }
        
        internal void OnPreDraw()
        {
            if (_rootView.Width <= 0 || _rootView.Height <= 0) return;
            
            if (_rootView.Width != _rootViewWidth  || _rootView.Height != _rootViewHeight)
            {
                try { _internalCanvas?.Dispose(); } catch { /* do nothing */ }
                    
                _internalBitmap?.Recycle();
                try { _internalBitmap?.Dispose(); } catch { /* do nothing */ }
                
                _internalBlurredBitmap?.Recycle();
                try { _internalBlurredBitmap?.Dispose(); } catch { /* do nothing */ }
                
                _rootViewWidth = _rootView.Width;
                _rootViewHeight = _rootView.Height;
                    
                _internalBitmap = Bitmap.CreateBitmap(_rootViewWidth, _rootViewHeight, Bitmap.Config.Argb8888);
                _internalBlurredBitmap = Bitmap.CreateBitmap(_rootViewWidth, _rootViewHeight, Bitmap.Config.Argb8888);
                _internalCanvas = new AcrylicCanvas(_internalBitmap)
                {
                    Density = 0
                };
                
                _internalAllocation = Allocation.CreateFromBitmap(_renderScript, _internalBitmap);
                _internalBlurredAllocation = Allocation.CreateFromBitmap(_renderScript, _internalBlurredBitmap);
            }
            
            if (!_blurView.IsDirty)
                _blurView.PostInvalidate();
        }
        
        internal bool Draw(Canvas canvas)
        {
            if (_isDrawing || _internalCanvas is null || ReferenceEquals(canvas, _internalCanvas)) return false;

            try
            {
                _isDrawing = true;

                _rootView.GetLocationOnScreen(_rootViewLocation);
                _blurView.GetLocationOnScreen(_blurViewLocation);

                var left = _blurViewLocation[0] - _rootViewLocation[0];
                var top = _blurViewLocation[1] - _rootViewLocation[1];

                var croppedBitmap = new Rect(left, top, left + _blurView.Width, top + _blurView.Height);
                var blurViewRect = new Rect(0, 0, _blurView.Width, _blurView.Height);


                var _ul = 100;
                var _ur = 100;
                var _lr = 100;
                var _ll = 100;


                var width = blurViewRect.Width();
                var height = blurViewRect.Height();
                var path = new Path();

                // Create "rounded rect" path moving clock-wise starting at the top-left corner.
                //
                var (r0, r1) = GetNormalizedRadius(_ul, _ur, width);
                var first = r0;
                path.MoveTo(r0, 0);
                path.LineTo(width - r1, 0);

                (r0, r1) = GetNormalizedRadius(_ur, _lr, height);
                path.QuadTo(width, 0, width, r0);
                path.LineTo(width, height - r1);

                (r0, r1) = GetNormalizedRadius(_lr, _ll, width);
                path.QuadTo(width, height, width - r0, height);
                path.LineTo(r1, height);

                (r0, r1) = GetNormalizedRadius(_ll, _ul, height);
                path.QuadTo(0, height, 0, height - r0);
                path.LineTo(0, r1);

                path.QuadTo(0, 0, first, 0);


                canvas.Save();
                canvas.ClipPath(path, Region.Op.Intersect);

                // Make the background opaque.
                // 
                canvas.DrawRect(blurViewRect, new Paint { Color = Color.White });

                // //Make it frosty
                // Paint paint = new Paint();
                // paint.setXfermode(new PorterDuffXfermode(PorterDuff.Mode.SRC_IN));
                // ColorFilter filter = new LightingColorFilter(0xFFFFFFFF, 0x00222222); // lighten
                // //ColorFilter filter = new LightingColorFilter(0xFF7F7F7F, 0x00000000);    // darken
                // paint.setColorFilter(filter);

                // Shadow
                // Create paint for shadow
                // paint.setColor(shadowColor);
                // paint.setMaskFilter(new BlurMaskFilter(
                //     blurRadius /* shadowRadius */,
                //     BlurMaskFilter.Blur.NORMAL));
                //
                // // Draw shadow before drawing object
                // canvas.drawRect(20 + offsetX, 20 + offsetY, 100 + offsetX, 100 + offsetY, paint);
                //
                // // Create paint for main object
                // paint.setColor(mainColor);
                // paint.setMaskFilter(null);
                //
                // // Draw main object 
                // canvas.drawRect(20, 20, 100, 100, paint);

                _rootView.Draw(_internalCanvas);

                _blur.SetRadius(BlurRadius);
                _blur.ForEach(_internalBlurredAllocation);
                _blur.SetInput(_internalAllocation);
                _internalBlurredAllocation.CopyTo(_internalBlurredBitmap);

                canvas.DrawBitmap(_internalBlurredBitmap,
                    src: croppedBitmap,
                    dst: blurViewRect,
                    paint: new Paint(PaintFlags.FilterBitmap));

                canvas.DrawRect(blurViewRect, new Paint { Color = BackgroundColor });

                canvas.Restore();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine();
            }
            finally
            {
                _isDrawing = false;
            }

            return true;
        }

        private (int r0, int r1) GetNormalizedRadius(int r0, int r1, int length)
        {
            if (r0 + r1 > length)
            {
                var total = (float)(r0 + r1);
                r0 = (int)(length * (r0 / total));
                r1 = (int)(length * (r1 / total));
            }

            return (r0, r1);
        }
        
        #region IDisposable
        protected virtual void Dispose(bool disposing)
         {
             if (!disposing) return;
             
             try { _internalCanvas?.Dispose(); } catch { /* do nothing */ }
             _internalCanvas = null;
             
             _internalBitmap?.Recycle();
             try { _internalBitmap?.Dispose(); } catch { /* do nothing */ }
             _internalBitmap = null;
             
             _internalBlurredBitmap?.Recycle();
             try { _internalBlurredBitmap?.Dispose(); } catch { /* do nothing */ }
             _internalBlurredBitmap = null;
             
             try { _internalAllocation?.Dispose(); } catch { /* do nothing */ }
             _internalAllocation = null;
             
             try { _internalBlurredAllocation?.Dispose(); } catch { /* do nothing */ }
             _internalBlurredAllocation = null;
         }

         public void Dispose()
         {
             Dispose(true);
             GC.SuppressFinalize(this);
         }

         ~BlurController()
         {
             Dispose(false);
         }
         #endregion
    }
    
    internal class AcrylicCanvas : Canvas
    {
        internal AcrylicCanvas(Bitmap bitmap) : base(bitmap) { }
    }
}