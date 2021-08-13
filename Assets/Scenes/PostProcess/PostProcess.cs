using System;
using Rendering.PostProcess;
using UnityEngine;
using TTouchTracker;
using UnityEngine.UIElements;

public class PostProcess : MonoBehaviour
{
    PostProcess_Opaque m_Controller;
    Camera m_ControlCamera;
    SingleCoroutine m_AreaCoroutine;
    Vector3 m_AreaOrigin;
    float m_AreaRadius;
    private void Awake()
    {
        m_AreaCoroutine = CoroutineHelper.CreateSingleCoroutine();
        m_Controller = GetComponentInChildren<PostProcess_Opaque>();
        m_ControlCamera = m_Controller.GetComponent<Camera>();
        UIT_TouchConsole.InitDefaultCommands();
        foreach(var postEffects in GetComponentsInChildren<APostProcessBase>())
        {
            UIT_TouchConsole.NewPage(postEffects.GetType(). Name);
            UIT_TouchConsole.InitSerializeCommands(postEffects,effect=>effect.OnValidate());
        }
        TouchTracker.Init();
    }

    private void Update()
    {
        var tracks=TouchTracker.Execute(Time.unscaledDeltaTime);
        foreach (var click in tracks.ResolveClicks())
        {
            if (m_ControlCamera.InputRayCheck(click, out RaycastHit _hit))
                m_Controller.StartDepthScanCircle(_hit.point, 10f, 1f);
        }
        
    }

    void OnTouchCheck(bool down, Vector2 stretch1Pos,Vector2 strech2Pos)
    {
        m_AreaCoroutine.Stop();
        if (down)
        {
            m_AreaRadius= 0f;
            m_AreaOrigin = Vector3.zero;
            return;
        }
        
        if (m_AreaRadius == 0)
            return;
        m_Controller.m_Data.m_ScanData.m_Origin = m_AreaOrigin;
        m_AreaCoroutine.Start(TIEnumerators.ChangeValueTo((float value) =>
        {
            m_Controller.m_Data.m_Area = true;
            m_Controller.m_Data.m_AreaData.m_Radius = m_AreaRadius * value;
            m_Controller.OnValidate();
        }, 1,0, .2f, () =>
        {
            m_Controller.m_Data.m_Area = false;
            m_Controller.OnValidate();
        }));
    }
    void OnPressCheck(Vector2 stretch1Pos, Vector2 strech2Pos)
    {
        if (m_ControlCamera.InputRayCheck(stretch1Pos, out RaycastHit _hit1)&&m_ControlCamera.InputRayCheck(strech2Pos,out RaycastHit _hit2))
        {
            m_AreaOrigin = (_hit1.point + _hit2.point) / 2;
            m_AreaRadius = Vector3.Distance( _hit2.point,_hit1.point)/2f;
            m_Controller.m_Data.m_AreaData.m_Radius = m_AreaRadius;
            m_Controller.m_Data.m_AreaData.m_Origin = m_AreaOrigin;
            m_Controller.OnValidate();
        }

    }
}
