using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
    public abstract class EditorPrefProvider : PrefsProvider
    {
        static readonly IPrefsConstGenerator _defaultConstGenerator = new PrefsConstGenerator("EditorPrefsConst");
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

        bool _isInitializeCompleted;
        protected sealed override IPrefsConstGenerator _constGenerator => staticConstGenerator;

        protected override string[] _keysForIgnore { get; } = new string[0];

        public sealed override void Initialize()
        {
            if(_isInitializeCompleted)
            {
                ApplyInitializeValue();
                return;
            }

            base.Initialize();
            OnInitialize();
            _isInitializeCompleted = true;
        }

        public sealed override void SetInt(string key, int value)
        {
            EditorPrefs.SetInt(key, value);
            AddEntity(key, value.ToString(), PrefsEntity.ValueType.Number);
        }

        public sealed override void SetFloat(string key, float value)
        {
            EditorPrefs.SetFloat(key, value);
            AddEntity(key, value.ToString(), PrefsEntity.ValueType.Number);
        }

        public sealed override void SetString(string key, string value)
        {
            EditorPrefs.SetString(key, value);
            AddEntity(key, value.ToString(), PrefsEntity.ValueType.String);
        }

        public sealed override void Delete(string key)
        {
            EditorPrefs.DeleteKey(key);
            DeleteEntity(key);
        }

        public sealed override void DeleteAll()
        {
            EditorPrefs.DeleteAll();
            DeleteEntities();
        }

        public sealed override void Save()
        {
        }

        protected abstract void OnInitialize();
    }
}