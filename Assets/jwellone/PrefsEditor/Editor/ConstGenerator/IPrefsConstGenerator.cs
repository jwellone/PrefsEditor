using System.Collections.Generic;

#nullable enable

namespace jwelloneEditor
{
    public interface IPrefsConstGenerator
    {
        void Generate(IReadOnlyList<PrefsEntity> entities);
    }
}