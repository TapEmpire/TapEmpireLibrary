namespace TapEmpire.Experimental
{
    public static class MaxAdPositionExtensions
    {
        public static MaxSdkBase.AdViewPosition ToMaxPosition(this AdPosition position)
        {
            return position switch
            {
                AdPosition.TopLeft => MaxSdkBase.AdViewPosition.TopLeft,
                AdPosition.TopCenter => MaxSdkBase.AdViewPosition.TopCenter,
                AdPosition.TopRight => MaxSdkBase.AdViewPosition.TopRight,
                AdPosition.CenterLeft => MaxSdkBase.AdViewPosition.CenterLeft,
                AdPosition.Center => MaxSdkBase.AdViewPosition.Centered,
                AdPosition.CenterRight => MaxSdkBase.AdViewPosition.CenterRight,
                AdPosition.BottomLeft => MaxSdkBase.AdViewPosition.BottomLeft,
                AdPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                AdPosition.BottomRight => MaxSdkBase.AdViewPosition.BottomRight,
                _ => MaxSdkBase.AdViewPosition.BottomCenter,
            };
        }
    }
}
