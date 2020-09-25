using UnityEngine;

public class BroadCastTest : MonoBehaviour {
    
    enum enum_BroadCastTest
    {
        Invalid=0,
        LifeCycle,
        EnableSet,
    }
    private void Awake()
    {
        TBroadCaster<enum_BroadCastTest>.Add<string>(enum_BroadCastTest.LifeCycle, OnLifeCycle);
        TBroadCaster<enum_BroadCastTest>.Add<bool, string>(enum_BroadCastTest.EnableSet, OnEnableSet);

        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.LifeCycle, "Awake");
    }
    private void OnEnable()
    {
        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.LifeCycle, "Enable");
        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.EnableSet, true, "Enable");
    }
    private void Start()
    {
        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.LifeCycle, "Start");
    }
    private void OnDisable()
    {
        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.LifeCycle, "Disable");
        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.EnableSet, false, "Disable");
    }
    private void OnDestroy()
    {
        TBroadCaster<enum_BroadCastTest>.Trigger(enum_BroadCastTest.LifeCycle, "Destroy");

        TBroadCaster<enum_BroadCastTest>.Remove<string>(enum_BroadCastTest.LifeCycle, OnLifeCycle);
        TBroadCaster<enum_BroadCastTest>.Remove<bool, string>(enum_BroadCastTest.EnableSet, OnEnableSet);
    }
    void OnLifeCycle(string cycle)
    {
        Debug.Log("Life Cycle:"+cycle);
    }

    void OnEnableSet(bool enable,string test)
    {
        Debug.LogWarning("Enable Cycle:"+enable + "," + test);
    }

}
