using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class WaitUlti
{
    private static readonly Dictionary<float, WaitForSeconds> waitCache = new Dictionary<float, WaitForSeconds>();

    private static WaitForSeconds GetWaitForSeconds(float duration)
    {
        if (!waitCache.TryGetValue(duration, out WaitForSeconds wait))
        {
            wait = new WaitForSeconds(duration);
            waitCache[duration] = wait;
        }
        return wait;
    }

    public static void Wait(this MonoBehaviour runner, float duration, Action onComplete)
    {
        if (runner == null || onComplete == null)
        {
            Debug.LogWarning("Runner or callback is null!");
            return;
        }
        runner.StartCoroutine(Wait(duration, onComplete));
    }

    public static void WaitUntil(this MonoBehaviour runner, Func<bool> condition, Action onComplete)
    {
        if (runner == null || condition == null || onComplete == null)
        {
            Debug.LogWarning("Runner, condition, or callback is null!");
            return;
        }
        runner.StartCoroutine(WaitUntil(condition, onComplete));
    }

    public static void WaitUntil(this MonoBehaviour runner, Func<bool> condition, float maxDuration, Action onComplete)
    {
        if (runner == null || condition == null || onComplete == null)
        {
            Debug.LogWarning("Runner, condition, or callback is null!");
            return;
        }
        runner.StartCoroutine(WaitUntil(condition, maxDuration, onComplete));
    }

    public static void ClearWaitCache(this MonoBehaviour runner)
    {
        waitCache.Clear();
    }

    private static IEnumerator Wait(float duration, Action onComplete)
    {
        if (onComplete == null)
        {
            Debug.LogWarning("Callback is null!");
            yield break;
        }

        yield return GetWaitForSeconds(duration);

        onComplete();
    }

    private static IEnumerator WaitUntil(Func<bool> condition, Action onComplete)
    {
        if (condition == null || onComplete == null)
        {
            Debug.LogWarning("Condition or callback is null!");
            yield break;
        }

        yield return new WaitUntil(condition);

        onComplete();
    }

    private static IEnumerator WaitUntil(Func<bool> condition, float maxDuration, Action onComplete)
    {
        if (condition == null || onComplete == null)
        {
            Debug.LogWarning("Condition or callback is null!");
            yield break;
        }

        float startTime = Time.time;
        yield return new WaitUntil(() => condition() || Time.time - startTime > maxDuration);

        onComplete();
    }
}