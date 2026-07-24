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
        protected List<LevelSettings> _levels;

        public List<LevelSettings> Levels => _levels;

        public IEnumerable<T> LevelsAs<T>() where T : LevelSettings => Levels.OfType<T>();
    }
}