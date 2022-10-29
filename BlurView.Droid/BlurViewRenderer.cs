using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using BlurView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using Element = Xamarin.Forms.Element;
using Rect = Android.Graphics.Rect;
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

        private bool _drawing = false;
        
        private readonly int[] _rootViewLocation = new int[] { -1, -1 };
        private readonly int[] _blurViewLocation = new int[2];
        
        private int _rootViewWidth;
        private int _rootViewHeight;
        
        private Bitmap? _internalBitmap;
        private Canvas? _internalCanvas;
        
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
        }
        
        internal void OnPreDraw()
        {
            if (!_blurView.IsDirty)
                _blurView.PostInvalidate();
        }
        
        // if (_rootViewWidth != _rootView.Width || _rootViewHeight != _rootView.Height)
        // {
        //     try { _internalCanvas?.Dispose(); } catch { /* do nothing */ }
        //         
        //     _internalBitmap?.Recycle();
        //     try { _internalBitmap?.Dispose(); } catch { /* do nothing */ }
        //      
        //     _rootViewWidth = _rootView.Width;
        //     _rootViewHeight = _rootView.Height;
        //         
        //     _internalBitmap = Bitmap.CreateBitmap(_rootViewWidth, _rootViewHeight, Bitmap.Config.Argb8888);
        //     _internalCanvas = new AcrylicCanvas(_blurView, _internalBitmap)
        //     {
        //         Density = 0
        //     };
        // }
        //     
        // _rootView.Draw(_internalCanvas);

        private Bitmap? DrawIt()
        {
            if (_rootView.Width > 0 && _rootView.Height > 0)
            {
                var bitmap = Bitmap.CreateBitmap(_rootView.Width, _rootView.Height, Bitmap.Config.Argb8888);
                using var canvas = new AcrylicCanvas(_blurView, bitmap)
                {
                    Density = 0
                };
                
                _rootView.Draw(canvas);

                return bitmap;
            }
            
            return null;
        }
        
        internal bool Draw(Canvas canvas)
        {
            if (_drawing || (canvas is AcrylicCanvas acrylicCanvas && ReferenceEquals(acrylicCanvas.Parent, _blurView))) return false;

            try
            {
                _drawing = true;
                
                _rootView.GetLocationOnScreen(_rootViewLocation);
                _blurView.GetLocationOnScreen(_blurViewLocation);
            
                var left = _blurViewLocation[0] - _rootViewLocation[0];
                var top = _blurViewLocation[1] - _rootViewLocation[1];

                var croppedBitmap = new Rect(left, top, left + _blurView.Width, top + _blurView.Height);
                var blurViewRect = new Rect(0, 0, _blurView.Width, _blurView.Height);

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

                using var bitmap = DrawIt();
                using var blurred = Blur(bitmap);

                if (blurred is null)
                {
                    return false;
                }

                canvas.DrawBitmap(blurred,
                    src: croppedBitmap,
                    dst: blurViewRect,
                    paint: new Paint(PaintFlags.FilterBitmap));

                canvas.DrawRect(blurViewRect, new Paint { Color = BackgroundColor });

                return true;
            }
            finally
            {
                _drawing = false;
            }
        }
        
        private Bitmap? Blur(Bitmap? image) {
            if (image is null) return null;
            //return image;
            var outputBitmap = Bitmap.CreateBitmap(image);
            var renderScript = RenderScript.Create(_blurView.Context);
            Allocation tmpIn = Allocation.CreateFromBitmap(renderScript, image);
            Allocation tmpOut = Allocation.CreateFromBitmap(renderScript, outputBitmap);
            
            //Intrinsic Gausian blur filter
            ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(renderScript, global::Android.Renderscripts.Element.U8_4(renderScript));
            theIntrinsic.SetRadius(BlurRadius);
            theIntrinsic.SetInput(tmpIn);
            theIntrinsic.ForEach(tmpOut);
            tmpOut.CopyTo(outputBitmap);
            return outputBitmap;
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
        internal readonly View Parent;

        internal AcrylicCanvas(View parent, Bitmap bitmap) : base(bitmap)
        {
            Parent = parent;
        }
    }
}