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

        private void Start()
        {
            _sequence = DOTween.Sequence();
            _sequence.SetDelay(_delay).SetTarget(this.gameObject);

            _sequence.AppendCallback(() => OnStart.OnNext(Unit.Default));
            transform.DOLocalMove(_endPoint, _duration).SetEase(Ease.Linear).AppendTo(_sequence);
            _sequence.AppendInterval(_repeatDelay);

            _sequence.SetLoops(-1, LoopType.Restart);
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}
