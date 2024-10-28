using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TapEmpire.Utility
{
    public class FalseRandomizerInt
    {
        private List<int> _chances = null;
        private List<int> _currentChances = null;

        public List<int> CurrentChances => _currentChances; 

        public FalseRandomizerInt(List<int> chances, List<int> currentChances)
        {
            _chances = chances.ToList();
            _currentChances = currentChances ?? _chances.ToList();
        }

        public int GetNextRandomIndex()
        {
            var total = _currentChances.Sum();

            if (total == 0)
            {
                _chances.ForEachIndexed((chance, index) => _currentChances[index] = chance);
                total = _currentChances.Sum();
            }

            var finalIndex = GetRandomIndex(_currentChances, total);
            _currentChances[finalIndex] -= 1;

            return finalIndex;
        }

        private int GetRandomIndex(List<int> probabilities, int total)
        {
            var randomValue = Random.Range(0, total);

            int cumulativeWeight = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                cumulativeWeight += probabilities[i];
                if (randomValue < cumulativeWeight)
                {
                    return i;
                }
            }

            return probabilities.FindLastIndex(probability => probability > 0);
        }
    }

    public class FalseRandomizerIntSimple
    {
        private List<int> _chances = null;
        private List<int> _indices = new();

        public FalseRandomizerIntSimple(List<int> chances)
        {
            _chances = chances.ToList();
            FillIndices();
        }

        public int GetNextRandomIndex()
        {
            if (_indices.Count == 0)
            {
                FillIndices();
            }

            var index = Random.Range(0, _indices.Count);
            var result = _indices[index];

            _indices.RemoveAt(index);

            return result;
        }

        private void FillIndices()
        {
            _indices.Clear();
            _chances.ForEachIndexed((chance, index) => _indices.AddRange(Enumerable.Repeat(index, chance)));
        }
    }
}