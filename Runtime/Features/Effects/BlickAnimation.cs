using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using R3;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Feature.Effects
{
    public class BlickAnimation : MonoBehaviour
    {
        public Subject<Unit> OnStart = new();

        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private float _delay = 0.0f;
        [SerializeField] private float _repeatDelay = 3.0f;
        [SerializeField] private Vector3 _endPoint = Vector3.zero;

        private Sequence _sequence = null;
        private Vector3 _startPos;
        
        private void Start()
        {
            _startPos = transform.localPosition;
            
            var body = DOTween.Sequence()
                .SetTarget(gameObject)
                .AppendCallback(() =>
                {
                    transform.localPosition = _startPos;
                    OnStart.OnNext(Unit.Default);
                })
                .Append(transform.DOLocalMove(_endPoint, _duration).SetEase(Ease.Linear))
                .AppendInterval(_repeatDelay)
                .SetLoops(int.MaxValue, LoopType.Restart); //MaxValue - infinite loops inside sequence are not allowed
            
            _sequence = DOTween.Sequence()
                .SetTarget(gameObject)
                .AppendInterval(_delay)
                .Append(body);
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}
