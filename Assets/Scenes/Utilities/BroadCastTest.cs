using UnityEngine;

namespace ExampleScenes.Utilities
{
    public class BroadCastTest : MonoBehaviour {
        
        enum EBroadCastTest
        {
            Invalid=0,
            LifeCycle,
            EnableSet,
        }
        private void Awake()
        {
            TBroadCaster<EBroadCastTest>.Add<string>(EBroadCastTest.LifeCycle, OnLifeCycle);
            TBroadCaster<EBroadCastTest>.Add<bool, string>(EBroadCastTest.EnableSet, OnEnableSet);

            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.LifeCycle, "Awake");
        }
        private void OnEnable()
        {
            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.LifeCycle, "Enable");
            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.EnableSet, true, "Enable");
        }
        private void Start()
        {
            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.LifeCycle, "Start");
        }
        private void OnDisable()
        {
            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.LifeCycle, "Disable");
            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.EnableSet, false, "Disable");
        }
        private void OnDestroy()
        {
            TBroadCaster<EBroadCastTest>.Trigger(EBroadCastTest.LifeCycle, "Destroy");

            TBroadCaster<EBroadCastTest>.Remove<string>(EBroadCastTest.LifeCycle, OnLifeCycle);
            TBroadCaster<EBroadCastTest>.Remove<bool, string>(EBroadCastTest.EnableSet, OnEnableSet);
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
}
