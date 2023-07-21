using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace BlurView;

public partial class MainPage : ContentPage
{
    private const string SystemKey = "System";
    private const Views.BlurView.Materials SystemMaterial = Views.BlurView.Materials.System;
    
    private static readonly Dictionary<string, Views.BlurView.Materials> _materials = new()
    {
        { "Ultra Thin", Views.BlurView.Materials.UltraThin },
        { "Ultra Thin Light", Views.BlurView.Materials.UltraThinLight },
        { "Ultra Thin Dark", Views.BlurView.Materials.UltraThinDark },
                         
        { "Thin", Views.BlurView.Materials.Thin },
        { "Thin Light", Views.BlurView.Materials.ThinLight },
        { "Thin Dark", Views.BlurView.Materials.ThinDark },
                         
        { SystemKey, SystemMaterial },
        { "System Light", Views.BlurView.Materials.SystemLight },
        { "System Dark", Views.BlurView.Materials.SystemDark },
                         
        { "Thick", Views.BlurView.Materials.Thick },
        { "Thick Light", Views.BlurView.Materials.ThickLight },
        { "Thick Dark", Views.BlurView.Materials.ThickDark },
                         
        { "Chrome", Views.BlurView.Materials.Chrome },
        { "Chrome Dark", Views.BlurView.Materials.ChromeDark },
        { "Chrome Light", Views.BlurView.Materials.ChromeLight },
    };
    
    public IList<string> Materials => _materials.Keys.AsEnumerable().ToList();
    
