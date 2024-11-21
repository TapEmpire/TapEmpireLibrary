using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RagDoll/RateUsSettings", fileName = "RateUsSettings")]
public class RateUsSettings : ScriptableObject
{
    [SerializeField] 
    public bool DefaultNeedRateUs;

    [SerializeField] 
    public List<int> Levels;
    
    public bool CheckNeedRateUs(int numberLevel)
    {
        if (!DefaultNeedRateUs) return false;
            
        return Levels.Contains(numberLevel);
    }
}
