using TMPro;
using UnityEngine;

namespace TapEmpire.Services.Localization
{
    public class LocalizeStringEventScene : MonoBehaviour
    {
        [SerializeField] private string _table;
        [SerializeField] private string _key;

        private TMP_Text _text;
        private LocalizationStringModel _descriptionLocalization;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();

            _descriptionLocalization = new LocalizationStringModel(_table, _key, s => { _text.text = s; });
        }
    }
}