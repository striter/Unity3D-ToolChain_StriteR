using System;
using UnityEngine;
using UnityEngine.UI;
public class UIT_GridItem : TGameObjectPool_Instance_Monobehaviour<int>
{
    protected RectTransform m_Container;
    public RectTransform rectTransform { get; private set; }
    public RectTransform containerRectTransform { get; private set; }
    public override void OnInitItem(Action<int> DoRecycle)
    {
        base.OnInitItem(DoRecycle);
        rectTransform = transform as RectTransform;
        Transform container = transform.Find("Container");
        if(container)
            m_Container = container as RectTransform;
    }
    public void SetShowScrollView(bool show)=> m_Container.SetActivate(show);


    public void InitHighlight(Action<int> OnHighlighClick ) { OnInitHighlight(() => { OnHighlighClick(m_Identity); }); }
    protected virtual void OnInitHighlight(Action OnHighlightClick) { }
    public virtual void OnHighlight(bool highlight) { }
}
