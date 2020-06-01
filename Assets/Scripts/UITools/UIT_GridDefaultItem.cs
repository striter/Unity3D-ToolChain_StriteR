
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIT_GridDefaultItem : UIT_GridItem {

    protected UIT_TextExtend txt_Default;
    protected Button btn_Default;
    protected Image img_Default;
    protected Image img_HighLight;
    public bool B_HighLight { get; protected set; }
    Action<int> OnItemClick;
    public override void OnInitItem()
    {
        base.OnInitItem();
        txt_Default = rtf_Container.Find("DefaultText").GetComponent<UIT_TextExtend>();
        img_Default = rtf_Container.Find("DefaultImage").GetComponent<Image>();
        img_HighLight = rtf_Container.Find("DefaultHighLight").GetComponent<Image>();
        btn_Default = rtf_Container.Find("DefaultBtn").GetComponent<Button>();
        if (btn_Default)  btn_Default.onClick.AddListener(OnItemTrigger);
        if (img_HighLight) SetHighLight(false);
    }
    public void SetDefaultOnClick(Action<int> _OnItemClick)
    {
        OnItemClick = _OnItemClick;
    }

    public void SetItemInfo(string defaultKey = "", bool highLight = false, Sprite defaultSprite = null, bool setNativeSize = false)
    {
        if (defaultKey != "")
            txt_Default.localizeKey = defaultKey;
        if (defaultSprite != null)
        {
            img_Default.sprite = defaultSprite;
            if (setNativeSize)
                img_Default.SetNativeSize();
        }
        SetHighLight(highLight);
    }
    public void SetHighLight(bool highLight)
    {
        B_HighLight = highLight;
        if (img_HighLight == null)
        {
            return;
        }
        img_HighLight.SetActivate(highLight);
    }
    protected void OnItemTrigger()
    {
        OnItemClick?.Invoke(m_Identity);
    }
}
