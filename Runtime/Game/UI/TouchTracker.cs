using System.Collections;
using System.Collections.Generic;
using System.Linq;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TapEmpire.UI
{
    public class TouchTracker : MonoBehaviour, IPointerDownHandler
    {
        public Subject<GameObject> OnDown = new();
        [SerializeField] bool _autofindOnStart = false;
        [SerializeField][ShowIf("@!_autofindOnStart")] List<TouchTracker> _children;

        private CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_autofindOnStart)
            {
                _children = GetComponentsInChildren<TouchTracker>().Where(tracker => tracker != this).ToList();
            }
            _children.ForEach(child => child.OnDown.Subscribe(OnPointerDown).AddTo(_disposables));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        public void OnPointerDown(PointerEventData eventData) => OnDown.OnNext(eventData.pointerCurrentRaycast.gameObject);
        private void OnPointerDown(GameObject gameObject) => OnDown.OnNext(gameObject);
    }
}
