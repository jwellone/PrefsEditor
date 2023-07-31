using System;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
    public class AddEntityPopupWindowContent : PopupWindowContent
    {
        string _key = string.Empty;
        string _value = string.Empty;
        PrefsEntity.ValueType _valueType;

        bool isValid
        {
            get
            {
                if (string.IsNullOrEmpty(_key) || _valueType == PrefsEntity.ValueType.None)
                {
                    return false;
                }

                if (_valueType == PrefsEntity.ValueType.Number)
                {
                    if (!int.TryParse(_value, out var iValue) && !float.TryParse(_value, out var fValue))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public Action<string, PrefsEntity.ValueType, string>? addCallback { get; set; }

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
            _valueType = (PrefsEntity.ValueType)EditorGUILayout.EnumPopup(_valueType, GUILayout.Width(226));
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
}