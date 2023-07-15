using Xamarin.Forms;

namespace BlurView;

public class BlurView : ContentView
{
    public enum MaterialThicknesses : byte
    {
        UltraThin = 0x00,
        Thin      = 0x10,
        System    = 0x20,
        Thick     = 0x40,
        Chrome    = 0x80, 
    }
    
    public enum MaterialColors : byte
    {
        Dynamic = 0x00,
        Light   = 0x01,
        Dark    = 0x02,
    }
    
    public enum Materials : byte
    {
        UltraThin      = MaterialThicknesses.UltraThin | MaterialColors.Dynamic,
        UltraThinLight = MaterialThicknesses.UltraThin | MaterialColors.Light,
        UltraThinDark  = MaterialThicknesses.UltraThin | MaterialColors.Dark,
        Thin           = MaterialThicknesses.Thin      | MaterialColors.Dynamic,
        ThinLight      = MaterialThicknesses.Thin      | MaterialColors.Light,
        ThinDark       = MaterialThicknesses.Thin      | MaterialColors.Dark,
        System         = MaterialThicknesses.System    | MaterialColors.Dynamic,
        SystemLight    = MaterialThicknesses.System    | MaterialColors.Light,
        SystemDark     = MaterialThicknesses.System    | MaterialColors.Dark,
        Thick          = MaterialThicknesses.Thick     | MaterialColors.Dynamic,
        ThickLight     = MaterialThicknesses.Thick     | MaterialColors.Light,
        ThickDark      = MaterialThicknesses.Thick     | MaterialColors.Dark,
        Chrome         = MaterialThicknesses.Chrome    | MaterialColors.Dynamic,
        ChromeLight    = MaterialThicknesses.Chrome    | MaterialColors.Light,
        ChromeDark     = MaterialThicknesses.Chrome    | MaterialColors.Dark,
    }
    
    #region Material Property
    public static readonly BindableProperty MaterialProperty = BindableProperty.Create(
        propertyName: nameof(Material),
        returnType: typeof(Materials),
        declaringType: typeof(BlurView),
        defaultValue: Materials.System,
        defaultBindingMode: BindingMode.OneWay);

    public Materials Material
    {
        get => (Materials) GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }
    #endregion
}