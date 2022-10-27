using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using BlurView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android = Xamarin.Forms.PlatformConfiguration.Android;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(BlurView.BlurView), typeof(BlurViewRenderer))]
namespace BlurView.Droid
{
    public class BlurViewRenderer : ViewRenderer, ViewTreeObserver.IOnPreDrawListener
    {
        private readonly BlurController _blurController;

        private View? RootView => (Element as BlurView)?.RootView?.GetRenderer()?.View;

        private Color Backgroundcolor => ((Element as BlurView)?.BackgroundColor ?? BlurView.DefaultBackgroundColor).ToAndroid();
        
        private float BlurRadius => (float)((Element as BlurView)?.BlurRadius ?? BlurView.DefaultBlurRadius);
        
        public BlurViewRenderer(Context context) : base(context)
        {
            _blurController = new BlurController(this, Backgroundcolor, BlurRadius);
        }
        
        public bool OnPreDraw()
        {
            _blurController.Update();
            return true;
        }

        public override void Draw(Canvas canvas)
        {
            if (!_blurController.Draw(canvas)) return;
            base.Draw(canvas);
            SetBackgroundColor(Color.Transparent);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if (Element is not null)
            {
                _blurController.RootView = RootView;
                _blurController.BackgroundColor = Backgroundcolor;
                _blurController.BlurRadius = BlurRadius;
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            ViewTreeObserver.AddOnPreDrawListener(this);
            
            _blurController.RootView = RootView;
            _blurController.BackgroundColor = Backgroundcolor;
            _blurController.BlurRadius = BlurRadius;
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            ViewTreeObserver.RemoveOnPreDrawListener(this);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            _blurController.UpdateBlurViewSize();
        }
        
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            
            if (string.Equals(e.PropertyName, BlurView.BackgroundColorProperty.PropertyName) ||
                string.Equals(e.PropertyName, BlurView.RootViewProperty.PropertyName) ||
                string.Equals(e.PropertyName, BlurView.BlurRadiusProperty.PropertyName))
            {
                _blurController.RootView = RootView;
                _blurController.BackgroundColor = Backgroundcolor;
                _blurController.BlurRadius = BlurRadius;
            }
        }
    }

    internal class BlurController
    {
        private readonly int[] _rootViewLocation = new int[2];
        private readonly int[] _blurViewLocation = new int[2];
        
        private readonly View _blurView;
        private Bitmap? _internalBitmap;
        private BlurViewCanvas? _internalCanvas;
        private bool _initialized;

        private View? _rootView;
        public View? RootView
        {
            get => _rootView;
            set
            {
                if (ReferenceEquals(_rootView, value)) return;
                _rootView = value;

                if (_initialized)
                {
                    Update();
                    _blurView.Invalidate();
                }
            }
        }

        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                
                if(_initialized)
                    _blurView.Invalidate();
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
                
