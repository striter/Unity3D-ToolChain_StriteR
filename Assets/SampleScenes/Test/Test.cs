using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    private void Update()
    {
        int test = 1 << 0 | 1 << 1 | 0 << 2 | 0 << 3| 1 << 4;
        Debug.Log(test);
        Debug.Log(Convert.ToString(test,2));
        Debug.Log(TCommon.GetByBit(test,0));
    }
}
