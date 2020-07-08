
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ShaderTest : MonoBehaviour {
    List<int> test = new List<int>() { 5,6,7,1,2,10,15,int.MaxValue,int.MinValue,int.MaxValue-100,-100};//new List<int>() { -100, -2147483648, 2, 1 };
    void Awake()
    {
        test.Clear();
        for(int i=0;i<UnityEngine.Random.Range(5,20);i++)
            test.Add(UnityEngine.Random.Range(-100,100));

        Debug.Log(TDataConvert.Convert(test));
        test.TSort(TCommon.enum_SortType.Quick);
        Debug.Log(TDataConvert.Convert(test));
        Application.runInBackground = true;
        GetComponent<CameraEffectManager>().Init().SetMainTextureCamera(true);
        CoroutineHelper.StartCoroutine(TIEnumerators.YieldReturnAction( TestMain().TaskCoroutine(),()=> { Debug.Log("Howdy"+count); }));
    }

    int count;

    async Task TestMain()
    {
        Action action = TestSub1;
        Task test1= Task.Delay(5000);
        Task test2 = Task.Run(action);
        Task test3 = TestDelay(5);
        Task<int> test4 = TestSub2();

        count = await test4;
        await test1;
        await test2;
        await test3;
    }

    async Task TestDelay(int count)
    {
        await Task.Delay(1000 * count);
    }

    void TestSub1()
    {
        int count = 0;
        for (int i = 0; i < int.MaxValue; i++)
            count += 1;
    }


     async Task<int> TestSub2()
    {
        await Task.Delay(1000);
        return 100;
    }
}
