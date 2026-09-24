using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class FrameAnimation : MonoBehaviour
    {
        public Subject<Unit> OnImpact { get; } = new();

        public Image Image => _image;

        public float Duration => _frames.Length / _frameRate;

        [SerializeField] private Image _image;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _frameRate = 30f;
        [SerializeField] private int _impactFrame;

        public UniTask Play()
        {
            var sequence = DOTween.Sequence()
                .Append(DOVirtual
                    .Int(0, _frames.Length - 1, Duration, frame => _image.sprite = _frames[frame])
                    .SetEase(Ease.Linear))
                .InsertCallback(_impactFrame / _frameRate, () => OnImpact.OnNext(Unit.Default));

            return sequence.SetLink(gameObject).ToUniTask();
        }
    }
}
