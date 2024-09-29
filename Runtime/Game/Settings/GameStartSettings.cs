using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Settings
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/GameStartSettings", fileName = "GameStartSettings")]
    public class GameStartSettings : ScriptableObject
    {
        [SerializeField]
        private bool _debug;

        [SerializeField, ShowIf(nameof(Debug))]
        private bool _autoRestartLevel;

        [SerializeField, ShowIf(nameof(Debug))]
        private bool _skipInters;

        [SerializeField, ShowIf(nameof(Debug))]
        private bool _editorStartFromPrefLevel;
        
        [SerializeField, ShowIf(nameof(Debug)), HideIf(nameof(_editorStartFromPrefLevel))]
        private int _editorEditorDebugStartLevelIndexIndex = -1;

        public bool Debug
        {
            get => _debug;
            set => _debug = value;
        }

        public bool AutoRestartLevel
        {
            get => _autoRestartLevel;
            set => _autoRestartLevel = value;
        }

        public bool SkipInters
        {
            get => _skipInters;
            set => _skipInters = value;
        }

        public int EditorDebugStartLevelIndex
        {
            get => _editorEditorDebugStartLevelIndexIndex;
            set => _editorEditorDebugStartLevelIndexIndex = value;
        }

        public bool EditorStartFromPrefLevel
        {
            get => _editorStartFromPrefLevel;
            set => _editorStartFromPrefLevel = value;
        }
    }
}