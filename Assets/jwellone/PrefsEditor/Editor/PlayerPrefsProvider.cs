using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
    public abstract class PlayerPrefsProvider
    {
        static readonly string[] _keysForIgnore = new[]
        {
            "unity.cloud_userid",
            "unity.player_session_count",
            "unity.player_sessionid",
            "UnityGraphicsQuality"
        };

        static IPlayerPrefsConstGenerator _constGenerator = new PlayerPrefsConstGenerator();

        public static IPlayerPrefsConstGenerator constGenerator
        {
            get => _constGenerator;
            set => SetConstGenerator(value);
        }

        readonly List<PlayerPrefsEntity> _entities = new List<PlayerPrefsEntity>();

        public abstract string filePath { get; }
        public IReadOnlyList<PlayerPrefsEntity> entities => _entities;

        public bool isDirty => _entities.Any(e => e.isDirty);
        public virtual bool showOpenFinder => !string.IsNullOrEmpty(filePath);

        public virtual void RevealInFinder()
        {
            System.Diagnostics.Process.Start(filePath);

        }

        public virtual void Initialize()
        {
            _entities.Clear();
        }

        public bool Has(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            AddEntity(key, value.ToString(), PlayerPrefsEntity.ValueType.Number);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            AddEntity(key, value.ToString(), PlayerPrefsEntity.ValueType.Number);
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
            AddEntity(key, value.ToString(), PlayerPrefsEntity.ValueType.String);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            DeleteEntity(key);
            Save();
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            Save();
            _entities.Clear();
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }

        public void SaveDirtyIfNeed()
        {
            var isSave = false;
            foreach (var entity in _entities)
            {
                if (!entity.isDirty)
                {
                    continue;
                }

                if (entity.type == PlayerPrefsEntity.ValueType.Number)
                {
                    if (int.TryParse(entity.value, out var iValue))
                    {
                        SetInt(entity.key, iValue);
                    }
                    else if (float.TryParse(entity.value, out var fValue))
                    {
                        SetFloat(entity.key, fValue);
                    }
                    else
                    {
                        Debug.LogError($"({entity.key} , {entity.value}) for invalid.");
                        continue;
                    }
                }
                else
                {
                    SetString(entity.key, entity.value);
                }

                entity.Apply();

                isSave = true;
            }

            if (isSave)
            {
                Save();
            }
        }

        public void ConstGenerate()
        {
            _constGenerator.Generate(_entities);
        }

        protected void AddEntity(string key, string value)
        {
            if (_keysForIgnore.Any(ignore => ignore.Equals(key, System.StringComparison.Ordinal)) ||
                _entities.Any(e => e.key == key))
            {
                return;
            }


            _entities.Add(new PlayerPrefsEntity(key, value));
        }

        protected void AddEntity(string key, string value, PlayerPrefsEntity.ValueType type)
        {
            if (_keysForIgnore.Any(ignore => ignore.Contains(key)) ||
                _entities.Any(e => e.key == key))
            {
                return;
            }

            _entities.Add(new PlayerPrefsEntity(key, value, type));
        }

        protected void DeleteEntity(string key)
        {
            for (var i = _entities.Count - 1; i >= 0; --i)
            {
                if (_entities[i].key == key)
                {
                    PlayerPrefs.DeleteKey(key);
                    _entities.RemoveAt(i);
                    return;
                }
            }
        }

        public static void SetConstGenerator(IPlayerPrefsConstGenerator generator)
        {
            _constGenerator = generator == null ? new PlayerPrefsConstGenerator() : generator;
        }

        public static IPlayerPrefsConstGenerator GetConstGenerator()
        {
            return _constGenerator;
        }
    }
}