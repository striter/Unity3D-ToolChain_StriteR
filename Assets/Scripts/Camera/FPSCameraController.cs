using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCameraController : CameraController
{
    public enum enum_RecoilSpreadMode
    {
       Invalid=-1,
       T_Like,
       Triangle_Like,
    }
    protected static FPSCameraController ninstance;
    public static new FPSCameraController Instance => ninstance;

    public enum_RecoilSpreadMode E_RecoilMode = enum_RecoilSpreadMode.Triangle_Like;
    public int I_PitchMin=-90, I_PitchMax=90;
    public bool B_SelfSetoffRecoil=true;
    public float f_angleSmoothParam = .1f;
    public float f_recoilPitchEdge=20f,f_recoilYawEdge=5f;
    protected float f_fovCurrent,f_fovStart;
    protected float f_recoilAutoSetoff;
    protected float f_recoilPitch, f_recoilYaw;
    protected float f_sprintRoll;
    protected float f_damagePitch, f_damageYaw, f_damageRoll;
    protected override void Awake()
    {
        base.Awake();
        ninstance = this;
        SetCameraYawClamp(I_PitchMin,I_PitchMax);
        m_BindCamera = true;

        f_fovStart = m_Camera.fieldOfView;
        f_fovCurrent = f_fovStart;
    }
    protected override Quaternion GetUnbindRotation()
    {
        f_sprintRoll = Mathf.Lerp(f_sprintRoll, 0, f_angleSmoothParam);
        f_damageRoll = Mathf.Lerp(f_damageRoll, 0, f_angleSmoothParam);
        m_Roll = Mathf.Lerp(m_Roll, f_sprintRoll + f_damageRoll, f_angleSmoothParam);
        if (B_SelfSetoffRecoil)
        {
            if (Time.time > f_recoilAutoSetoff)
            {
                f_recoilPitch = Mathf.Lerp(f_recoilPitch, 0, f_angleSmoothParam);
                f_recoilYaw = Mathf.Lerp(f_recoilYaw, 0, f_angleSmoothParam);
            }
            f_damagePitch = Mathf.Lerp(f_damagePitch, 0, f_angleSmoothParam);
            f_damageYaw = Mathf.Lerp(f_damageYaw, 0, f_angleSmoothParam);
            return Quaternion.Euler(m_Pitch + f_recoilPitch + f_damagePitch, m_Yaw + f_recoilYaw + f_damageYaw, m_Roll);
        }
        else
        {
            m_Pitch += f_recoilPitch;
            m_Yaw += f_recoilYaw;
            f_recoilPitch = 0;
            f_recoilYaw = 0;
            m_Pitch += f_damagePitch;
            m_Yaw += f_damageYaw;
            f_damagePitch = 0;
            f_damageYaw = 0;
            return base.GetUnbindRotation();
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
        f_recoilAutoSetoff = Time.time + fireRate;
        switch (E_RecoilMode)
        {
            case enum_RecoilSpreadMode.Triangle_Like:
                {
                    f_recoilPitch += recoilPitch;
                    f_recoilYaw += recoilYaw;
                }
                break;
            case enum_RecoilSpreadMode.T_Like:
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
