using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AdNetworkBase : MonoBehaviour
{
    protected bool isInitialized;
    public abstract UniTask Initialize();
    public abstract bool HasInterstitial(bool doRequest);
    public abstract bool HasRewarded(bool doRequest);

    public bool IsReady => isInitialized;
}