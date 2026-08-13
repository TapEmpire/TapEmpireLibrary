using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TEL.Utilities;
using UnityEngine;

namespace TapEmpire.Level
{
    [CreateAssetMenu(menuName = "TapEmpire/LevelsTable", fileName = "LevelsTable")]
    public class LevelsTable : ScriptableObject
    {
        [SerializeField]
        protected List<LevelSettings> _levels;

        public List<LevelSettings> Levels => _levels;

        public IEnumerable<T> LevelsAs<T>() where T : LevelSettings => Levels.OfType<T>();

        [Button("Remove empty levels")]
        public void RemoveEmptyLevels()
        {
            _levels.RemoveAll(level => level == null);
            EditorTools.SetDirty(this);
        }
    }
}