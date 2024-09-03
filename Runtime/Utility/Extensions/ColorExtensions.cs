using UnityEngine;

namespace TapEmpire.Utility
{
    public static class ColorExtensions
    {
        public static Color RandomizeColor(this Color self, float darkenAmount = 0.1f, float lightenAmount = 0.1f)
        {
            var change = Random.Range(-darkenAmount, lightenAmount);
            var r = Mathf.Clamp(self.r + change, 0f, 1f);
            var g = Mathf.Clamp(self.g + change, 0f, 1f);
            var b = Mathf.Clamp(self.b + change, 0f, 1f);
            return new Color(r, g, b, self.a);
        }

        public static Color WithAlpha(this Color self, float alpha)
        {
            return new Color(self.r, self.g, self.b, alpha);
        }
    }
}