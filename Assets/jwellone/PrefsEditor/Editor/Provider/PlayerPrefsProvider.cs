using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
    public abstract class PlayerPrefsProvider : PrefsProvider
    {
        static readonly IPrefsConstGenerator _defaultConstGenerator = new PrefsConstGenerator("PlayerPrefsConst");
        static IPrefsConstGenerator _staticConstGenerator = _defaultConstGenerator;

        public static IPrefsConstGenerator staticConstGenerator
        {
            get => _staticConstGenerator;
            set
            {
                _staticConstGenerator = value;
                _staticConstGenerator ??= _defaultConstGenerator;
            }
        }

        protected sealed override IPrefsConstGenerator _constGenerator => staticConstGenerator;
        protected sealed override string[] _keysForIgnore => new[]
        {
            "unity.cloud_userid",
            "unity.player_session_count",
            "unity.player_sessionid",
            "UnityGraphicsQuality"
        };

        public sealed override void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            Save();
            AddEntity(key, value.ToString(), PrefsEntity.ValueType.Number);
        }

        public sealed override void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            Save();
            AddEntity(key, value.ToString(), PrefsEntity.ValueType.Number);
        }

        public sealed override void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            Save();
            AddEntity(key, value.ToString(), PrefsEntity.ValueType.String);
        }

        public sealed override void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            Save();
            DeleteEntity(key);
        }

        public sealed override void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            Save();
            DeleteEntities();
        }

        public sealed override void Save()
        {
            PlayerPrefs.Save();
        }
    }
}