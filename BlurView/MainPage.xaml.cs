using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace BlurView
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void SKCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var size = e.Info.Size;
            
            var center = new SKPoint(size.Width * 0.5f, size.Height * 0.5f);
            var radius = Math.Min(size.Width, size.Height) / 2;
                
            canvas.Clear();
            canvas.DrawCircle(center, radius, new SKPaint()
            {
                IsAntialias = false,
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.Blue,
            });
        }
    }
}