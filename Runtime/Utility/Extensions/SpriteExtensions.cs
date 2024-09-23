using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.PSD;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class SpriteExtensions
    {
        private static readonly MethodInfo _generateOutlineMethodInfo = typeof(UnityEditor.Sprites.SpriteUtility).GetMethod("GenerateOutlineFromSprite", BindingFlags.NonPublic | BindingFlags.Static);

        public static Texture2D ToTexture2D(this Sprite self)
        {
            var texture = new Texture2D((int) self.rect.width, (int) self.rect.height);
            var pixels = self.texture.GetPixels((int) self.textureRect.x,
                (int) self.textureRect.y,
                (int) self.textureRect.width,
                (int) self.textureRect.height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Creates Vector2 path that outlines source sprite
        /// </summary>
        public static Vector2[][] GenerateTransparentOutline(Sprite sprite, float detail, byte alphaTolerance, bool detectHoles)
        {
            detail = Mathf.Pow(detail, 3);
            var parameters = new object[] {sprite, detail, alphaTolerance, detectHoles, null};
            _generateOutlineMethodInfo.Invoke(sprite, parameters);
            return (Vector2[][]) parameters[4];
        }
        
        public static Object GetSelectedObjectByExtension(string targetExtension, out string path)
        {
            Object result = null;
            path = string.Empty;

            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetFileName(path);
                    var extension = path.Split('.').Last();

                    if (targetExtension == extension)
                    {
                        result = obj;
                        path = AssetDatabase.GetAssetPath(obj);

                        break;
                    }
                }
            }

            return result;
        }

        public static float GetPixelsPerUnitMultiplier(string path)
        {
            var importer = (PSDImporter)PSDImporter.GetAtPath(path);
            
            return importer.spritePixelsPerUnit;
        }
        
        public static void SetPixelsPerUnitMultiplier(string path, float pixelsPerUnit)
        {
            var importer = (PSDImporter)PSDImporter.GetAtPath(path);

            importer.spritePixelsPerUnit = pixelsPerUnit;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
        
        public static void SetPsdDefaultSettings(string path)
        {
            var importer = (PSDImporter)PSDImporter.GetAtPath(path);

            importer.filterMode = FilterMode.Trilinear;
            
            var platformSettings = importer.GetImporterPlatformSettings(EditorUserBuildSettings.activeBuildTarget);
            platformSettings.maxTextureSize = 4096;
            importer.SetImporterPlatformSettings(platformSettings);
            
            var textureImporterSettings = (TextureImporterSettings)typeof(PSDImporter).GetField("m_TextureImporterSettings", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(importer);
            textureImporterSettings.alphaIsTransparency = true;
            typeof(PSDImporter).GetField("m_TextureImporterSettings", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(importer, textureImporterSettings);
            
            importer.mipmapEnabled = false;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}