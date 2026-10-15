using Cysharp.Threading.Tasks;
using System.Diagnostics;
using System.Threading;    
using TMPro;
using UnityEngine;

namespace TapEmpire.UI
{
    public class TextDotsAnimation : MonoBehaviour
    {
        private const string DotsPattern = "...";
        private const float StepInterval = 0.4f;

        [SerializeField] private TMP_Text _dotsText;

        private CancellationTokenSource _cts;

        public void StartAnimation()
        {
            StopAnimation();
            if (_cts != null)
                return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            RunAsync(_cts.Token).Forget();
        }

        public void StopAnimation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid RunAsync(CancellationToken ct)
        {
            if (_dotsText == null)
                return;

            string fullText = _dotsText.text;
            bool dotsAtStart = fullText.StartsWith(DotsPattern);
            bool dotsAtEnd = fullText.EndsWith(DotsPattern);

            if (!dotsAtStart && !dotsAtEnd)
                return;

            string baseText = dotsAtStart
                ? fullText[DotsPattern.Length..]
                : fullText[..^DotsPattern.Length];

            var rt = _dotsText.rectTransform;
            var oldPivot = rt.pivot;
            var deltaPos = new Vector2((0f - oldPivot.x) * rt.rect.width, 0f);
            rt.pivot = new Vector2(0f, oldPivot.y);
            rt.anchoredPosition += deltaPos;

            int step = 0;

            while (!ct.IsCancellationRequested)
            {
                step = (step % 3) + 1;
                string dots = new('.', step);
                _dotsText.text = dotsAtStart ? dots + baseText : baseText + dots;
                await UniTask.WaitForSeconds(StepInterval, cancellationToken: ct);
            }

            _dotsText.text = fullText;
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}
