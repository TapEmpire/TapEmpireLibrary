using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class NetworkUtility
    {
        public static async UniTask WaitNetworkAsyncReliable(System.Action<int> syncResult = null)
        {
            bool result;

            do
            {
                result = await WaitPing("8.8.8.8", syncResult) > 0;
            } while (!result);
        }

        public static async UniTask<float> WaitPing(string pingServer, System.Action<int> syncResult = null)
        {
            var ping = new Ping(pingServer);

            do
            {
                await UniTask.WaitForSeconds(0.1f);
            } while (!ping.isDone);

            syncResult?.Invoke(ping.time);
            return ping.time;
        }
    }
}