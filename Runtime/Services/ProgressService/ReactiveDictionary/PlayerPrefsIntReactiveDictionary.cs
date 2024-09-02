using UnityEngine;

namespace TapEmpire.Services
{
    public class PlayerPrefsIntReactiveDictionary : PlayerPrefsReactiveDictionary<int>
    {
        protected override int GetValueFromPrefs(string key)
        {
            return PlayerPrefs.GetInt(key);
        }

        protected override void SetValueToPrefs(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
    }
}