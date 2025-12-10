using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class ResourcesBar : MonoBehaviour
    {
        [SerializeField] Canvas _canvas;
        [SerializeField] List<ResourceBarData> _data;

        private int _counter = 0;

        public void MoveFront() => CheckMove(false);

        public void MoveBack() => CheckMove(true);

        private void CheckMove(bool isBack)
        {
            _counter += isBack ? -1 : +1;
            ChangePosition(_counter == 0);
        }

        private void ChangePosition(bool isBack)
        {
            foreach (var item in _data)
            {
                item.Button.enabled = isBack;
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
