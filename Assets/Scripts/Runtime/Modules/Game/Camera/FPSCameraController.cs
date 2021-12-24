using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class FPSCameraController : CameraController
{
    public enum ERecoilSpreadMode
    {
       Invalid=-1,
       T_Like,
       Triangle_Like,
    }
    public ERecoilSpreadMode E_RecoilMode = ERecoilSpreadMode.Triangle_Like;
    public bool B_SelfSetoffRecoil=true;
    public float f_angleSmoothParam = .1f;
    public float f_recoilPitchEdge=20f,f_recoilYawEdge=5f;
    protected float f_fovCurrent,f_fovStart;
    protected Counter m_RecoilTimer = new Counter();
    protected float f_recoilPitch, f_recoilYaw;
    protected float f_sprintRoll;
    protected float f_damagePitch, f_damageYaw, f_damageRoll;
    protected float m_RollAdditive;
    protected override void Awake()
    {
        base.Awake();
        f_fovStart = m_Camera.fieldOfView;
        f_fovCurrent = f_fovStart;
    }
    protected override Vector3 GetRootRotateAdditive(float _deltaTime)
    {
        f_sprintRoll = Mathf.Lerp(f_sprintRoll, 0, f_angleSmoothParam);
        f_damageRoll = Mathf.Lerp(f_damageRoll, 0, f_angleSmoothParam);
        m_RollAdditive = Mathf.Lerp(m_RollAdditive, f_sprintRoll + f_damageRoll, f_angleSmoothParam);
        if (B_SelfSetoffRecoil)
        {
            m_RecoilTimer.Tick(_deltaTime);
            if (!m_RecoilTimer.m_Counting)
            {
                f_recoilPitch = Mathf.Lerp(f_recoilPitch, 0, f_angleSmoothParam);
                f_recoilYaw = Mathf.Lerp(f_recoilYaw, 0, f_angleSmoothParam);
            }
            f_damagePitch = Mathf.Lerp(f_damagePitch, 0, f_angleSmoothParam);
            f_damageYaw = Mathf.Lerp(f_damageYaw, 0, f_angleSmoothParam);
            return new Vector3(f_recoilPitch + f_damagePitch, f_recoilYaw + f_damageYaw, m_RollAdditive);
        }
        else
        {
            m_BindRoot.Rotate(f_recoilPitch + f_damagePitch, f_recoilYaw + f_damageYaw,0, Space.Self);
            f_recoilPitch = 0;
            f_recoilYaw = 0;
            f_damagePitch = 0;
            f_damageYaw = 0;
            return Vector3.zero;
        }
    }
    public void OnSprintAnimation(float animationRoll)
    {
        f_sprintRoll = animationRoll;
    }
    public void DoDamageAnimation(Vector3 v3_PitchYawRoll)
    {
        f_damagePitch += v3_PitchYawRoll.x;
        f_damageYaw += v3_PitchYawRoll.y;
        f_damageRoll += v3_PitchYawRoll.z;
    }
    protected override void LateUpdate()
    {
        base.LateUpdate();
        m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView,f_fovCurrent,f_angleSmoothParam);
    }
    public void SetZoom(bool zoom)
    {
        f_fovCurrent = zoom ? 45 : f_fovStart;
    }
    public void AddRecoil(float recoilPitch, float recoilYaw,float fireRate)
    {
        m_RecoilTimer.Set(fireRate);
        switch (E_RecoilMode)
        {
            case ERecoilSpreadMode.Triangle_Like:
                {
                    f_recoilPitch += recoilPitch;
                    f_recoilYaw += recoilYaw;
                }
                break;
            case ERecoilSpreadMode.T_Like:
                {
                    f_recoilPitch += recoilPitch;
                    if (Mathf.Abs(f_recoilPitch) >= f_recoilPitchEdge)
                        f_recoilYaw += recoilYaw;
                }
                break;
        }

        f_recoilPitch = Mathf.Clamp(f_recoilPitch, -f_recoilPitchEdge, f_recoilPitchEdge);
        f_recoilYaw = Mathf.Clamp(f_recoilYaw, -f_recoilYawEdge, f_recoilYawEdge);
    }
}
