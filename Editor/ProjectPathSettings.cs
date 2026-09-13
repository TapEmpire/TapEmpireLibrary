using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace TapEmpire.Build
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/ProjectPathSettings", fileName = "ProjectPathSettings")]
    public class ProjectPathSettings : ScriptableObject
    {
        public string DefaultGameFolder = "_Game";
        [ShowInInspector] public string DefaultGamePath => $"Assets/{DefaultGameFolder}";
        [ShowInInspector] public string DefaultLibraryAssetsPath => $"{DefaultGamePath}/_LibraryAssets";
        [ShowInInspector] public string DefaultScriptablesPath => $"{DefaultLibraryAssetsPath}/Scriptables";
        [ShowInInspector] public string DefaultServicesPath => $"{DefaultLibraryAssetsPath}/Services";
        [ShowInInspector] public string GameBuildSettingsPath => $"{DefaultScriptablesPath}/GameBuildSettings.asset";
        [ShowInInspector] public string GameSettingsPath => $"{DefaultScriptablesPath}/GameSettings.asset";
        [ShowInInspector] public string SystemSettingsPath => $"{DefaultScriptablesPath}/SystemSettings.asset";

        [Title("Project specific")]

        [ShowInInspector] public string ProjectScriptablesPath => $"{DefaultGamePath}/Scriptables";
        [ShowInInspector] public string ProjectArtPath => $"{DefaultGamePath}/Art";

        private static ProjectPathSettings _instance = null;

        public static ProjectPathSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    var guids = AssetDatabase.FindAssets("t:ProjectPathSettings");
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<ProjectPathSettings>(path);
                }

                return _instance;
            }
        }
    }
}