using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public static class TIEnumerators
{
    public static class UI
    {
        public static IEnumerator StartTypeWriter(Text text,float duration)
        {
            string targetText = text.text;
            float startTime = Time.time;
            StringBuilder m_stringBuilder = new StringBuilder(targetText);
            int length=m_stringBuilder.Length;
            for (; ;)
            {
                float timeParam = (Time.time - startTime) / duration;
                if (timeParam > 1)
                {
                    text.text = targetText;
                    yield break;
                }
                text.text = m_stringBuilder.ToString(0, (int)(length * timeParam));
                yield return null;
            }
            
        }
    }
    public static IEnumerator Tick(Action OnTick, float duration = -1)
    {
        WaitForSeconds seconds = duration == -1 ? null : new WaitForSeconds(duration);
        OnTick();
        for (; ; )
        {
            yield return seconds;
            OnTick();
        }
    }
    public static IEnumerator TickCount(Action OnTick, int totalTicks, float duration = -1, Action OnFinished = null,bool tickOnStart=true)
    {
        WaitForSeconds seconds = duration == -1 ? null : new WaitForSeconds(duration);
        int count = 1;
        OnTick();
        for (; ; )
        {
            yield return seconds;
            if (count >= totalTicks)
            {
                OnFinished?.Invoke();
                yield break;
            }
            OnTick();
            count++;
        }
    }
    public static IEnumerator TickDelta(Func<float,bool> OnTickDeltaBreak, float duration = -1)
    {
        float preTime = Time.time;
        WaitForSeconds seconds = duration == -1 ? null : new WaitForSeconds(duration);
        if (OnTickDeltaBreak(Time.time - preTime))
            yield break;
        for (; ; )
        {
            yield return seconds;
            if (OnTickDeltaBreak(Time.time - preTime))
                yield break;
            preTime = Time.time;
        }
    }
    public static IEnumerator TickTraversel<T>(Action<T> OnTick, List<T> list, float duration = -1)
    {
        int index = 0;
        WaitForSeconds seconds = duration == -1 ? null : new WaitForSeconds(duration);
        OnTick(list[index++]);
        for (; ; )
        {
            yield return seconds;
            OnTick(list[index++]);
            if (index == list.Count)
                yield break;
        }
    }
    public static IEnumerator ChangeValueTo(Action<float> OnValueChanged, float startValue, float endValue, float duration, Action OnFinished = null, bool scaled = true)
    {
        float timeValue = duration;
        for (; ; )
        {
            timeValue -= scaled ? Time.deltaTime : Time.unscaledDeltaTime;
            OnValueChanged(Mathf.Lerp(endValue,startValue,timeValue/duration));
            if (timeValue<0)
            {
                OnValueChanged(endValue);
                OnFinished?.Invoke();
                yield break;
            }
            yield return null;
        }
    }

    public static IEnumerator YieldReturnAction(IEnumerator ienumerator,Action action)
    {
        yield return ienumerator;
        action();
    }

    public static IEnumerator YieldReturnAction(YieldInstruction yieldInstruction, Action action)
    {
        yield return yieldInstruction;
        action();
    }

    public static IEnumerator TaskCoroutine(this Task task)
    {
        for(; ; )
        {
            if (task.IsFaulted)
                throw(task.Exception);

            if (task.IsCompleted || task.IsCanceled)
                yield break;

            yield return null;
        }
    }
}

public class SingleCoroutine
{
    Coroutine m_Coroutine;
    MonoBehaviour m_StarterBehaviour;
    public SingleCoroutine(MonoBehaviour _starterBehaviour)
    {
        m_StarterBehaviour = _starterBehaviour;
        m_Coroutine = null;
    }
    public void Start(IEnumerator coroutine)
    {
        Stop();
        m_Coroutine= m_StarterBehaviour.StartCoroutine(coroutine);
    }
    public void Stop()
    {
        if (m_Coroutine == null)
            return;
        m_StarterBehaviour.StopCoroutine(m_Coroutine);
        m_Coroutine = null;
    }
}

public static class CoroutineHelper
{
    public static SingleCoroutine CreateSingleCoroutine() => new(CoroutineHelperManager.Instance);
    //Main Coroutine Manager
    class CoroutineHelperManager : SingletonMono<CoroutineHelperManager>
    {
        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }
    }
}
