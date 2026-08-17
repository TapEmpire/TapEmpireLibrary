using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace TapEmpire.Utility
{
    public static class WebRequestUtility
    {
        public static async UniTask<(string Text, string Error)> GetTextAsync(string url, int timeout,
            CancellationToken cancellationToken)
        {
            using var request = UnityWebRequest.Get(url);

            request.timeout = timeout;

            try
            {
                await request.SendWebRequest().WithCancellation(cancellationToken);
            }
            catch (Exception exception)
            {
                return (null, exception.Message);
            }

            return request.result == UnityWebRequest.Result.Success
                ? (request.downloadHandler.text, null)
                : (null, request.error);
        }
    }
}
