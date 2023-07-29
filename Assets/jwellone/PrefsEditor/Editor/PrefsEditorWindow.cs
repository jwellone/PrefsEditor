using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System;
using System.Linq;

#nullable enable

namespace jwelloneEditor
{
    public class PrefsEditorWindow : EditorWindow
    {
        enum SearchType
        {
            Key,
            Value
        }

        enum SortType
        {
            Default,
            Ascending,
            Descending
        }

        class EmptyProvider : PlayerPrefsProvider
        {
            public override string filePath => string.Empty;
        }

        class AddPopup : PopupWindowContent
        {
            string _key = string.Empty;
            string _value = string.Empty;
            PlayerPrefsEntity.ValueType _valueType;

            bool isValid
            {
                get
                {
                    if (string.IsNullOrEmpty(_key) || _valueType == PlayerPrefsEntity.ValueType.None)
                    {
                        return false;
                    }

                    if (_valueType == PlayerPrefsEntity.ValueType.Number)
                    {
                        if (!int.TryParse(_value, out var iValue) && !float.TryParse(_value, out var fValue))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            public Action<string, PlayerPrefsEntity.ValueType, string>? addCallback { get; set; }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(384, 84);
            }

            public override void OnGUI(Rect rect)
            {
                _key = EditorGUILayout.TextField("key", _key);
                _value = EditorGUILayout.TextField("value", _value);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Value type");
                _valueType = (PlayerPrefsEntity.ValueType)EditorGUILayout.EnumPopup(_valueType, GUILayout.Width(226));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUI.enabled = isValid;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add", GUILayout.Width(42)))
                {
                    addCallback?.Invoke(_key, _valueType, _value);
                    editorWindow.Close();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        Vector2 _scrollPosition = Vector2.zero;
        ReorderableList? _reorderableList;
        GUIContent? _openIcon;
        GUIContent? _trashIcon;
        GUIContent? _saveIcon;
        GUIContent? _refreshIcon;
        GUIContent? _scriptIcon;
        GUIStyle? _guiStyleForValue;
        SearchType _searchType;
        SortType _sortType;
        string _searchFieldText = string.Empty;
        SearchField? _searchField;
        readonly List<PlayerPrefsEntity> _entities = new List<PlayerPrefsEntity>();

#if UNITY_EDITOR_OSX
        readonly PlayerPrefsProvider _provider = new MacPlayerPrefsProvider();
#elif UNITY_EDITOR_WIN
        readonly PlayerPrefsProvider _provider = new WindowsPlayerPrefsProvider();
#else
        readonly PlayerPrefsProvider _provider = new EmptyProvider();
#endif

        [MenuItem("jwellone/window/PrefsEditor")]
        static void Open()
        {
            var window = GetWindow<PrefsEditorWindow>();
            window.minSize = new Vector2(650f, 200f);
            window.titleContent = CreateIcon("d_SaveAs", "Can edit Prefs", "PlayerPrefs");
        }

        void OnEnable()
        {
            _openIcon = CreateIcon("d_FolderOpened Icon", "Open prefs");
            _trashIcon = CreateIcon("d_TreeEditor.Trash", "Delete prefs");
            _saveIcon = CreateIcon("d_SaveAs", "Save prefs");
            _refreshIcon = CreateIcon("d_TreeEditor.Refresh", "Reload prefs");
            _scriptIcon = CreateIcon("d_cs Script Icon", "Output constant script");

            _searchField = new SearchField();
        }

        void OnGUI()
        {
            if (_reorderableList == null)
            {
                _provider.Initialize();
                _reorderableList = CreateReorderableList();
            }

            GUILayout.BeginHorizontal();

            SetHeaderParam(
                (SearchType)EditorGUILayout.EnumPopup(_searchType, GUILayout.Width(64)),
                (SortType)EditorGUILayout.EnumPopup(_sortType, GUILayout.Width(96)),
                _searchField!.OnToolbarGUI(_searchFieldText));

            if(_provider.showOpenFinder)
            {
                if (GUILayout.Button(_openIcon, GUILayout.Width(32), GUILayout.Height(20)))
                {
                    _provider.RevealInFinder();
                }
            }

            if (GUILayout.Button(_trashIcon, GUILayout.Width(32)))
            {
                if (EditorUtility.DisplayDialog("PlayerPrefs", "Delete?", "ok", "cancel"))
                {
                    _provider.DeleteAll();
                    RefreshEntities();
                }
            }

            GUI.enabled = _provider.isDirty && !_entities.Any(e => !e.isValid);
            if (GUILayout.Button(_saveIcon, GUILayout.Width(32)))
            {
                _provider.SaveDirtyIfNeed();
            }
            GUI.enabled = true;

            if (GUILayout.Button(_refreshIcon, GUILayout.Width(32)))
            {
                _reorderableList = null;
            }

            if (GUILayout.Button(_scriptIcon, GUILayout.Width(32), GUILayout.Height(20)))
            {
                _provider.ConstGenerate();
            }

            GUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _reorderableList?.DoLayoutList();
            EditorGUILayout.EndScrollView();

            for (var i = 0; i < _entities.Count; ++i)
            {
                var entity = _entities[i];
                if (!entity.isValid)
                {
                    EditorGUILayout.HelpBox($"data in key({entity.key}) is problem.", MessageType.Error);
                    return;
                }
            }

            if (_entities.Any(e => e.isDirty))
            {
                EditorGUILayout.HelpBox($"There are data changes. A save is required to apply the changes.", MessageType.Warning);
            }
        }

        ReorderableList CreateReorderableList()
        {
            RefreshEntities();
            return new ReorderableList(_entities, typeof(PlayerPrefsEntity), true, false, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var tmpColor = GUI.backgroundColor;
                    var entity = _entities[index];
                    var tmpWidth = rect.width;

                    rect.height *= 0.94f;

                    GUI.backgroundColor = entity.type == PlayerPrefsEntity.ValueType.Number ? Color.cyan : tmpColor;
                    rect.width = tmpWidth * 0.35f;
                    EditorGUI.SelectableLabel(rect, entity.key, EditorStyles.selectionRect);

                    _guiStyleForValue ??= new GUIStyle(EditorStyles.textField);
                    if (!entity.isValid)
                    {
                        _guiStyleForValue.normal.textColor = Color.red;
                    }
                    else if (entity.isDirty)
                    {
                        _guiStyleForValue.normal.textColor = Color.yellow;
                    }
                    else
                    {
                        _guiStyleForValue.normal.textColor = EditorStyles.textField.normal.textColor;
                    }

                    _guiStyleForValue.focused.textColor = _guiStyleForValue.normal.textColor;
                    rect.x += rect.width + 4;
                    rect.width = tmpWidth * 0.65f;
                    entity.value = EditorGUI.TextField(rect, entity.value, _guiStyleForValue);
                    GUI.backgroundColor = tmpColor;
                },
                onAddDropdownCallback = (rect, list) =>
                {
                    var content = new AddPopup();
                    content.addCallback = (key, valueType, value) =>
                    {
                        if (valueType == PlayerPrefsEntity.ValueType.Number)
                        {
                            if (int.TryParse(value, out var iValue))
                            {
                                _provider.SetInt(key, iValue);
                            }
                            else if (float.TryParse(value, out var fValue))
                            {
                                _provider.SetFloat(key, fValue);
                            }
                        }
                        else
                        {
                            _provider.SetString(key, value);
                        }

                        RefreshEntities();
                    };
                    rect.x -= content.GetWindowSize().x;
                    PopupWindow.Show(rect, content);

                },
                onRemoveCallback = (list) =>
                {
                    var entity = _entities[list.index];
                    _provider.Delete(entity.key);
                    RefreshEntities();
                }
            };
        }

        void SetHeaderParam(SearchType searchType, SortType sortType, string searchFiledText)
        {
            var isUpdate = _searchType != searchType || _sortType != sortType || _searchFieldText != searchFiledText;

            _searchType = searchType;
            _sortType = sortType;
            _searchFieldText = searchFiledText;

            if (isUpdate)
            {
                RefreshEntities();
            }
        }

        void RefreshEntities()
        {
            _entities.Clear();

            if (!string.IsNullOrEmpty(_searchFieldText))
            {
                if (_searchType == SearchType.Key)
                {
                    foreach (var entity in _provider.entities)
                    {
                        if (entity.key.Contains(_searchFieldText))
                        {
                            _entities.Add(entity);
                        }
                    }
                }
                else
                {
                    foreach (var entity in _provider.entities)
                    {
                        if (entity.value.Contains(_searchFieldText))
                        {
                            _entities.Add(entity);
                        }
                    }
                }
            }
            else
            {
                _entities.AddRange(_provider.entities);
            }

            if (_sortType == SortType.Ascending)
            {
                if (_searchType == SearchType.Key)
                {
                    _entities.Sort((a, b) => string.Compare(a.key, b.key));
                }
                else
                {
                    _entities.Sort((a, b) => string.Compare(a.value, b.value));
                }
            }
            else if (_sortType == SortType.Descending)
            {
                if (_searchType == SearchType.Key)
                {
                    _entities.Sort((a, b) => string.Compare(b.key, a.key));
                }
                else
                {
                    _entities.Sort((a, b) => string.Compare(b.value, a.value));
                }
            }
        }

        static GUIContent CreateIcon(string fileName, string tooltip, string text = "")
        {
            return new GUIContent(text, EditorGUIUtility.IconContent(fileName).image, tooltip);
        }
    }
}