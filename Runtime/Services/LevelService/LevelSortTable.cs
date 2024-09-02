using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/LevelSortTable", fileName = "LevelSortTable")]
    public class LevelSortTable : ScriptableObject
    {
        public int[] Order = Array.Empty<int>();
    }
}