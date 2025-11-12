using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Fonts
{
    [CreateAssetMenu(menuName = "TapEmpire/FontSettings", fileName = "FontSettings")]
    public class FontSettings : ScriptableObject
    {
        [field: SerializeField] public SerializableDictionary<Material, FontData> FontSpacings { get; private set; }

        [Button]
        private void FindAllFontMaterials(string tag)
        {
            var materials = FindAllTmpMaterials(tag);

            var dictionary = materials.ToDictionary(material => material, material => new FontData());
            FontSpacings = new SerializableDictionary<Material, FontData>(dictionary);
        }

        public static List<Material> FindAllTmpMaterials(string materialTag)
        {
            var materials = EditorCustomUtility.LoadAllAssetsOfType<Material>();
            return materials.FindAll(material =>
                material.name.Contains(materialTag) && IsTmpFontMaterial(material));
        }

        private static bool IsTmpFontMaterial(Material material)
        {
            if (material == null || material.shader == null) return false;

            var name = material.shader.name;
            return name.StartsWith("TextMeshPro/");
        }
    }

    [System.Serializable]
    public struct FontData
    {
        public float CharacterSpacing;
        public float WordSpacing;
    }
}
