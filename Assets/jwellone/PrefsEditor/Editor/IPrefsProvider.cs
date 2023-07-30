using System.Collections.Generic;

#nullable enable

namespace jwelloneEditor
{
    public interface IPrefsProvider
    {
        bool showOpenFinder { get; }
        bool isDirty { get; }
        string filePath { get; }
        IReadOnlyList<PrefsEntity> entities { get; }

        void Initialize();
        void RevealInFinder();
        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        void SetString(string key, string value);
        void Delete(string key);
        void DeleteAll();
        void Save();
        void SaveDirtyIfNeed();
        void ConstGenerate();
    }
}