#if UNITY_EDITOR_WIN
using System.Text;
using UnityEditor;
using Microsoft.Win32;

#nullable enable

namespace jwelloneEditor
{
    public sealed class WindowsPlayerPrefsProvider : PlayerPrefsProvider
    {
        public override string filePath => $"Software\\Unity\\UnityEditor\\{PlayerSettings.companyName}\\{PlayerSettings.productName}";
        public override bool showOpenFinder => false;

        public override void Initialize()
        {
            base.Initialize();

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