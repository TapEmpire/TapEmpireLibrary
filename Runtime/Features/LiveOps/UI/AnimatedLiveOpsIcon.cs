using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using Spine.Unity;
using TapEmpire.Services;
using TapEmpire.Services.LiveOps;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.LiveOps.UI
{
    public class AnimatedLiveOpsIcon : LiveOpsIcon
    {
        private const float AnimationMultiplier = 1.0f;
        private const string IdleName = "Idle_Animation";
        private const string ActiveName = "Activate_Animation";

        [Header("Animations")]
        [SerializeField] private SkeletonGraphic _skeletonGraphic;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private List<Image> _images;

        [Header("Timer")]
        [SerializeField] private CountdownTimerView _countdownTimerView;

        protected ISystemService _systemService;
        protected IAudioService _audioService;
        protected Sequence _animations;
        private Vector3 _amountPosition;
        private List<Vector3> _imagePositions;

        [Inject]
        private void Construct(ISystemService systemService, IAudioService audioService)
        {
            _systemService = systemService;
            _audioService = audioService;
        }

        public override void Initialize(ILiveOps liveOps)
        {
            base.Initialize(liveOps);
            SetDefaultState();
            _countdownTimerView.Initialize(liveOps.GetRemainingTime, _systemService).AddTo(_disposables);
        }

        private void SetDefaultState()
        {
            _amount.gameObject.SetActive(false);
            _images.ForEach(image => image.gameObject.SetActive(false));
            _skeletonGraphic.AnimationState.SetAnimation(0, IdleName, true);

            _amountPosition = _amount.transform.localPosition;
            _imagePositions = _images.Select(image => image.transform.localPosition).ToList();
        }

        public override UniTask Animate(int addend, bool debug = false)
        {
            _animations?.Kill();
            _animations = DOTween.Sequence().SetTarget(gameObject);

            _amount.text = $"+{addend}";

            _animations.AppendCallback(() =>
            {
                _amount.transform.localPosition = _amountPosition;
                _images.ForEach((image, index) => image.transform.localPosition = _imagePositions[index]);

                _amount.transform.localScale = Vector3.zero;
                _images.ForEach(image => image.transform.localScale = Vector3.zero);

                _amount.gameObject.SetActive(true);
                _images.ForEach(image => image.gameObject.SetActive(true));

                _amount.alpha = 1.0f;
                _images.ForEach(image => image.SetAlpha(1.0f));
            });

            _amount.transform.DOScale(Vector3.one, 0.3f * AnimationMultiplier).SetEase(Ease.OutBack).AppendTo(_animations);
            _images.ForEach((image, index) => image.transform
                .DOScale(Vector3.one, 0.2f * AnimationMultiplier)
                .SetEase(Ease.OutBack)
                .InsertTo(_animations, index * 0.05f + 0.2f));

            _animations.AppendInterval(0.5f);

            var duration = _animations.Duration();

            _animations.InsertCallback(duration + 0.35f, () =>
            {
                _skeletonGraphic.AnimationState.SetAnimation(0, ActiveName, false);
                _skeletonGraphic.AnimationState.AddAnimation(0, IdleName, true, 0.0f);
            });

            _images.ForEach((image, index) => image.transform
                .DOMoveX(transform.position.x, 0.5f * AnimationMultiplier)
                .SetEase(Ease.InBack)
                .SetDelay(index * 0.05f)
                .InsertTo(_animations, duration + index * 0.05f));

            _amount.DOFade(0.0f, 0.3f * AnimationMultiplier)
                .SetEase(Ease.OutCubic)
                .InsertTo(_animations, duration + 0.2f);

            _images.ForEach((image, index) => image
                .DOFade(0.0f, 0.2f * AnimationMultiplier)
                .SetEase(Ease.OutCubic)
                .InsertTo(_animations, duration + index * 0.05f + 0.4f));

            AppendAnimation(addend, debug);

            return _animations.AwaitForComplete();
        }

        protected virtual void AppendAnimation(int addend, bool debug) { }
    }
}
