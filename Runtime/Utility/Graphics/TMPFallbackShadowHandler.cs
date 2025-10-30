using UnityEngine;
using LeTai.TrueShadow;
using UnityEngine.Localization.Components;
using TMPro;
using System.Collections.Generic;

namespace TapEmpire.UI
{
    [RequireComponent(typeof(LocalizeStringEvent))]
    public class TMPFallbackShadowHandler : MonoBehaviour
    {
        private TextMeshProUGUI _tmp;
        private TrueShadow _trueShadow;
        private LocalizeStringEvent _localizeStringEvent;

        private bool _needUpdateTMPSubmeshesShadow = false;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
            _trueShadow = GetComponent<TrueShadow>();
            _localizeStringEvent = GetComponent<LocalizeStringEvent>();
        }

        private void OnEnable()
        {
            _localizeStringEvent.OnUpdateString.AddListener(OnUpdateString);
        }

        private void OnDisable()
        {
            _localizeStringEvent.OnUpdateString.RemoveListener(OnUpdateString);
        }

        private void Start()
        {
            Canvas.willRenderCanvases += OnWillRenderCanvases;
        }

        private void OnDestroy()
        {
            Canvas.willRenderCanvases -= OnWillRenderCanvases;
        }

        private void OnUpdateString(string text)
        {
            _needUpdateTMPSubmeshesShadow = true;
        }

        private void OnWillRenderCanvases()
        {
            if (_needUpdateTMPSubmeshesShadow)
            {
                _needUpdateTMPSubmeshesShadow = false;
                _trueShadow.CopyToTMPSubMeshes();
                UpdateSubmeshShadowUsage();
            }
        }

        private void UpdateSubmeshShadowUsage()
        {
            var textInfo = _tmp.textInfo;
            var usedFonts = new HashSet<TMP_FontAsset>();

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var character = textInfo.characterInfo[i];
                if (character.isVisible)
                {
                    usedFonts.Add(character.fontAsset);
                }
            }

            var subMeshes = GetComponentsInChildren<TMP_SubMeshUI>(true);
            foreach (var subMesh in subMeshes)
            {
                subMesh.GetComponent<TrueShadow>().enabled = usedFonts.Contains(subMesh.fontAsset);
            }
        }
    }
}