                if(_initialized)
                    _blurView.Invalidate();
            }
        }

        public BlurController(View blurView, Color backgroundColor, float blurRadius) : base()
        {
            _blurView = blurView;
            BackgroundColor = backgroundColor;
            BlurRadius = blurRadius;
        }

        internal void Init()
        {
            // var blurViewMeasuredWidth = _blurView.MeasuredWidth;
            // var blurViewMeasuredHeight = _blurView.MeasuredHeight;
            //
            // if (blurViewMeasuredWidth == 0 || blurViewMeasuredHeight == 0)
            // {
            //     _blurView.SetWillNotDraw(true);
            //     return;
            // }
            //
            // _blurView.SetWillNotDraw(true);
            //
            // _internalBitmap?.Dispose();
            // _internalBitmap = Bitmap.CreateBitmap(blurViewMeasuredWidth, blurViewMeasuredHeight, Bitmap.Config.Argb8888);
            //
            // _internalCanvas?.Dispose();
            // _internalCanvas = new BlurViewCanvas(_blurView, _internalBitmap);
            // _initialized = true;
        }
        
        internal void Update()
        {
            // if (!_initialized) return;
            //
            // if (_rootView is null) return;
            //
            // _internalBitmap.EraseColor(Color.White);
            //
            // _internalCanvas.Save();
            // SetupInternalCanvasMatrix();
            // //_rootView?.Draw(_internalCanvas);

            _internalBitmap = RenderUsingCanvasDrawing(_blurView, _rootView);
            
            // _internalCanvas.Restore();
        }
        
        private void SetupInternalCanvasMatrix() 
        {
            if (_rootView is null)
            {
                _rootViewLocation[0] = 0;
                _rootViewLocation[1] = 0;
            }
            else
                _rootView.GetLocationOnScreen(_rootViewLocation);
            
            _blurView.GetLocationOnScreen(_blurViewLocation);

            var left = _blurViewLocation[0] - _rootViewLocation[0];
            var top = _blurViewLocation[1] - _rootViewLocation[1];

            _internalCanvas?.Translate(-left, -top);
        }

        internal bool Draw(Canvas canvas)
        {
            // if (!_initialized) return true;

            // Do not try to draw a blur on a blur canvas.
            // This would be very bad!!!
            /// 
            if (canvas is BlurViewCanvas) return false;
            
            
            // TODO: Does this still need to be called here? 
            Update();
            
            _rootView.GetLocationOnScreen(_rootViewLocation);
            _blurView.GetLocationOnScreen(_blurViewLocation);

            var left = _blurViewLocation[0] - _rootViewLocation[0];
            var top = _blurViewLocation[1] - _rootViewLocation[1];
            
            var croppedBitmap = new global::Android.Graphics.Rect(left, top, left + _blurView.Width, top + _blurView.Height);
            var blurViewRect = new global::Android.Graphics.Rect(0, 0, _blurView.Width, _blurView.Height);
            
            
            // Make the background opaque.
            // 
            canvas.DrawRect(blurViewRect, new Paint { Color = Color.White });
            
            canvas.DrawBitmap(Blur(_internalBitmap),
                src: croppedBitmap,
                dst: blurViewRect,
                paint: new Paint(PaintFlags.FilterBitmap));
            
            canvas.DrawRect(blurViewRect, new Paint { Color = BackgroundColor });
            
            return true;
        }
        
        public void UpdateBlurViewSize()
        {
            Init();
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

        
         private Bitmap RenderUsingCanvasDrawing(View parent, View view)
         {
             try
             {
                 if (view?.LayoutParameters == null || Bitmap.Config.Argb8888 == null)
                     return null;
                 var width = view.Width;
                 var height = view.Height;

                 var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                 if (bitmap == null)
                     return null;

                 using (var canvas = new BlurViewCanvas(parent, bitmap))
                 {
                     canvas.Density = 0;
                     
                     view.Draw(canvas);
                 }

                 return bitmap;
             }
             catch (Exception e)
             {
                 return null;
             }
         }
        
        
//         static Bitmap Render(View view)
//         {
//             var bitmap = RenderUsingCanvasDrawing(view);
//             
//             if (bitmap == null)
//                 bitmap = RenderUsingDrawingCache(view);
//
//             return bitmap;
//         }
//         
//         static Bitmap RenderUsingCanvasDrawing(View view)
//         {
//             try
//             {
//                 if (view?.LayoutParameters == null || Bitmap.Config.Argb8888 == null)
//                     return null;
//                 var width = view.Width;
//                 var height = view.Height;
//
//                 var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
//                 if (bitmap == null)
//                     return null;
//
//                 using (var canvas = new BlurViewCanvas(bitmap))
//                 {
//                     canvas.Density = 0;
//                     
//                     view.Draw(canvas);
//                 }
//
//                 return bitmap;
//             }
//             catch (Exception)
//             {
//                 return null;
//             }
//         }
//         
//         static Bitmap RenderUsingDrawingCache(View view)
//         {
// #pragma warning disable CS0618 // Type or member is obsolete
//             try
//             {
//                 var enabled = view.DrawingCacheEnabled;
//                 view.DrawingCacheEnabled = true;
//                 view.BuildDrawingCache();
//                 var cachedBitmap = view.DrawingCache;
//                 if (cachedBitmap == null)
//                     return null;
//                 var bitmap = Bitmap.CreateBitmap(cachedBitmap);
//                 view.DrawingCacheEnabled = enabled;
//                 return bitmap;
//             }
//             catch (Exception)
//             {
//                 return null;
//             }
// #pragma warning restore CS0618 // Type or member is obsolete
//         }
    }

    /// <summary>
    /// Marker class to prevent BlurViews from trying to blur other BlurViews.
    /// </summary>
    internal class BlurViewCanvas : Canvas
    {
        private readonly View _parent;
        internal BlurViewCanvas(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }

        internal BlurViewCanvas(View parent, Bitmap bitmap) : base(bitmap)
        {
            _parent = parent;
        }

        internal void GetLocationOnScreen(int[]? outLocation)
            => _parent.GetLocationOnScreen(outLocation);
    }
}