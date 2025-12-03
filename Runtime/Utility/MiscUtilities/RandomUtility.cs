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

    public class BoxRandomizer<T>
    {
        private List<T> _elements;
        private List<T> _currentElements = new();
        private bool _withShuffle = true;

        public List<T> CurrentElements => _currentElements;

        public BoxRandomizer(List<T> elements, List<T> currentElements = null, bool withShuffle = true)
        {
            _elements = elements;
            _withShuffle = withShuffle;
            _currentElements = currentElements ?? FillElements();
        }

        public T GetRandomElement()
        {
            if (_currentElements.Count == 0)
            {
                FillElements();
            }

            return _currentElements.Pop();
        }

        public void ReturnToPool(T element)
        {
            _currentElements.Insert(0, element);
        }

        private List<T> FillElements()
        {
            _currentElements.Clear();
            _currentElements.AddRange(_elements);
            if (_withShuffle)
            {
                _currentElements.Shuffle();
            }
            return _currentElements;
        }
    }
}