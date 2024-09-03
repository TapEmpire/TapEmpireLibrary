using UnityEngine;

namespace TapEmpire.Utility
{
    public static class SpriteExtensions
    {
        public static Texture2D ToTexture2D(this Sprite self)
        {
            var texture = new Texture2D((int)self.rect.width, (int)self.rect.height);
            var pixels = self.texture.GetPixels((int)self.textureRect.x, 
                (int)self.textureRect.y, 
                (int)self.textureRect.width, 
                (int)self.textureRect.height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}