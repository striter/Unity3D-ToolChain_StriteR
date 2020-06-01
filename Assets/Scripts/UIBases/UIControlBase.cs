using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControlBase : UIComponentBase {
    public static T Show<T>(Transform _parentTrans) where T : UIControlBase
    {
        T tempBase = TResources.Instantiate<T>("UI/Controls/" + typeof(T).ToString(), _parentTrans);
        tempBase.Init();
        return tempBase;
    }

    protected override void Init()
    {
        base.Init();
    }
    protected virtual void OnDestroy()
    {

    }
    public void Hide()
    {
        Destroy(this.gameObject);
    }
}
