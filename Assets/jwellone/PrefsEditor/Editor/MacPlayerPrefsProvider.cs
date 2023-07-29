﻿#if UNITY_EDITOR_OSX
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
    public sealed class MacPlayerPrefsProvider : PlayerPrefsProvider
    {
        public override string filePath => Path.Combine(Environment.GetEnvironmentVariable("HOME"), $"Library/Preferences/unity.{PlayerSettings.companyName}.{PlayerSettings.productName}.plist");

        public override void Initialize()
        {
            base.Initialize();

            using var process = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = "plutil",
                Arguments = $"-p {filePath}",
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Application.dataPath
            });

            process.EnableRaisingEvents = true;

            var result = string.Empty;
            process.Exited += (sender, e) =>
            {
                result = process.StandardOutput.ReadToEnd().Trim();
            };

            process.WaitForExit();

            var rows = result.Split('\n');
            for (var i = 1; i < rows.Length - 1; ++i)
            {
                var columns = rows[i].Split(" => ");
                var key = columns[0].Replace("\"", "").Replace(" ", "");
                AddEntity(key, columns[1]);
            }
        }
    }
}
#endif