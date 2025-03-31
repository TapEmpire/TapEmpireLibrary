using System.Collections.Generic;
using UnityEngine;

namespace TEL.GraphTool
{
    public class GTSettings : ScriptableObject
    {
        [field: SerializeField] public string DefaultAssetFolder { get; set; } = "Assets/Resources";
        [field: SerializeField] public string DefaultEditorAssetFolder { get; set; } = "Assets/Editor/Resources";

        public static readonly Vector2 DefaultWindowMinSize = new Vector2(800.0f, 500.0f);
    }
}