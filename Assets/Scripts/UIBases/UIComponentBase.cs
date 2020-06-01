using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponentBase : MonoBehaviour {
    public RectTransform rectTransform { get;private set; }

    protected virtual void Init()
    {
        rectTransform=transform as RectTransform;
    }

}
