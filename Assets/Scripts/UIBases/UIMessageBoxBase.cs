using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class UIMessageBoxBase : UIComponentBase {
    public static UIMessageBoxBase m_MessageBox { get; private set; }
    public static Action OnMessageBoxExit;
    public static T Show<T>(Transform _parentTrans) where T : UIMessageBoxBase
    {
        if(m_MessageBox)
        {
            Debug.LogError("Can't Open Another MessageBox While One Is Active");
            return null;
        }

        T messageBox = TResources.Instantiate<T>("UI/MessageBoxes/" + typeof(T).ToString(), _parentTrans);
        messageBox.Init();
        return messageBox;
    }
    protected Transform tf_Container { get; private set; }
    Button btn_Confirm;
    Action OnConfirmClick;

    protected override void Init()
    {
        base.Init();
        tf_Container = transform.Find("Container");
        btn_Confirm = tf_Container.Find("Confirm").GetComponent<Button>();
        btn_Confirm.onClick.AddListener(OnConfirm);
        tf_Container.Find("Cancel").GetComponent<Button>().onClick.AddListener(OnCancel);
        Button btn_BG = transform.Find("Background").GetComponent<Button>();
        if (btn_BG) btn_BG.onClick.AddListener(OnCancel);
        m_MessageBox = this;
    }
    protected virtual void OnDestroy()
    {
        m_MessageBox = null;
        OnMessageBoxExit();
    }

    protected void Play(Action _OnConfirmClick)
    {
        OnConfirmClick = _OnConfirmClick;
    }
     void OnCancel() => Destroy(this.gameObject);
    void OnConfirm()
    {
        OnConfirmClick();
        OnCancel();
    }
}
