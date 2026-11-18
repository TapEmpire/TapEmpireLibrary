#if TEL_METICA
using Metica.Ads;

namespace TapEmpire.Services
{
    public static class MeticaAdPositionExtensions
    {
        public static MeticaAdViewPosition ToMeticaPosition(this AdPosition position)
        {
            return position switch
            {
                AdPosition.TopLeft => MeticaAdViewPosition.TopLeft,
                AdPosition.TopCenter => MeticaAdViewPosition.TopCenter,
                AdPosition.TopRight => MeticaAdViewPosition.TopRight,
                AdPosition.CenterLeft => MeticaAdViewPosition.CenterLeft,
                AdPosition.Center => MeticaAdViewPosition.Centered,
                AdPosition.CenterRight => MeticaAdViewPosition.CenterRight,
                AdPosition.BottomLeft => MeticaAdViewPosition.BottomLeft,
                AdPosition.BottomCenter => MeticaAdViewPosition.BottomCenter,
                AdPosition.BottomRight => MeticaAdViewPosition.BottomRight,
                _ => MeticaAdViewPosition.BottomCenter,
            };
        }
    }
}
#endif
