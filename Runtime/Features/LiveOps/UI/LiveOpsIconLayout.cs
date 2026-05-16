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
        private Transform _offerButton;
        private bool _hasIconBeforeOffer = false;

        public void Add(LiveOpsIcon icon)
        {
            var index = GetPlacementIndex(icon.Name);
            if (index >= _firstCommonIndex)
                _generalIcons.Add(icon);
            icon.transform.SetParent(_placements[index], false);
            icon.OnFinished.Subscribe(_ => Remove(icon)).AddTo(_disposables);

            if (index == PreOfferButtonIndex)
                _hasIconBeforeOffer = true;

            RefreshOfferButton();
        }

        public void Remove(LiveOpsIcon icon)
        {
            Destroy(icon.gameObject);

            if (!_generalIcons.Remove(icon))
            {
                if (GetPlacementIndex(icon.Name) == PreOfferButtonIndex)
                    _hasIconBeforeOffer = false;
                RefreshOfferButton();
                return;
            }

            for (var i = 0; i < _generalIcons.Count; i++)
                _generalIcons[i].transform.SetParent(_placements[_firstCommonIndex + i], false);
        }

        public void AddOfferButton(Transform offerButton)
        {
            _offerButton = offerButton;
            RefreshOfferButton();
        }

        private int PreOfferButtonIndex => GetPlacementIndex("OfferButton") - 1;

        private void RefreshOfferButton()
        {
            var offerIndex = GetPlacementIndex("OfferButton");
            var index = _hasIconBeforeOffer ? offerIndex : offerIndex - 1;
            _offerButton.SetParent(_placements[index], false);
        }

        private int GetPlacementIndex(string name)
        {
            if (_reservedPlacements.TryGetValue(name, out var index))
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
