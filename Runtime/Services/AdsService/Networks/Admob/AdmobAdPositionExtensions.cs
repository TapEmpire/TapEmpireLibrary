using AdmobAdPosition = GoogleMobileAds.Api.AdPosition;

namespace TapEmpire.Services
{
    public static class AdmobAdPositionExtensions
    {
        public static AdmobAdPosition ToAdmobPosition(this AdPosition position)
        {
            return position switch
            {
                AdPosition.TopLeft => AdmobAdPosition.TopLeft,
                AdPosition.TopCenter => AdmobAdPosition.Top,
                AdPosition.TopRight => AdmobAdPosition.TopRight,
                AdPosition.CenterLeft => AdmobAdPosition.Center,
                AdPosition.Center => AdmobAdPosition.Center,
                AdPosition.CenterRight => AdmobAdPosition.Center,
                AdPosition.BottomLeft => AdmobAdPosition.BottomLeft,
                AdPosition.BottomCenter => AdmobAdPosition.Bottom,
                AdPosition.BottomRight => AdmobAdPosition.BottomRight,
                _ => AdmobAdPosition.Bottom,
            };
        }
    }
}
