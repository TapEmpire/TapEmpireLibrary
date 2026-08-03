using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TapEmpire.Editor
{
    public class FavoritesWindow : EditorWindow
    {
        [Serializable]
        private class Entry
        {
            public string GlobalId;
            public string Name;
        }

        [Serializable]
        private class State
        {
            public List<Entry> Entries = new List<Entry>();
        }

        private const string PrefsKeyPrefix = "TapEmpire.Favorites.";
        private const float RowHeight = 20f;

        private readonly Dictionary<string, UnityEngine.Object> _resolved = new Dictionary<string, UnityEngine.Object>();

        private State _state = new State();
        private GUIStyle _selectedRowStyle;
        private Vector2 _scroll;
        private int _selectedIndex = -1;

        private static string PrefsKey => PrefsKeyPrefix + Application.dataPath;

        [MenuItem("TapEmpire/Favorites")]
        private static void OpenWindow()
        {
            GetWindow<FavoritesWindow>("Favorites").Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Favorites", EditorGUIUtility.IconContent("Favorite").image);
            Load();
            EditorApplication.projectChanged += InvalidateCache;
            EditorApplication.hierarchyChanged += InvalidateCache;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= InvalidateCache;
            EditorApplication.hierarchyChanged -= InvalidateCache;
        }

        private void OnGUI()
        {
            if (_selectedRowStyle == null)
            {
                _selectedRowStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.white } };
            }

            HandleKeyboard();
            DrawRows();
            HandleDragAndDrop();
        }

        private void DrawRows()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < _state.Entries.Count; i++)
            {
                DrawRow(EditorGUILayout.GetControlRect(false, RowHeight), i);
            }
            EditorGUILayout.EndScrollView();

            if (_state.Entries.Count == 0)
            {
                EditorGUILayout.HelpBox("Drag assets or scene objects here.", MessageType.None);
            }
        }

        private void DrawRow(Rect rect, int index)
        {
            var entry = _state.Entries[index];
            var target = Resolve(entry);
            var isSelected = index == _selectedIndex;
            var current = Event.current;

            if (current.type == EventType.Repaint)
            {
                if (isSelected)
                {
                    var color = EditorGUIUtility.isProSkin
                        ? new Color(0.17f, 0.36f, 0.53f)
                        : new Color(0.24f, 0.48f, 0.90f);
                    EditorGUI.DrawRect(rect, color);
                }

                var content = target != null
                    ? new GUIContent(target.name, AssetPreview.GetMiniThumbnail(target))
                    : new GUIContent($"{entry.Name} (missing)");
                var labelRect = new Rect(rect.x + 4f, rect.y, rect.width - 4f, rect.height);
                (isSelected ? _selectedRowStyle : EditorStyles.label).Draw(labelRect, content, false, false, false, false);
            }

            if (target != null && target.name != entry.Name)
            {
                entry.Name = target.name;
                Save();
            }

            if (current.type != EventType.MouseDown || !rect.Contains(current.mousePosition))
            {
                return;
            }

            if (current.button == 1)
            {
                ShowRowMenu(index);
                current.Use();
                return;
            }

            _selectedIndex = index;
            if (target != null)
            {
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
                if (current.clickCount == 2)
                {
                    AssetDatabase.OpenAsset(target);
                }
            }
            current.Use();
            Repaint();
        }

        private void ShowRowMenu(int index)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"), false, () => Remove(index));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Remove All"), false, () =>
            {
                _state.Entries.Clear();
                _selectedIndex = -1;
                Save();
                Repaint();
            });
            menu.ShowAsContext();
        }

        private void HandleKeyboard()
        {
            var current = Event.current;
            if (current.type != EventType.KeyDown || _selectedIndex < 0)
            {
                return;
            }
            if (current.keyCode != KeyCode.Delete && current.keyCode != KeyCode.Backspace)
            {
                return;
            }
            Remove(_selectedIndex);
            current.Use();
        }

        private void HandleDragAndDrop()
        {
            var current = Event.current;
            if (current.type != EventType.DragUpdated && current.type != EventType.DragPerform)
            {
                return;
            }
            if (!new Rect(0f, 0f, position.width, position.height).Contains(current.mousePosition))
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var dragged in DragAndDrop.objectReferences)
                {
                    Add(dragged);
                }
                Save();
                Repaint();
            }
            current.Use();
        }

        private void Add(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
            if (_state.Entries.Exists(entry => entry.GlobalId == globalId))
            {
                return;
            }
            _state.Entries.Add(new Entry { GlobalId = globalId, Name = target.name });
        }

        private void Remove(int index)
        {
            if (index < 0 || index >= _state.Entries.Count)
            {
                return;
            }
            _state.Entries.RemoveAt(index);
            _selectedIndex = -1;
            Save();
            Repaint();
        }

        private UnityEngine.Object Resolve(Entry entry)
        {
            if (_resolved.TryGetValue(entry.GlobalId, out var cached) && cached != null)
            {
                return cached;
            }
            if (!GlobalObjectId.TryParse(entry.GlobalId, out var globalId))
            {
                return null;
            }
            var target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
            _resolved[entry.GlobalId] = target;
            return target;
        }

        private void InvalidateCache()
        {
            _resolved.Clear();
            Repaint();
        }

        private void Load()
        {
            var json = EditorPrefs.GetString(PrefsKey, string.Empty);
            _state = string.IsNullOrEmpty(json) ? new State() : JsonUtility.FromJson<State>(json) ?? new State();
            _resolved.Clear();
        }

        private void Save()
        {
            EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(_state));
        }
    }
}
