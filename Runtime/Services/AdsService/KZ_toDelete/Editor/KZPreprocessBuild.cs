using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

class KZPreprocessBuild : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        DeleteJarFiles();
    }

    #region Delete Jar Conflicts
    static void DeleteJarFiles()
    {
        DeleteDependency("org.jetbrains.kotlinx.kotlinx-coroutines-android-1.4.3", ".jar");
        DeleteDependency("org.jetbrains.kotlinx.kotlinx-coroutines-core-1.7.1", ".jar");

        if (DependencyExists("org.jetbrains.kotlinx.kotlinx-coroutines-core-jvm-1.6.4", ".jar"))
            DeleteDependency("org.jetbrains.kotlinx.kotlinx-coroutines-core-jvm-1.7.1", ".jar");
    }

    static bool DependencyExists(string fileName, string fileExtension)
    {
        string filePath = Application.dataPath + "/Plugins/Android/" + fileName + fileExtension;
        return File.Exists(filePath);
    }

    static void DeleteDependency(string fileName, string fileExtension)
    {
        string filePath = Application.dataPath + "/Plugins/Android/" + fileName + fileExtension;

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            AssetDatabase.Refresh();
            Debug.Log($"{fileName} deleted successfully...");
        }
    }
    #endregion
}