using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
    public abstract class PrefsProvider
    {
        readonly List<PrefsEntity> _entities = new List<PrefsEntity>();

        public virtual bool showOpenFinder => !string.IsNullOrEmpty(filePath);
        public bool isDirty => _entities.Any(e => e.isDirty);
        public abstract string filePath { get; }
        public IReadOnlyList<PrefsEntity> entities => _entities;

        protected abstract IPrefsConstGenerator _constGenerator { get; }
        protected abstract string[] _keysForIgnore { get; }

        public virtual void Initialize()
        {
            _entities.Clear();
        }

        public void RevealInFinder()
        {
            System.Diagnostics.Process.Start(filePath);
        }

        public abstract void SetInt(string key, int value);
        public abstract void SetFloat(string key, float value);
        public abstract void SetString(string key, string value);
        public abstract void Delete(string key);
        public abstract void DeleteAll();
        public abstract void Save();

        public void SaveDirtyIfNeed()
        {
            var isSave = false;
            foreach (var entity in entities)
            {
                if (!entity.isDirty)
                {
                    continue;
                }

                if (entity.type == PrefsEntity.ValueType.Number)
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

        protected void ApplyInitializeValue()
        {
            foreach(var entity in _entities)
            {
                entity.ApplyInitializeValue();
            }
        }

        protected void AddEntity(string key, string value)
        {
            if (_keysForIgnore.Any(ignore => ignore.Equals(key, System.StringComparison.Ordinal)) ||
                _entities.Any(e => e.key == key))
            {
                return;
            }

            _entities.Add(new PrefsEntity(key, value));
        }

        protected void AddEntity(string key, string value, PrefsEntity.ValueType type)
        {
            if (_keysForIgnore.Any(ignore => ignore.Contains(key)) ||
                _entities.Any(e => e.key == key))
            {
                return;
            }

            _entities.Add(new PrefsEntity(key, value, type));
        }

        protected void DeleteEntity(string key)
        {
            for (var i = _entities.Count - 1; i >= 0; --i)
            {
                if (_entities[i].key == key)
                {
                    _entities.RemoveAt(i);
                    return;
                }
            }
        }

        protected void DeleteEntities()
        {
            _entities.Clear();
        }
    }
}