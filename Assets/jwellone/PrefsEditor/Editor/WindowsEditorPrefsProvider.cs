#if UNITY_EDITOR_WIN
using System.Text;
using Microsoft.Win32;

#nullable enable

namespace jwelloneEditor
{
    public sealed class WindowsEditorPrefsProvider : EditorPrefProvider
    {
        public override string filePath => "Software\\Unity Technologies\\Unity Editor 5.x\\";
        public override bool showOpenFinder => false;

        protected override void OnInitialize()
        {
            using var regKey = Registry.CurrentUser.OpenSubKey(filePath, false);
            if (regKey == null)
            {
                return;
            }

            foreach (var valueName in regKey.GetValueNames())
            {
                var key = valueName.ToString();
                key = key.Substring(0, key.LastIndexOf("_h"));

                var value = regKey.GetValue(valueName);
                var str = regKey.GetValueKind(valueName) == RegistryValueKind.Binary
                    ? $"\"{Encoding.UTF8.GetString((byte[])regKey.GetValue(valueName))}\""
                    : value.ToString();

                AddEntity(key, str);
            }
        }
    }
}
#endif