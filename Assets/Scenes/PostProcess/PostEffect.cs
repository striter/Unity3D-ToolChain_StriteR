using System.Collections;
using System.Collections.Generic;
using Rendering.ImageEffect;
using UnityEngine;

public class PostEffect : MonoBehaviour
{
    PostProcess_ScanArea m_CircleArea;
    Camera m_CircleAreaCamera;
    SingleCoroutine m_AreaCoroutine;
    Vector3 m_AreaOrigin;
    float m_AreaRadius;
    private void Awake()
    {
        m_AreaCoroutine = CoroutineHelper.CreateSingleCoroutine();
        m_CircleArea = GetComponentInChildren<PostProcess_ScanArea>();
        m_CircleAreaCamera = m_CircleArea.GetComponent<Camera>();
        UIT_TouchConsole.InitDefaultCommands();
        foreach(var postEffects in GetComponentsInChildren<APostProcessBase>())
        {
            UIT_TouchConsole.NewPage(postEffects.GetType(). Name);
            UIT_TouchConsole.InitSerializeCommands(postEffects,effect=>effect.OnValidate());
        }
    }

    void OnTouchCheck(bool down, Vector2 stretch1Pos,Vector2 strech2Pos)
    {
        m_CircleArea.enabled = true;
        m_AreaCoroutine.Stop();
        if (down)
        {
            PostProcess_ScanCircle scan = GetComponentInChildren<PostProcess_ScanCircle>();
            Camera scanCamera = scan.GetComponent<Camera>();
            if (scanCamera.InputRayCheck(stretch1Pos, out RaycastHit _hit))
                scan.StartDepthScanCircle(_hit.point, 10f, 1f);

            m_AreaRadius= 0f;
            m_AreaOrigin = Vector3.zero;
        }
        else
        {
            if (m_AreaRadius == 0)
                return;
            m_CircleArea.m_EffectData.m_Origin = m_AreaOrigin;
            m_AreaCoroutine.Start(TIEnumerators.ChangeValueTo((float value) => {
                m_CircleArea.m_EffectData.m_Radius = m_AreaRadius * value;
                m_CircleArea.OnValidate();
            }, 1,0, .2f, () => { m_CircleArea.enabled = false; }));
        }
    }
    void OnPressCheck(Vector2 stretch1Pos, Vector2 strech2Pos)
    {
        if (m_CircleAreaCamera.InputRayCheck(stretch1Pos, out RaycastHit _hit1)&&m_CircleAreaCamera.InputRayCheck(strech2Pos,out RaycastHit _hit2))
        {
            m_AreaOrigin = (_hit1.point + _hit2.point) / 2;
            m_AreaRadius = Vector3.Distance( _hit2.point,_hit1.point)/2f;
            m_CircleArea.m_EffectData.m_Radius = m_AreaRadius;
            m_CircleArea.m_EffectData.m_Origin = m_AreaOrigin;
            m_CircleArea.OnValidate();
        }

    }
}
