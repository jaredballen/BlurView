﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using SkiaSharp.Views.Forms;
using Xamarin.Forms.Internals;
using SKCanvasViewRenderer = BlurView.Droid.Renderers.SKCanvasViewRenderer;

namespace BlurView.Droid
{
    [Activity(
        Label = "BlurView",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait, 
        ConfigurationChanges = 
            ConfigChanges.ScreenSize | 
            ConfigChanges.Orientation | 
            ConfigChanges.UiMode
    )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            // This is a manual registration technique for custom renderers.
            // 
            Registrar.Registered.Register(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));

            LoadApplication(new App());
        }
    }
}