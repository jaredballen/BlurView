using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Renderscripts;
using Android.Util;
using Android.Views;
using BlurView.Droid.Extensions;
using Java.Lang.Reflect;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Element = Xamarin.Forms.Element;

namespace BlurView.Droid;

internal class BlurController : ViewOutlineProvider, IDisposable
{
    private readonly AcrylicViewRenderer _acrylicView;
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
    private Path? _viewPath;

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
            _acrylicView.PostInvalidate();
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
            _acrylicView.PostInvalidate();
        }
    }
        
    private float _elevation;
    public float Elevation
    {
        get => _elevation;
        set
        {
            if (_elevation == value) return;
            _elevation = value;
            _acrylicView.Elevation = DpToPixel(_elevation);
            _acrylicView.PostInvalidate();
        }
    }

    public BlurController(AcrylicViewRenderer acrylicView, View rootView) : base()
    {
        _acrylicView = acrylicView;
        _rootView = rootView;

        _acrylicView.OutlineProvider = this;
            
        _renderScript = RenderScript.Create(_acrylicView.Context);
        _blur = ScriptIntrinsicBlur.Create(_renderScript, Android.Renderscripts.Element.U8_4(_renderScript));
    }
        
    internal void OnPreDraw()
    {
        var contentDescription = _acrylicView.ContentDescription;
        
        if (_rootView.Width <= 0 || _rootView.Height <= 0)
            return;
        
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
            //_internalBlurredBitmap = Bitmap.CreateBitmap(_rootViewWidth, _rootViewHeight, Bitmap.Config.Argb8888);
            _internalBlurredBitmap = Bitmap.CreateBitmap((int)(_rootViewWidth * 0.25), (int)(_rootViewHeight * 0.25), Bitmap.Config.Argb8888);
            _internalCanvas = new AcrylicCanvas(_acrylicView, _internalBitmap)
            {
                Density = 0
            };
                
            try { _viewPath?.Dispose(); } catch { /* do nothing */ }
            _viewPath = new Path();
                
            _internalAllocation = Allocation.CreateFromBitmap(_renderScript, _internalBitmap);
            _internalBlurredAllocation = Allocation.CreateFromBitmap(_renderScript, _internalBlurredBitmap);
        }
            
        if (!_acrylicView.IsDirty)
            _acrylicView.PostInvalidate();
    }
    
    internal bool Draw(Canvas canvas)
    {
        //if (_isDrawing || _internalCanvas is null || ReferenceEquals(canvas, _internalCanvas)) return false;

        if (canvas is AcrylicCanvas otherAcrylicCanvas)
        {
            Log.Info("AcrylicView",$"Draw {_acrylicView.ContentDescription} onto {otherAcrylicCanvas.View.ContentDescription}");
        }

        if (_internalCanvas is null)
        {
            Log.Warn("AcrylicView", $"Canvas not initialized: {_acrylicView.ContentDescription}");
            return false;
        }

        if (ReferenceEquals(canvas, _internalCanvas))
        {
            Log.Warn("AcrylicView", $"Attempt tp draw to self: {_acrylicView.ContentDescription}");
            return false;
        }

        if (canvas is AcrylicCanvas otherAcrylicCanvas1)
    {
            var cd1 = _acrylicView.ContentDescription;
            var dc2 = otherAcrylicCanvas1.View.ContentDescription;

            var isOnTop = _acrylicView.Intersects(otherAcrylicCanvas1.View) &&
                          _acrylicView.Element.Above(otherAcrylicCanvas1.View.Element);
            
            if (_acrylicView.Intersects(otherAcrylicCanvas1.View))
            {
                
                var parent1 = _acrylicView.Element.Parent;
                var parents1 = new Stack<Element>();
                while (parent1 is not null)
                {
                    parents1.Push(parent1);
                    parent1 = parent1.Parent;
                }
                var parent2 = otherAcrylicCanvas1.View.Element.Parent;
                var parents2 = new Stack<Element>();
                while (parent2 is not null)
                {
                    parents2.Push(parent2);
                    parent2 = parent2.Parent;
                }

                if (!parents1.TryPeek(out parent1) || !parents2.TryPeek(out parent2) || !ReferenceEquals(parent1, parent2))
                    throw new Exception("View do NOT share root parent!?");
                
                while (parents1.TryPop(out parent1) && parents2.TryPop(out parent2))
                {
                    parents1.TryPeek(out var peekParent1);
                    parents2.TryPeek(out var peekParent2);

                    peekParent1 ??= _acrylicView.Element;
                    peekParent2 ??= otherAcrylicCanvas1.View.Element;
                    
                    var index1 = parent1?.LogicalChildren.IndexOf(peekParent1) ?? int.MaxValue;
                    var index2 = parent2?.LogicalChildren.IndexOf(peekParent2) ?? int.MaxValue;

                    if (index1 > index2)
                        return false;
                }
                
                var z1 = _acrylicView.GetZ();
                var z2 = otherAcrylicCanvas1.View.GetZ();
                Log.Warn("AcrylicView", $"Overlap: {_acrylicView.ContentDescription}, {otherAcrylicCanvas1.View.ContentDescription}");
            }
            else
            {
                Log.Warn("AcrylicView", $"No overlap: {_acrylicView.ContentDescription}, {otherAcrylicCanvas1.View.ContentDescription}");
                return false;
            }
        }
                
        // if (canvas is AcrylicCanvas otherAcrylicCanvas && !_acrylicView.Overlaps(otherAcrylicCanvas.View))
        //     return false;
        //
        if (_isDrawing)
            return false;
        
        try
        {
            _isDrawing = true;

            Log.Info("AcrylicView", $"Start Drawing: {_acrylicView.ContentDescription}");
            
            
            _rootView.GetLocationOnScreen(_rootViewLocation);
            _acrylicView.GetLocationOnScreen(_blurViewLocation);

            var left = _blurViewLocation[0] - _rootViewLocation[0];
            var top = _blurViewLocation[1] - _rootViewLocation[1];
            var width = _acrylicView.Width;
            var height = _acrylicView.Height;
                
            var offsetViewRect = new Rect((int)(left * 0.25), (int)(top * 0.25), (int)((left + width) * 0.25), (int)((top + height) * 0.25));
            var viewRect = new Rect(0, 0, width, height);

                
                
                
            var temp = (int)DpToPixel(16);
            var _ul = temp;
            var _ur = temp;
            var _lr = temp;
            var _ll = temp;
                
            _viewPath.Reset();
                
            // Create "rounded rect" path moving clock-wise starting at the top-left corner.
            //
            var (r0, r1) = GetNormalizedRadius(_ul, _ur, width);
            var first = r0;
            _viewPath.MoveTo(r0, 0);
            _viewPath.LineTo(width - r1, 0);

            (r0, r1) = GetNormalizedRadius(_ur, _lr, height);
            _viewPath.QuadTo(width, 0, width, r0);
            _viewPath.LineTo(width, height - r1);

            (r0, r1) = GetNormalizedRadius(_lr, _ll, width);
            _viewPath.QuadTo(width, height, width - r0, height);
            _viewPath.LineTo(r1, height);

            (r0, r1) = GetNormalizedRadius(_ll, _ul, height);
            _viewPath.QuadTo(0, height, 0, height - r0);
            _viewPath.LineTo(0, r1);

            _viewPath.QuadTo(0, 0, first, 0);
                
            canvas.Save();

            // This shadow support if too inefficient for Android. Instead we use ViewOutlineProvider
            // to allow Android to render the shadow for us.
            // 
            // if (Elevation > 0)
            // {
            //     var (alpha, dx, dy, radius, spread) = MaterialShadow.GetPenumbra(Elevation);
            //     DrawShadow(canvas, width, height, _viewPath, alpha, dx, dy, radius, spread);
            //
            //     (alpha, dx, dy, radius, spread) = MaterialShadow.GetAmbient(Elevation);
            //     DrawShadow(canvas, width, height, _viewPath, alpha, dx, dy, radius, spread);
            // }

            canvas.ClipPath(_viewPath, Region.Op.Intersect);
                
            _rootView.Draw(_internalCanvas);
            
            using var scaledBitmap = Bitmap.CreateScaledBitmap(_internalBitmap, (int)(_rootViewWidth * 0.25), (int)(_rootViewHeight * 0.25), true);
                
            _blur.SetRadius(BlurRadius);
            _blur.ForEach(_internalBlurredAllocation);
                
            using var allocation = Allocation.CreateFromBitmap(_renderScript, scaledBitmap);
            _blur.SetInput(allocation);
            //_blur.SetInput(_internalAllocation);
                
            _internalBlurredAllocation.CopyTo(_internalBlurredBitmap);
                
            canvas.DrawBitmap(_internalBlurredBitmap,
                src: offsetViewRect,
                dst: viewRect,
                paint: null);
                
            //canvas.DrawRect(viewRect, new Paint { Color = BackgroundColor });
                
            // TODO: Tie this into new enum like iOS for light/dark and blur radius
            // USe Black to Darken and White to Lighten. This works better than a ColorFilter on the Paint
            canvas.DrawRect(viewRect, new Paint { Color = Color.Black, Alpha = (int)(255 * 0.30) });
                
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
            
            Log.Info("AcrylicView", $"Finished Drawing: {_acrylicView.ContentDescription}");
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

    private double DpToPixel(double dp) => DpToPixel((float)dp);
    private float DpToPixel(float dp) => dp * _acrylicView.Context.Resources.DisplayMetrics.Density;
    private static Color ColorWithAlpha(Color color, int alpha) => new Color(color.R, color.G, color.B, alpha);
        
    private void DrawShadow(Canvas? canvas, float outlineWidth, float outlineHeight, Path outline, double alpha, double dx, double dy, double radius, double spread)
    {
        alpha = 255 * alpha;
        dx = DpToPixel(dx);
        dy = DpToPixel(dy);
        radius = DpToPixel(radius);
        spread = DpToPixel(spread);

        using var path = new Path(outline);
        if (spread > 0)
        {
            var sx = (float)((outlineWidth + spread) / outlineWidth);
            var sy = (float)((outlineHeight + spread) / outlineHeight);
            using var matrix = new Matrix();
            matrix.SetScale(sx, sy, outlineWidth / 2, outlineHeight / 2);
            path.Transform(matrix);
        }
            
        using var paint = new Paint();
        paint.AntiAlias = true;
        paint.Color = Color.Transparent;
        paint.SetShadowLayer((float)radius, (float)dx, (float)dy, ColorWithAlpha(Color.Black, (int)alpha));
        canvas.DrawPath(path, paint);
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
             
        try { _viewPath?.Dispose(); } catch { /* do nothing */ }
        _viewPath = new Path();
             
        try { _internalAllocation?.Dispose(); } catch { /* do nothing */ }
        _internalAllocation = null;
             
        try { _internalBlurredAllocation?.Dispose(); } catch { /* do nothing */ }
        _internalBlurredAllocation = null;

        _acrylicView.OutlineProvider = null;
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

    public override void GetOutline(View? view, Outline? outline)
    {
        view.ClipToOutline = true;
        outline.SetEmpty();
        if (_viewPath is not null)
            outline.SetPath(_viewPath);
    }
}