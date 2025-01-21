#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace TapEmpire.Services
{
    public class AndroidStoreAuthAdapter : IStoreAuthAdapter
    {
        public string Token;
        public string Error;
        
        public AndroidStoreAuthAdapter()
        {
            PlayGamesPlatform.Activate();
        }
        
        public void Login()
        {
            PlayGamesPlatform.Instance.Authenticate((success) =>
            {
                if (success == SignInStatus.Success)
                {
                    Debug.Log("Login with Google Play games successful.");

                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                    {
                        Debug.Log($"Authorization code: {code}");
                        Token = code;
                    });
                }
                else
                {
                    Error = "Failed to retrieve Google play games authorization code";
                    Debug.Log("Login Unsuccessful");
                }
            });
        }
        
        public void Logout()
        {
            //do nothing
        }
    }
}
#endif