using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class RaycastUICoreSystem : Initializable, IRaycastUICoreSystem, ITickable
    {
        [SerializeField] private string _raycastTag = string.Empty;

        private IInputCoreSystem _inputCoreSystem;
        private ILevelExecutionCoreSystem _levelExecutionCoreSystem;

        private GraphicRaycaster _raycaster;
        private IDisposable _subscription;
        private IEnumerable<RaycastResult> _raycastResults = null;

        public IEnumerable<RaycastResult> RaycastHitUI
        {
            get
            {
                if (_raycastResults == null)
                {
                    _raycastResults = DoRaycast(_raycastTag);
                }

                return _raycastResults;
            }
        }

        [Inject]
        private void Construct(IInputCoreSystem inputCoreSystem, ILevelExecutionCoreSystem levelExecutionCoreSystem)
        {
            _inputCoreSystem = inputCoreSystem;
            _levelExecutionCoreSystem = levelExecutionCoreSystem;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _subscription = _levelExecutionCoreSystem.ExecutionData.Subscribe(OnUpdateExecutionData);
            return base.OnInitializeAsync(cancellationToken);
        }

        private void OnUpdateExecutionData(LevelExecutionData levelExecutionData)
        {
            if (levelExecutionData != null)
            {
                var canvas = levelExecutionData.LevelView.GetLevelReference<Canvas>("Canvas");
                _raycaster = canvas.GetComponent<GraphicRaycaster>();
            }
        }

        protected override void OnRelease()
        {
            _subscription.Dispose();
        }

        public IEnumerable<RaycastResult> RaycastHitTaggedUI(string tag)
        {
            return DoRaycast(tag);
        }

        private IEnumerable<RaycastResult> DoRaycast(string tag)
        {
            var raycastResults = new List<RaycastResult>();
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = _inputCoreSystem.InputPosition;
            _raycaster.Raycast(pointerData, raycastResults);
            return raycastResults.Where(result => result.gameObject.CompareTag(tag));
        }

        public void Tick()
        {
            _raycastResults = null;
        }
    }
}
