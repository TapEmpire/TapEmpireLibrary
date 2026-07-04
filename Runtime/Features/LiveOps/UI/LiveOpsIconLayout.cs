using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace TapEmpire.LiveOps.UI
{
    public class LiveOpsIconLayout : MonoBehaviour
    {
        [SerializeField] private Transform _specialPlacement;
        [SerializeField] private string _specialPlacementName;
        [SerializeField] private PriorityPlacement _left;
        [SerializeField] private PriorityPlacement _right;

        private readonly CompositeDisposable _disposables = new();

        public void Add(LiveOpsIcon icon)
        {
            AddItem(icon.Name, icon.transform);
            icon.OnFinished.Subscribe(_ => Remove(icon)).AddTo(_disposables);
        }

        public void Remove(LiveOpsIcon icon)
        {
            Destroy(icon.gameObject);
            if (!_left.Remove(icon.Name))
                _right.Remove(icon.Name);
        }

        public void AddOfferButton(Transform offerButton)
        {
            AddItem("OfferButton", offerButton);
        }

        private void AddItem(string name, Transform transform)
        {
            if (name == _specialPlacementName)
            {
                transform.SetParent(_specialPlacement, false);
                return;
            }

            if (!_left.Add(name, transform))
                _right.Add(name, transform);
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

    [Serializable]
    public class PriorityPlacement
    {
        [SerializeField] private List<Transform> _placements;
        [SerializeField] private List<string> _priorityOrder;

        private readonly List<(string name, Transform transform)> _activeItems = new();

        public bool Add(string name, Transform transform)
        {
            if (!_priorityOrder.Contains(name))
                return false;

            _activeItems.Add((name, transform));
            Relayout();
            return true;
        }

        public bool Remove(string name)
        {
            if (_activeItems.RemoveAll(entry => entry.name == name) == 0)
                return false;

            Relayout();
            return true;
        }

        private void Relayout()
        {
            _activeItems.Sort((firstItem, secondItem) =>
                _priorityOrder.IndexOf(firstItem.name).CompareTo(_priorityOrder.IndexOf(secondItem.name)));

            for (var i = 0; i < _activeItems.Count; i++)
                _activeItems[i].transform.SetParent(_placements[i], false);
        }
    }
}
