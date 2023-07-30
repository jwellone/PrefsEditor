using System.Collections.Generic;

#nullable enable

namespace jwelloneEditor
{
    public static class PrefsProviderFactory
    {
        class EmptyProvider : PrefsProvider
        {
            class EmptyGenerator : IPrefsConstGenerator
            {
                void IPrefsConstGenerator.Generate(IReadOnlyList<PrefsEntity> entities) { }
            }

            public override string filePath => string.Empty;
            protected override IPrefsConstGenerator _constGenerator { get; } = new EmptyGenerator();
            protected override string[] _keysForIgnore { get; } = new string[0];
            public override void SetInt(string key, int value) { }
            public override void SetFloat(string key, float value) { }
            public override void SetString(string key, string value) { }
            public override void Delete(string key) { }
            public override void DeleteAll() { }
            public override void Save() { }
        }

        public static PrefsProvider[] Create()
        {
            return new[]
            {
                CreatePlayerPrefs(),
                CreateEditorPrefs()
            };
        }

        static PrefsProvider CreatePlayerPrefs()
        {
#if UNITY_EDITOR_OSX
            return new MacPlayerPrefsProvider();
#elif UNITY_EDITOR_WIN
            return new WindowsPlayerPrefsProvider();
#else
            return new EmptyProvider();
#endif
        }

        static PrefsProvider CreateEditorPrefs()
        {
#if UNITY_EDITOR_OSX
            return new MacEditorPrefsProvider();
#elif UNITY_EDITOR_WIN
            return new WindowsEditorPrefsProvider();
#else
            return new EmptyProvider();
#endif
        }
    }
}