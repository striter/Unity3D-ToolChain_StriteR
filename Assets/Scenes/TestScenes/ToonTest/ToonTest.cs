using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonTest : MonoBehaviour
{
    IEnumerable<int> Test()
    {
        yield return 1;
        yield return 2;
        yield return 3;
        yield return 4;
    }

    private void Update()
    {
        int index=0;
        if(Input.GetKeyDown(KeyCode.Space))
            for(int i=0;i<500000;i++)
                foreach (var VARIABLE in new List<int>(){1,2,3,4})
                    index++;
    }
}
