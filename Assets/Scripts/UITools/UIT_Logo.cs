using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIT_Logo : SingletonMono<UIT_Logo> {
    public Action OnShowLogoFinished;
    public bool passLogo;
    Text AllRightsReserved;
    float startTime;
    protected override void Awake()
    {
        base.Awake();
        AllRightsReserved = transform.Find("AllRightsReserved").GetComponent<Text>();
        AllRightsReserved.color = new Color(1, 1, 1, 0);
        startTime = Time.time;
    }
    void Update ()
    {
        if (passLogo)
        {
            OnFinished();
            return;
        }
        float curTime = Time.time - startTime;
        float timeParam;
        if (curTime < 1)
        {
            timeParam = curTime / 1;
            AllRightsReserved.color = Color.Lerp(new Color(1,1,1,0),new Color(1,1,1,1),timeParam);
        }
        else if (curTime < 5)
        {
             timeParam = (curTime - 3)/2;
            AllRightsReserved.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), timeParam);
        }
        else
        {
            OnFinished();
        }
	}
    void OnFinished()
    {
        Destroy(this.gameObject);
        if(OnShowLogoFinished!=null)
        OnShowLogoFinished();
    }
}

