using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TapEmpire.Level
{
    [CreateAssetMenu(menuName = "TapEmpire/LevelsTable", fileName = "LevelsTable")]
    public class LevelsTable : ScriptableObject
    {
        [SerializeField]
        private List<LevelSettings> _levels;

        public List<LevelSettings> Levels => _levels;

        public List<T> As<T>() where T : LevelSettings
        {
            return Levels.Select(level => level as T).ToList();
        }
    }
}