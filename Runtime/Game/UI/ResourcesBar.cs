using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class ResourcesBar : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private List<ResourceBarData> _data;
        [SerializeField] private bool _changePlusVisibilityOnMove = true;
        
        private int _counter = 0;

        public void MoveFront() => CheckMove(false);

        public void MoveBack() => CheckMove(true);

        private void CheckMove(bool isBack)
        {
            if (isBack && _counter == 0)
                Debug.LogWarning($"{nameof(ResourcesBar)}: MoveBack called without a matching MoveFront.", this);

            _counter = Mathf.Max(0, _counter + (isBack ? -1 : +1));
            ChangePosition(_counter == 0);
        }

        private void ChangePosition(bool isBack)
        {
            foreach (var item in _data)
            {
                item.Button.enabled = isBack;
                if (_changePlusVisibilityOnMove)
                    Utility.Utility.SetActive(item.PlusElement, isBack);
            }

            _canvas.overrideSorting = !isBack;
        }
    }

    [System.Serializable]
    public struct ResourceBarData
    {
        public Button Button;
        public GameObject PlusElement;
    }
}
