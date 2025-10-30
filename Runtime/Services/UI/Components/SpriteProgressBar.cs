using DG.Tweening;
using UnityEngine;

namespace TapEmpire.UI
{
    public class SpriteProgressBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _maxWidth;

        private Tween _tween;
        
        private void Start()
        {
            _spriteRenderer.size = new Vector2(0, _spriteRenderer.size.y);
        }


        public void SetValue(float val)
        {
            if (val == 0)
            {
                _spriteRenderer.size = new Vector2(_maxWidth * val, _spriteRenderer.size.y);
            }
            else
            {
                _tween?.Kill();
                _tween = DOVirtual.Float(_spriteRenderer.size.x, _maxWidth * val, 0.2f, value => { _spriteRenderer.size = new Vector2(value, _spriteRenderer.size.y); });
            }
        }
    }
}