using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDataPersistent;
using System;
using UnityEngine.UI;
using Rendering.ImageEffect;
public class Test : MonoBehaviour
{
    public Vector3 m_SrcVector = Vector3.one;
    public Vector3 m_DstVector = Vector3.down;
    public float m_RotateAngle;
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawLine(Vector3.zero, m_SrcVector);
        Gizmos.DrawLine(Vector3.zero, m_DstVector);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero,UAlthogrim.UQuaternion.AngleAxisToRotateMatrix(m_RotateAngle,m_DstVector)*m_SrcVector);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, UAlthogrim.UQuaternion.AngleAxisToQuaternion(m_RotateAngle, m_DstVector)*m_SrcVector);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, Quaternion.AngleAxis(m_RotateAngle, m_DstVector)* m_SrcVector);
        //Debug.Log(TVector.SqrMagnitude(m_SrcVector) + " " +  m_SrcVector.sqrMagnitude);
        //Debug.Log(TVector.Dot(m_SrcVector, m_DstVector) + " " + Vector3.Dot(m_SrcVector, m_DstVector));
        //Debug.Log(TVector.Project(m_SrcVector, m_DstVector) + " " + Vector3.Project(m_SrcVector, m_DstVector));
        //Debug.Log(TVector.Cross(m_SrcVector, m_DstVector) + " " + Vector3.Cross(m_SrcVector, m_DstVector));
    }
    public SaveTest m_SaveTest = new SaveTest();
    private void Start()
    {

        UIT_TouchConsole.InitDefaultCommands();
        UIT_TouchConsole.Header("Data Save");
        UIT_TouchConsole.Command("Read").Button(() => { m_SaveTest.ReadPersistentData(); Debug.Log(m_SaveTest.Test1); });
        UIT_TouchConsole.Command("Save").Slider(0,10,m_SaveTest.Test1, value =>
        {
            m_SaveTest.Test1 = value;
            m_SaveTest.m_Test1.m_Test1 = value * value;
            m_SaveTest.SavePersistentData();
        });

        UIT_TouchConsole.Header("Touch Input");
        UIT_TouchConsole.Command("Single").
            Button(() => TouchInputManager.Instance.SwitchToSingle().Init((down, pos) => Debug.LogFormat("Single{0}{1}", down, pos), pos => Debug.LogFormat("Single Tick{0}", pos)));
        UIT_TouchConsole.Command("Stretch").
            Button(() => TouchInputManager.Instance.SwitchToDualStretch().Init((down, pos1, pos2) => Debug.LogFormat("Stretch{0},{1},{2}", down, pos1, pos2), (pos1, pos2) => Debug.LogFormat("Stretch Tick{0} {1}", pos1, pos2)));
        UIT_TouchConsole.Command("Dual LR").
            Button(() => TouchInputManager.Instance.SwitchToTrackers().Init(new TouchTracker(vec2 => Debug.LogFormat("Dual L{0}", vec2), TouchTracker.s_LeftTrack), new TouchTracker(vec2 => Debug.LogFormat("Dual R{0}", vec2), TouchTracker.s_RightTrack)));
        UIT_TouchConsole.Command("Dual LR Joystick").
            Button(() => TouchInputManager.Instance.SwitchToTrackers().Init(new TouchTracker_Joystick(UIT_TouchConsole.GetHelperJoystick(), enum_Option_JoyStickMode.Retarget, vec2 => Debug.LogFormat("Dual L Joystick{0}", vec2), TouchTracker.s_LeftTrack), new TouchTracker(vec2 => Debug.LogFormat("Dual R Joystick{0}", vec2), TouchTracker.s_RightTrack)));

        UIT_TouchConsole.EmptyLine();
        UIT_TouchConsole.InitSerializeCommands(Camera.main.GetComponent<PostEffect_ColorGrading>(), effect => effect.OnValidate());
        UIT_TouchConsole.EmptyLine();
        UIT_TouchConsole.InitSerializeCommands(Camera.main.GetComponent<PostEffect_DepthOfField>(), effect => effect.OnValidate());
        UIT_TouchConsole.EmptyLine();
        UIT_TouchConsole.InitSerializeCommands(Camera.main.GetComponent<PostEffect_VHS>(), effect => effect.OnValidate());
    }
    [Serializable]
    public class SaveTest : CDataSave<SaveTest>
    {
        public float Test1;
        public string Test2;
        public SaveTest1 m_Test1;
        public override bool DataCrypt() => true;
    }
    [Serializable]
    public struct SaveTest1
    {
        public float m_Test1;
        public Dictionary<int, string> m_Test4;
    }
}

