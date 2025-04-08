using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Level
{
    [CreateAssetMenu(menuName = "TapEmpire/LevelsTable", fileName = "LevelsTable")]
    public class LevelsTable : ScriptableObject
    {
        [SerializeField]
        private List<LevelSettings> _levels;

        public List<LevelSettings> Levels => _levels;
    }
}