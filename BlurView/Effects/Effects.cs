using Xamarin.Forms;

namespace BlurView.Effects;

public static class Effects
{
    public const string ResolutionGroupName = "BlurView.Efffects";

    public static string ResolveEffectName<TEffect>() where TEffect : RoutingEffect
        => $"{ResolutionGroupName}.{typeof(TEffect).Name}";
}