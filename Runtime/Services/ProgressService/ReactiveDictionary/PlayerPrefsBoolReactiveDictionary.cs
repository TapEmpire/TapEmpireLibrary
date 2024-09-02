using UnityEngine;

namespace TapEmpire.Services
{
    public class PlayerPrefsBoolReactiveDictionary : PlayerPrefsReactiveDictionary<bool>
    {
        protected override bool GetValueFromPrefs(string key)
        {
            return PlayerPrefs.GetInt(key) == 1;
        }

        protected override void SetValueToPrefs(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }

}