using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TmpMaterialSetter : MonoBehaviour
    {
        [SerializeField] private Material _tmpMaterial;

        private void OnEnable()
        {
            GetComponent<TMP_Text>().fontSharedMaterial = _tmpMaterial;
        }

        private void OnValidate()
        {
            GetComponent<TMP_Text>().fontSharedMaterial = _tmpMaterial;
        }
    }
}
