using System;
using System.Collections.Generic;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace TapEmpire.UI
{
    public class DotsScrollPagination : MonoBehaviour
    {
        [SerializeField] List<Image> _dots;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _chosenColor = Color.white;
        [SerializeField] ScrollSnap _scrollSnap;

        private void OnEnable()
        {
            _scrollSnap.onPageChange += Initialize;
            _scrollSnap.onPageChange += OnPageChange;
        }

        private void OnDisable()
        {
            _scrollSnap.onPageChange -= OnPageChange;
        }

        private void OnPageChange(int page)
        {
            _dots.ForEach(dot => dot.color = _defaultColor);
            _dots[page].color = _chosenColor;
        }

        private void Initialize(int page)
        {
            var hasManyPages = _scrollSnap.Pages > 1;
            _scrollSnap.onPageChange -= Initialize;
            _dots.ForEach((dot, index) => dot.gameObject.SetActive(index < _scrollSnap.Pages && hasManyPages));
        }
    }
}
