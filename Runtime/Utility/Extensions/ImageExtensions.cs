using UnityEngine;
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

        public static float GetAlpha(this Image self)
        {
            return self.color.a;
        }
    }
}