using System;
using UnityEngine;
using UnityEngine.UI;
public class UIT_GridItem : CObjectPoolMono<int>
{
    protected RectTransform rtf_Container;
    protected RectTransform rtf_RectTransform;
    public RectTransform rectTransform => rtf_RectTransform;
    public override void OnInitItem()
    {
        base.OnInitItem();
        rtf_RectTransform = transform.GetComponent<RectTransform>();
        rtf_Container = transform.Find("Container") as RectTransform;
    }

    public void SetShowScrollView(bool show)=> rtf_Container.SetActivate(show);
}
