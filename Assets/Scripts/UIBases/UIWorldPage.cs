using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldPage : UIWorldBase
{
    public static T ShowPage<T>(Transform parentTransform) where T : UIWorldPage
    {
        T tempBase = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/World/" + typeof(T).ToString()), parentTransform).GetComponent<T>();
        tempBase.Init();
        return tempBase;
    }
    protected Button btn_Cancel;
    public override void Init()
    {
        base.Init();
        btn_Cancel = tf_Container.Find("BtnCancel").GetComponent<Button>();
        btn_Cancel.onClick.AddListener(OnCancelBtnClick);
    }
    protected virtual void OnCancelBtnClick()
    {
        Hide();
    }
}