    #region SelectedMaterial Property
    public static readonly BindableProperty SelectedMaterialProperty = BindableProperty.Create(
        propertyName: nameof(SelectedMaterial),
        returnType: typeof(string),
        declaringType: typeof(MainPage),
        defaultValue: SystemKey,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, _, newValue) =>
        {
            var mainPage = (MainPage) bindable;
            var newSelectedMaterial = (string) newValue ?? SystemKey;
            
            mainPage.Material = _materials.TryGetValue(newSelectedMaterial, out var material)
                ? material
                : SystemMaterial;
        });

    public string SelectedMaterial
    {
        get => ( string )GetValue(SelectedMaterialProperty);
        set => SetValue(SelectedMaterialProperty, value);
    }
    #endregion
    
    #region Material Property
    public static readonly BindableProperty MaterialProperty = BindableProperty.Create(
        propertyName: nameof(Material),
        returnType: typeof(Views.BlurView.Materials),
        declaringType: typeof(MainPage),
        defaultValue: SystemMaterial,
        defaultBindingMode: BindingMode.OneWay);

    public Views.BlurView.Materials Material
    {
        get => ( Views.BlurView.Materials )GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }
    #endregion
    
    private static readonly ImageSource StarryMountainsImageSource = ImageSource.FromResource(
        "BlurView.Resources.Images.starry_mountains.jpg", typeof(MainPage).GetTypeInfo().Assembly);
    
    private static readonly ImageSource TownImageSource = ImageSource.FromResource(
        "BlurView.Resources.Images.town.jpg", typeof(MainPage).GetTypeInfo().Assembly);
    
    private static readonly ImageSource BigBenImageSource = ImageSource.FromResource(
        "BlurView.Resources.Images.big_ben.jpg", typeof(MainPage).GetTypeInfo().Assembly);
    
    private const string SolidInverseSystem = "Solid Inverse System";
    private const string SolidWhite = "Solid White";
    private const string SolidBlack = "Solid Black";
    private const string StarryMountains = "Starry Mountains";
    private const string Town = "Town";
    private const string BigBen = "Big Ben";

    private const string DefaultBackground = StarryMountains;
    private static readonly ImageSource DefaultBackgroundImageSource = StarryMountainsImageSource;
    
    public IList<string> Backgrounds => new List<string>
    {
        SolidInverseSystem,
        SolidWhite,
        SolidBlack,
        StarryMountains,
        Town,
        BigBen
    };
    
    #region SelectedBackgroundImageSource Property
    public static readonly BindableProperty SelectedBackgroundImageSourceProperty = BindableProperty.Create(
        propertyName: nameof(SelectedBackgroundImageSource),
        returnType: typeof(ImageSource),
        declaringType: typeof(MainPage),
        defaultValue: DefaultBackgroundImageSource,
        defaultBindingMode: BindingMode.TwoWay);

    public ImageSource? SelectedBackgroundImageSource
    {
        get => ( ImageSource? )GetValue(SelectedBackgroundImageSourceProperty);
        set => SetValue(SelectedBackgroundImageSourceProperty, value);
    }
    #endregion

    #region ShowBackgroundImage Property
    public static readonly BindableProperty ShowBackgroundImageProperty = BindableProperty.Create(
        propertyName: nameof(ShowBackgroundImage),
        returnType: typeof(bool),
        declaringType: typeof(MainPage),
        defaultValue: true,
        defaultBindingMode: BindingMode.TwoWay);

    public bool ShowBackgroundImage
    {
        get => ( bool )GetValue(ShowBackgroundImageProperty);
        set => SetValue(ShowBackgroundImageProperty, value);
    }
    #endregion

    #region SelectedBackgroundColor Property
    public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
        propertyName: nameof(SelectedBackgroundColor),
        returnType: typeof(Color),
        declaringType: typeof(MainPage),
        defaultValue: Color.Black,
        defaultBindingMode: BindingMode.TwoWay);

    public Color SelectedBackgroundColor
    {
        get => ( Color )GetValue(SelectedBackgroundColorProperty);
        set => SetValue(SelectedBackgroundColorProperty, value);
    }
    #endregion

    #region SelectedBackground Property
    public static readonly BindableProperty SelectedBackgroundProperty = BindableProperty.Create(
        propertyName: nameof(SelectedBackground),
        returnType: typeof(string),
        declaringType: typeof(MainPage),
        defaultValue: DefaultBackground,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (bindable, _, newValue) =>
        {
            var mainPage = (MainPage) bindable;
            var selectedBackground = (string) newValue;

            switch (selectedBackground)
            {
                case SolidInverseSystem:
                    mainPage.UpdateBackgroundColorFromOsAppTheme();
                    mainPage.SelectedBackgroundImageSource = null;
                    break;
                case SolidWhite:
                    mainPage.SelectedBackgroundColor = Color.White;
                    mainPage.SelectedBackgroundImageSource = null;
                    break;
                case SolidBlack:
                    mainPage.SelectedBackgroundColor = Color.Black;
                    mainPage.SelectedBackgroundImageSource = null;
                    break;
                case Town:
                    mainPage.SelectedBackgroundColor = Color.Black;
                    mainPage.SelectedBackgroundImageSource = TownImageSource;
                    break;
                case BigBen:
                    mainPage.SelectedBackgroundColor = Color.Black;
                    mainPage.SelectedBackgroundImageSource = BigBenImageSource;
                    break;
                case StarryMountains:
                default:
                    mainPage.SelectedBackgroundColor = Color.Black;
                    mainPage.SelectedBackgroundImageSource = StarryMountainsImageSource;
                    break;
            };
        });

    public string SelectedBackground
    {
        get => ( string )GetValue(SelectedBackgroundProperty);
        set => SetValue(SelectedBackgroundProperty, value);
    }
    #endregion
    
    public MainPage()
    {
        SelectedBackgroundImageSource = DefaultBackgroundImageSource;
        InitializeComponent();
        BindingContext = this;
        Application.Current.RequestedThemeChanged += (_, _) => UpdateBackgroundColorFromOsAppTheme();
    }

    private void UpdateBackgroundColorFromOsAppTheme()
    {
        if (SelectedBackground != SolidInverseSystem) return;
        SelectedBackgroundColor = Application.Current.RequestedTheme == OSAppTheme.Dark
            ? Color.White
            : Color.Black;
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

public class AndMultiConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || !targetType.IsAssignableFrom(typeof(bool)))
            return false;

        foreach (var value in values)
        {
            if (value is not bool b)
                return false;

            if (b) continue;
            
            return false;
        }
        
        return true;
    }

    public object?[]? ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        if (value is not bool b || targetTypes.Any(t => !t.IsAssignableFrom(typeof(bool))))
            return null;

        return b ? targetTypes.Select(t => (object)true).ToArray() : null;
    }
}