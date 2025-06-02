using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEL.GraphTool.Windows
{
    using Utilities;

    public class GTEditorWindow : EditorWindow
    {
        private GTGraphView graphView;

        private Label filenameLabel;
        private Button saveButton;
        private Button saveAsButton;
        private Button miniMapButton;
        private string savedFilename = "";

        private GTComponentGraph componentGraph = null;

        [MenuItem("Tools/TEL/Graph Tool")]
        public static void Open()
        {
            GetWindow<GTEditorWindow>("Graph Tool");
        }

        private void OnEnable()
        {
            this.minSize = GTSettings.DefaultWindowMinSize;

            AddGraphView();
            AddToolbar();

            AddStyles();

            if (Application.isPlaying)
            {
                SubscribeToComponentGraph();
            }
            else if (savedFilename != "")
            {
                LoadAsset(savedFilename);
            }

            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            UnsubscribeFromComponentGraph();
        }

        private void AddGraphView()
        {
            graphView = new GTGraphView(this);

            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            filenameLabel = GTElementUtility.CreateLabel("Asset: ");

            saveButton = GTElementUtility.CreateButton("Save", () => Save());
            saveAsButton = GTElementUtility.CreateButton("Save As", () => SaveAs());

            Button loadButton = GTElementUtility.CreateButton("Load", () => Load());
            Button loadFromSceneButton = GTElementUtility.CreateButton("Load from scene", () => LoadSceneNodes());
            Button clearButton = GTElementUtility.CreateButton("Clear", () => Clear());

            miniMapButton = GTElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            saveButton.SetEnabled(false);

            toolbar.Add(filenameLabel);
            toolbar.Add(saveButton);
            toolbar.Add(saveAsButton);
            toolbar.Add(loadButton);
            toolbar.Add(loadFromSceneButton);
            toolbar.Add(clearButton);

            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("GTToolbarStyles");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("GTVariables");
        }

        private void Save()
        {
            if (CheckSaveError())
            {
                return;
            }

            if (string.IsNullOrEmpty(savedFilename))
            {
                EditorUtility.DisplayDialog("Save error.", "No filepath to save.", "Okay!");

                return;
            }

            GTIOUtility.Initialize(graphView);
            GTIOUtility.Save(savedFilename);
        }

        private void SaveAs()
        {
            if (CheckSaveError())
            {
                return;
            }

            string filepath = EditorUtility.SaveFilePanel("Save editor graph asset file + runtime asset", GTIOUtility.EditorAssetPath, "", "asset");

            if (string.IsNullOrEmpty(filepath))
            {
                return;
            }

            GTIOUtility.Initialize(graphView);
            GTIOUtility.Save(filepath);

            UpdateFileName(filepath);
        }

        private bool CheckSaveError()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Application is playing.", "Cannot save while the application is playing.", "Okay!");

                return true;
            }

            return false;
        }

        private void Load()
        {
            string filepath = EditorUtility.OpenFilePanel("Graphs", GTIOUtility.EditorAssetPath, "asset");

            if (string.IsNullOrEmpty(filepath))
            {
                return;
            }

            LoadAsset(filepath);
        }

        private void LoadAsset(string filename)
        {
            Clear();

            GTIOUtility.Initialize(graphView);
            var hasLoaded = GTIOUtility.Load(filename, componentGraph?.NodeGraph);

            if (hasLoaded)
            {
                UpdateFileName(filename);
                UpdateSceneNodes(new List<string>());
            }
        }

        private void LoadSceneNodes()
        {
            graphView.UpdateNodesFromScene();
        }

        private void UpdateSceneNodes(List<string> nodeIDs)
        {
            graphView.UpdateNodesFromScene(nodeIDs);
        }

        private void Clear()
        {
            UpdateFileName("");

            graphView.ClearGraph();
        }

        private void ClearGraph()
        {
            graphView.ClearGraph();
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();

            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        public void UpdateFileName(string filename)
        {
            savedFilename = filename;
            filenameLabel.text = "Asset: " + Path.GetFileName(filename);

            saveButton.SetEnabled(!string.IsNullOrEmpty(filename));
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
            saveAsButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
            saveAsButton.SetEnabled(false);
        }

        private void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SubscribeToComponentGraph();
                return;
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                UnsubscribeFromComponentGraph();
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                UpdateSceneNodes(new List<string>());
                return;
            }
        }

        private void SubscribeToComponentGraph()
        {
            UnsubscribeFromComponentGraph();

            componentGraph = FindObjectOfType<GTComponentGraph>();
            if (componentGraph != null)
            {
                componentGraph.OnNodesUpdated += UpdateSceneNodes;

                if (savedFilename != "")
                {
                    LoadAsset(savedFilename);
                }
            }
        }

        private void UnsubscribeFromComponentGraph()
        {
            if (componentGraph != null)
            {
                componentGraph.OnNodesUpdated -= UpdateSceneNodes;
                componentGraph = null;
            }
        }
    }
}