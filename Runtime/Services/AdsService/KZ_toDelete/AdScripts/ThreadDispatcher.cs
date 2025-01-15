// using GameAnalyticsSDK.Events;
// using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> adEventsQueue = new Queue<Action>();
    private static volatile bool adEventsQueueEmpty = true;

    public static void Enqueue(Action action)
    {
        lock (adEventsQueue)
        {
            adEventsQueue.Enqueue(action);
            adEventsQueueEmpty = false;
        }
    }

    void LateUpdate()
    {
        if (adEventsQueueEmpty) return;

        lock (adEventsQueue)
        {
            while (adEventsQueue.Count > 0)
            {
                try
                {
                    adEventsQueue.Dequeue()?.Invoke();
                }
                catch (Exception e)
                {
                    string message = $"Dispatcher : Message = {e.Message}, Scene = {SceneManager.GetActiveScene().name}, AdPlacement = {AnalyticsManager.PlacementName}.\n";
                    if (AdConstants.IsDebugBuild)
                    {
                        Debug.LogError(message);
                        Debug.LogException(e);
                    }
                    else
                    {
                        //AppMetrica.Instance.ReportError(e, "Dispatcher");
                        // GA_Debug.HandleLog(message, e.StackTrace, LogType.Exception);
                    }
                }

                adEventsQueueEmpty = true;
            }
        }
    }

    private void OnDisable()
    {
        isInitialized = false;
    }

    #region Instance

    static bool isInitialized;
    public static void Initialize()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            GameObject go = new GameObject("Thread Dispatcher");
            go.AddComponent<ThreadDispatcher>();
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
        }
    }

    #endregion
}
