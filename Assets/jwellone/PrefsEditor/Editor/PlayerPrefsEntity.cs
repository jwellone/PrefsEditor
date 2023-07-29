using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
    public sealed class PlayerPrefsEntity
    {
        public enum ValueType
        {
            None,
            Number,
            String
        }

        string _initialValue = string.Empty;

        public ValueType type = ValueType.None;
        public readonly string key = string.Empty;
        public string initialValue => _initialValue;
        public string value = string.Empty;
        public bool isDirty => _initialValue != value;
        public bool isValid
        {
            get
            {
                if (type == ValueType.None)
                {
                    return false;
                }
                else if (type == ValueType.Number)
                {
                    return int.TryParse(value, out var iValue) || float.TryParse(value, out var fValue);
                }

                return true;
            }
        }

        public PlayerPrefsEntity(string key, string value)
        {
            this.key = key;

            if (!PlayerPrefs.HasKey(key))
            {
                return;
            }

            if (int.TryParse(value, out var iValue))
            {
                type = ValueType.Number;
            }
            else if (float.TryParse(value, out var fValue))
            {
                type = ValueType.Number;
            }
            else
            {
                type = ValueType.String;
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Substring(1, value.Length - 2);
                }
            }

            _initialValue = value;
            this.value = value;
        }

        public PlayerPrefsEntity(string key, string value, ValueType type)
        {
            this.key = key;
            _initialValue = value;
            this.value = value;
            this.type = type;
        }

        public void Apply()
        {
            _initialValue = value;
        }
    }
}