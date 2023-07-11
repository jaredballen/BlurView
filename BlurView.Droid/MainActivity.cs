using Android.App;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Platform;
using SkiaSharp.Views.Forms;
using Xamarin.Forms.Internals;
using SKCanvasViewRenderer = BlurView.Droid.Renderers.SKCanvasViewRenderer;

namespace BlurView.Droid
{
    [Activity(Label = "BlurView", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            
            CachedImageRenderer.Init(true);
            CachedImageRenderer.InitImageViewHandler();

            // Need to do this to register the new svg canvas view rendered for Android
            // 
            // Dependency.Register(typeof (FileImageSource), typeof (FFImageLoadingImageViewHandler));
            
            Registrar.Registered.Register(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));
        
            LoadApplication(new App());
        }
    }
}