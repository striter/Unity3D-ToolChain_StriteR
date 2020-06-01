using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
//List Of INumerators Will Be Used
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
    public static IEnumerator PauseDel(float pauseDuration, Action del)
    {
        yield return new WaitForSeconds(pauseDuration);
        del();
    }
    public static IEnumerator PauseDel<T>(float pauseDuration, T template, Action<T> del)
    {
        yield return new WaitForSeconds(pauseDuration);
        del(template);
    }
    public static IEnumerator RectTransformLerpTo(RectTransform rectTrans, Vector3 startPos, Vector3 endPos, float duration, Action OnFinished = null)
    {
        float startTime = Time.time;
        for (; ; )
        {
            if (rectTrans == null)
            {
                yield break;
            }

            float timeParam = (Time.time - startTime) / duration;
            if (timeParam > 1)
            {
                rectTrans.anchoredPosition = endPos;
                if (OnFinished != null)
                    OnFinished();
                yield break;
            }
            else
            {
                rectTrans.anchoredPosition = Vector3.Lerp(startPos, endPos, timeParam);
            }
            yield return null;
        }
    }
    public static IEnumerator RigidbodyMovePosition(Rigidbody rigid, Vector3 startPos, Vector3 endPos, float duration, Action OnFinished = null)
    {

        float startTime = Time.time;
        for (; ; )
        {
            if (rigid == null)
            {
                yield break;
            }

            float timeParam = (Time.time - startTime) / duration;
            if (timeParam > 1)
            {
                rigid.MovePosition(endPos);
                if (OnFinished != null)
                    OnFinished();
                yield break;
            }
            else
            {
                rigid.MovePosition(Vector3.Lerp(startPos, endPos, timeParam));
            }
            yield return null;
        }
    }
    public static IEnumerator TransformLerpTo(Transform lerpTrans, Vector3 startPos, Vector3 endPos, float duration, bool isLocal, Action OnFinished = null)
    {
        float startTime = Time.time;
        for (; ; )
        {
            if (lerpTrans == null)
            {
                yield break;
            }

            float timeParam = (Time.time - startTime) / duration;
            if (timeParam > 1)
            {
                if (OnFinished != null)
                    OnFinished();
                yield break;
            }
            else
            {
                if (isLocal)
                    lerpTrans.localPosition = Vector3.Lerp(startPos, endPos, timeParam);
                else
                    lerpTrans.position = Vector3.Lerp(startPos, endPos, timeParam);
            }
            yield return null;
        }
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

//Interface For CoroutineManager
public interface ICoroutineHelperClass
{
}
public static class ICoroutineHelper_Extend
{
    struct SCoroutineHelperData
    {
        public int m_HelperIndex { get; private set; }
        public List<int> m_CoroutineIndexes { get; private set; }
        public SCoroutineHelperData(int helperIndex)
        {
            m_HelperIndex = helperIndex;
            m_CoroutineIndexes = new List<int>();
        }
    }
    static int m_HelperCount = 0;
    static Dictionary<ICoroutineHelperClass, SCoroutineHelperData> m_TargetCoroutines=new Dictionary<ICoroutineHelperClass, SCoroutineHelperData>();
    internal static int GetCoroutineIndex(ICoroutineHelperClass target, int coroutineIndex) => m_TargetCoroutines[target].m_HelperIndex * 10000 + coroutineIndex;
    public static void StartSingleCoroutine(this ICoroutineHelperClass target, int index, IEnumerator ienumerator)
    {
        if (!m_TargetCoroutines.ContainsKey(target))
            m_TargetCoroutines.Add(target, new SCoroutineHelperData(m_HelperCount++));

        int targetIndex = GetCoroutineIndex(target, index);
        CoroutineHelperManager.StartCoroutine(targetIndex,ienumerator);

        if (!m_TargetCoroutines[target].m_CoroutineIndexes.Contains(targetIndex))
            m_TargetCoroutines[target].m_CoroutineIndexes.Add(targetIndex);
    }
    public static void StopSingleCoroutine(this ICoroutineHelperClass target, int index = 0)
    {
        if (!m_TargetCoroutines.ContainsKey(target))
            return;

        int targetIndex = GetCoroutineIndex(target, index);
        CoroutineHelperManager.StopCoroutine(targetIndex);

        if (m_TargetCoroutines[target].m_CoroutineIndexes.Contains(index))
            m_TargetCoroutines[target].m_CoroutineIndexes.Remove(targetIndex);
    }

    public static void StopAllSingleCoroutines(this ICoroutineHelperClass target)
    {
        if (!m_TargetCoroutines.ContainsKey(target))
            return;

        m_TargetCoroutines[target].m_CoroutineIndexes.Traversal((int coroutineIndex) =>  {
            CoroutineHelperManager.StopCoroutine(coroutineIndex);
        });
        m_TargetCoroutines.Remove(target);
    }
    

    //Main Coroutine Manager
    class CoroutineHelperManager : SingletonMono<CoroutineHelperManager>
    {
        static Dictionary<int, Coroutine> m_CoroutinesDic = new Dictionary<int, Coroutine>();

        public static void StartCoroutine(int targetIndex, IEnumerator ienumerator)
        {
            StopCoroutine(targetIndex);

            if (!m_CoroutinesDic.ContainsKey(targetIndex))
                m_CoroutinesDic.Add(targetIndex, null);

            m_CoroutinesDic[targetIndex] = Instance.StartCoroutine(ienumerator);
        }

        public static void StopCoroutine(int targetIndex)
        {
            if (!m_CoroutinesDic.ContainsKey(targetIndex))
                return;

            if(m_CoroutinesDic[targetIndex]==null)
            {
                m_CoroutinesDic.Remove(targetIndex);
                return;
            }

            Instance.StopCoroutine(m_CoroutinesDic[targetIndex]);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_TargetCoroutines.Clear();
        }
    }
}
