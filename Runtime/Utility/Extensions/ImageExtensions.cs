using UnityEngine.UI;

namespace TapEmpire.Utility
{
    public static class ImageExtensions
    {
        public static void SetAlpha(this Image self, float alpha)
        {
            var color = self.color;
            color.a = alpha;
            self.color = color;
        }
    }
}