using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPageBase : UIComponentBase
{
    public static UIPageBase Generate(Type type,Transform parentTransform)
    {
        UIPageBase page = TResources.Instantiate<UIPageBase>("UI/Pages/" + type.ToString(), parentTransform);
        page.Init();
        return page;
    }

    Action<UIPageBase> OnPageExit;
    protected Image img_Background;
    bool m_Animating;
    public const float F_AnimDuration = .25f;
    protected Button btn_ContainerCancel,btn_WholeCancel;
    protected RectTransform rtf_Container;
    Vector2 m_AnimateStartPos, m_AnimateEndPos;
    SingleCoroutine m_AnimationCoroutine;

    protected override void Init()
    {
        base.Init();
        rtf_Container = transform.Find("Container") as RectTransform;
        img_Background = transform.Find("Background").GetComponent<Image>();
        btn_WholeCancel = img_Background.GetComponent<Button>();
        Transform containerCancel = rtf_Container.Find("BtnCancel");
        if (containerCancel) btn_ContainerCancel = containerCancel.GetComponent<Button>();

        if (btn_WholeCancel)  btn_WholeCancel.onClick.AddListener(OnCancelBtnClick);
        if (btn_ContainerCancel) btn_ContainerCancel.onClick.AddListener(OnCancelBtnClick);

        m_AnimateStartPos = rtf_Container.anchoredPosition + Vector2.up * Screen.height;
        m_AnimateEndPos = rtf_Container.anchoredPosition;
        m_AnimationCoroutine = new SingleCoroutine(this);
    }

    public virtual void OnPlay(bool doAnim, Action<UIPageBase> OnPageExit)
    {
        m_Animating = doAnim;
        this.OnPageExit = OnPageExit;
        if (btn_ContainerCancel) btn_ContainerCancel.enabled = true;
        if (btn_WholeCancel) btn_WholeCancel.enabled = true;

        if (!doAnim)
            return;
        m_AnimationCoroutine.StartSingleCoroutine( TIEnumerators.ChangeValueTo((float value) => {
            rtf_Container.anchoredPosition = Vector2.Lerp(m_AnimateStartPos, m_AnimateEndPos, value);
        }
        , 0f, 1f, F_AnimDuration, null, false));
    }

    public virtual void OnStop()
    {

    }

    protected virtual void OnCancelBtnClick()
    {
        if (btn_ContainerCancel) btn_ContainerCancel.enabled = false;
        if (btn_WholeCancel) btn_WholeCancel.enabled = false;
        if (!m_Animating)
        {
            OnPageExit(this);
            return;
        }
        m_AnimationCoroutine.StartSingleCoroutine(TIEnumerators.ChangeValueTo((float value) => {
            rtf_Container.anchoredPosition = Vector2.Lerp(m_AnimateStartPos, m_AnimateEndPos, value);
        }, 1f, 0f, F_AnimDuration,()=> { OnPageExit(this); }, false));
    }


}
