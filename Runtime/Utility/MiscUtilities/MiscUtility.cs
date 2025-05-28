using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;

namespace TapEmpire.Utility
{
    public static class Utility
    {
        public static void IfElse(bool condition, System.Action doIf, System.Action doElse)
        {
            if (condition)
            {
                doIf.Invoke();
            }
            else
            {
                doElse?.Invoke();
            }
        }

        public static Tween Delay(float delay, System.Action callback)
        {
            return DOVirtual.DelayedCall(delay, () => callback?.Invoke());
        }

        public static Tween Delay(float delay, System.Action callback, object target)
        {
            return DOVirtual.DelayedCall(delay, () => callback?.Invoke()).SetTarget(target);
        }

        public static void DelayAction(Transform transform, float delay, DG.Tweening.TweenCallback action)
        {
            DOTween.Sequence(transform).AppendInterval(delay).OnComplete(action);
        }
        public static void SetActive(GameObject gameObject, bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(active);
            }
        }
        
        public static void SetActive(Behaviour behaviour, bool active)
        {
            if (behaviour != null)
            {
                behaviour.enabled = active;
            }
        }
        
        public static void SetGameObjectActive(Component behaviour, bool active)
        {
            if (behaviour != null)
            {
                SetActive(behaviour.gameObject, active);
            }
        }

        public static void DestroySafe(GameObject gameObject)
        {
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
        }
        
        public static void DestroyObject(GameObject levelObject)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Object.Destroy(levelObject);
            else
                Object.DestroyImmediate(levelObject);
#else
            Object.Destroy(levelObject);
#endif
        }
        
        public static void UpdateWidth(Transform transform, float width)
        {
            var rectTransform = transform.GetComponent<RectTransform>();
            Vector2 size = rectTransform.sizeDelta;
            size.x = width;
            rectTransform.sizeDelta = size;
        }

        public static void UnparentWithScale(Transform transform, Vector3 scale)
        {
            transform.SetParent(null, true);
            transform.localScale = scale;
        }

        public static Vector3? GetPosition(Transform transform)
        {
            return transform == null ? null : transform.position;
        }

        public static IEnumerable<T> CreateEnumerable<T>(int count) where T : new()
        {
            for (int i = 0; i < count; ++i)
            {
                yield return new T();
            }
        }

        public static IEnumerable<T> CreateEnumerable<T>(params T[] items)
        {
            return items ?? Enumerable.Empty<T>();
        }

        public static void RestartScene()
        {
            DOTween.KillAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static T LoadResource<T>(string errorPrefix, string path) where T : Object
        {
            var resource = Resources.Load<T>(path);

            if (resource == null)
            {
                Debug.LogError(errorPrefix + path);
            }

            return resource;
        }

        public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                yield return child;
            }
        }
    }

    public class BoolCounter
    {
        public BoolCounter(bool enabled = false)
        {
            _counter = enabled ? 1 : 0;
        }

        public static implicit operator bool(BoolCounter instance) => instance.IsEnabled;
        public bool IsEnabled => _counter > 0;
        public void SetValue(bool isEnabled)
            => _counter = isEnabled ? _counter + 1 : Mathf.Max(_counter - 1, 0);

        private int _counter = 0;
    }
}