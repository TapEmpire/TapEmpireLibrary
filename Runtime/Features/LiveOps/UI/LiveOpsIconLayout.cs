using System.Collections.Generic;
using R3;
using UnityEngine;

namespace TapEmpire.LiveOps.UI
{
    public class LiveOpsIconLayout : MonoBehaviour
    {
        [SerializeField] private List<Transform> _placements;
        [SerializeField] private SerializableDictionary<string, int> _reservedPlacements = new();
        [SerializeField] private int _firstCommonIndex = 3;

        private readonly List<LiveOpsIcon> _generalIcons = new();
        private readonly CompositeDisposable _disposables = new();

        public void Add(LiveOpsIcon icon)
        {
            var index = GetPlacementIndex(icon);
            if (index >= _firstCommonIndex)
                _generalIcons.Add(icon);
            icon.transform.SetParent(_placements[index], false);
            icon.OnFinished.Subscribe(_ => Remove(icon)).AddTo(_disposables);
        }

        public void Remove(LiveOpsIcon icon)
        {
            icon.gameObject.SetActive(false);

            if (!_generalIcons.Remove(icon))
                return;

            for (var i = 0; i < _generalIcons.Count; i++)
                _generalIcons[i].transform.SetParent(_placements[_firstCommonIndex + i], false);
        }

        private int GetPlacementIndex(LiveOpsIcon icon)
        {
            if (_reservedPlacements.TryGetValue(icon.Name, out var index))
                return index;

            return _firstCommonIndex + _generalIcons.Count;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }

    public static class LiveOpsIconExtensions
    {
        public static void AddTo(this LiveOpsIcon icon, LiveOpsIconLayout layout)
        {
            layout.Add(icon);
        }
    }
}
