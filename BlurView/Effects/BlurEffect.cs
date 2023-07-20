using Xamarin.Forms;

namespace BlurView.Effects;

public class BlurEffect : RoutingEffect
{
    internal BlurEffect() : base(Effects.ResolveEffectName<BlurEffect>()) { }
}