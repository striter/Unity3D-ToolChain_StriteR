using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIT_TouchConsole;

public partial class UIT_TouchConsole
{
    Transform m_FrameRate;
    Text m_FrameRateValue;
    Queue<int> m_FrameRateQueue = new Queue<int>();
    [PartialMethod(EPartialMethods.Init,EPartialSorting.Miscs)]
    internal void InitMiscs()
    {
        m_FrameRate = transform.Find("FrameRate");
        m_FrameRateValue = m_FrameRate.Find("Value/Value").GetComponent<Text>();

    }
    [PartialMethod(EPartialMethods.Reset,EPartialSorting.Miscs)]
    internal void ResetMiscs()
    {
        m_FrameRateQueue.Clear();
        m_FrameRateValue.text = "";
    }
    [PartialMethod(EPartialMethods.Tick, EPartialSorting.Miscs)]
    internal void TickMiscs(float _deltaTime)
    {
        if (!m_Data.m_FilterSetting.m_RefValue.IsFlagEnable(EConsoleSetting.FPS))
            return;
        m_FrameRateQueue.Enqueue(Mathf.CeilToInt(1f / _deltaTime));
        if (m_FrameRateQueue.Count > 30)
            m_FrameRateQueue.Dequeue();

        int total = 0;
        foreach (var frameRate in m_FrameRateQueue)
            total += frameRate;
        total /= m_FrameRateQueue.Count;

        m_FrameRateValue.text = total.ToString();
    }
}
