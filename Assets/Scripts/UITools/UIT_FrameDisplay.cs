using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIT_FrameDisplay : SingletonMono<UIT_FrameDisplay>
{
    Text m_FrameText;
    protected override void Awake()
    {
        base.Awake();
        m_FrameText = GetComponent<Text>();

    }
    private void LateUpdate()
    {
        m_FrameText.text = ((int)(1 / Time.unscaledDeltaTime)).ToString();
    }
}
