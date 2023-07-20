using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace BlurView;

public partial class MainPage : ContentPage
{
    private const string SystemKey = "System";
    private const Views.BlurView.Materials SystemMaterial = Views.BlurView.Materials.System;
    
    private Dictionary<string, Views.BlurView.Materials> _materials = new()
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
            
            mainPage.Material = mainPage._materials.TryGetValue(newSelectedMaterial, out var material)
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

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}