using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroadCastTest : MonoBehaviour {
    
    enum enum_BroadCastTest
    {
        Invalid=0,
        Test1,
        Test2,
        Test3,
        Test4,
        Test5,
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
            TBroadCaster<enum_BroadCastTest>.Get(enum_BroadCastTest.Test1).Trigger();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TBroadCaster<enum_BroadCastTest>.Get<int>(enum_BroadCastTest.Test2).Trigger(10);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TBroadCaster<enum_BroadCastTest>.Get<string,string>(enum_BroadCastTest.Test3).Trigger("Howdy","My Friend");
        if (Input.GetKeyDown(KeyCode.Alpha4))
            TBroadCaster<enum_BroadCastTest>.Get<int,int,float>(enum_BroadCastTest.Test4).Trigger(10, 20,30f);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            TBroadCaster<enum_BroadCastTest>.Get<int, int, float,BroadCastTest>(enum_BroadCastTest.Test5).Trigger(10, 20, 30f,this);

        if (Input.GetKeyDown(KeyCode.Q))
            TBroadCaster<enum_BroadCastTest>.Get(enum_BroadCastTest.Test1).Add(OnTest1);
        if (Input.GetKeyDown(KeyCode.W))
            TBroadCaster<enum_BroadCastTest>.Get<int>(enum_BroadCastTest.Test2).Add(OnTest2);
        if (Input.GetKeyDown(KeyCode.E))
            TBroadCaster<enum_BroadCastTest>.Get<string,string>(enum_BroadCastTest.Test3).Add(OnTest3);
        if (Input.GetKeyDown(KeyCode.R))
            TBroadCaster<enum_BroadCastTest>.Get<int, int, float>(enum_BroadCastTest.Test4).Add(OnTest4);
        if (Input.GetKeyDown(KeyCode.T))
            TBroadCaster<enum_BroadCastTest>.Get<int, int, float, BroadCastTest>(enum_BroadCastTest.Test5).Add(OnTest5);

        if (Input.GetKeyDown(KeyCode.A))
            TBroadCaster<enum_BroadCastTest>.Get(enum_BroadCastTest.Test1).Remove(OnTest1);
        if (Input.GetKeyDown(KeyCode.S))
            TBroadCaster<enum_BroadCastTest>.Get<int>(enum_BroadCastTest.Test2).Remove(OnTest2);
        if (Input.GetKeyDown(KeyCode.D))
            TBroadCaster<enum_BroadCastTest>.Get<string, string>(enum_BroadCastTest.Test3).Remove(OnTest3);
        if (Input.GetKeyDown(KeyCode.F))
            TBroadCaster<enum_BroadCastTest>.Get<int, int, float>(enum_BroadCastTest.Test4).Remove(OnTest4);
        if (Input.GetKeyDown(KeyCode.G))
            TBroadCaster<enum_BroadCastTest>.Get<int, int, float, BroadCastTest>(enum_BroadCastTest.Test5).Remove(OnTest5);

    }

    void OnTest1()
    {
        Debug.LogError("Triggered");
    }

    void OnTest2(int identity)
    {
        Debug.LogWarning(identity);
    }

    void OnTest3(string test1,string test2)
    {
        Debug.Log(test1 + " " + test2);
    }

    void OnTest4(int test1,int test2,float test3)
    {
        Debug.Log(test1 + test2 - test3);
    }

    void OnTest5(int test1, int test2, float test3,BroadCastTest test4)
    {
        Debug.Log(test1 + test2 + test3 + test4.name);
    }
}
