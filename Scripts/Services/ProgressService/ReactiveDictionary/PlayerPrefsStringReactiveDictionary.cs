using UnityEngine;

namespace TapEmpire.Services
{
    public class PlayerPrefsStringReactiveDictionary : PlayerPrefsReactiveDictionary<string>
    {
        protected override string GetValueFromPrefs(string key)
        {
            return PlayerPrefs.GetString(key, string.Empty);
        }
        
        protected override void SetValueToPrefs(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
    }
}