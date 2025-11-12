using Facebook.Unity;

namespace TapEmpire.Modules
{
    public class FacebookModule
    {
        private bool _isPersonalized = false;

        public void Initialize(bool isPersonalized)
        {
            _isPersonalized = isPersonalized;

            if (!FB.IsInitialized)
            {
                FB.Init(OnInitComplete, OnHideUnity);
            }
            else
            {
                FB.ActivateApp(); // App Events session
            }
        }

        void OnInitComplete()
        {
            UnityEngine.Debug.Log("FB.Init complete");
            FB.ActivateApp();

#if UNITY_IOS && !UNITY_EDITOR
            FB.Mobile.SetAdvertiserTrackingEnabled(_isPersonalized);
#endif
        }

        void OnHideUnity(bool isGameShown)
        {
            UnityEngine.Time.timeScale = isGameShown ? 1 : 0;
        }
    }
}