using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : SingletonMono<CameraController>  {
    [Range(0,1)]
    public float F_CameraRotateSmooth = .3f;
    [Range(0, 1)]
    public float F_CameraMoveSmooth = .3f;
    public bool B_InvertCamera = false;
    public float F_RotateSensitive = 1;

    public Camera m_Camera { get; private set; }
    public Transform tf_AttachTo { get; private set; }
    protected Transform tf_MainCamera;
    protected Transform tf_CameraLookAt;

    protected bool m_BindCamera;
    protected int m_YawAngleMin = 0;
    protected int m_YawAngleMax = 30;

    Vector3 m_RootForward;
    Vector3 m_RootRightward;


    public float m_Yaw { get; protected set; } = 0;
    public float m_Pitch { get; protected set; } = 0;
    public float m_Roll { get; protected set; } = 0;

    public CameraEffectManager m_Effect { get; private set; }
    #region Preset
    protected override void Awake()
    {
        base.Awake();
        m_Camera = Camera.main;
        tf_MainCamera = m_Camera.transform;
        m_Effect = m_Camera.GetComponent<CameraEffectManager>();
    }

    #endregion
    #region Interact Apis
    protected void SetCameraYawClamp(int minRotationClamp = -1, int maxRotationClamp = -1)
    {
        m_YawAngleMin = minRotationClamp;
        m_YawAngleMax = maxRotationClamp;
        m_Pitch = Mathf.Clamp(m_Pitch, m_YawAngleMin, m_YawAngleMax);
    }

    public void RotateCamera(Vector2 _input)
    {
        m_Yaw += _input.x * F_RotateSensitive;
        m_Pitch += (B_InvertCamera ? _input.y : -_input.y) * F_RotateSensitive;
        m_Pitch = Mathf.Clamp(m_Pitch, m_YawAngleMin, m_YawAngleMax);
    }

    public CameraController Attach(Transform toTransform,bool bindCamera)
    {
        m_BindCamera = bindCamera;
        tf_AttachTo = toTransform;
        return this;
    }
    public CameraController SetCameraRotation(float pitch = -1, float yaw = -1)
    {
        if (pitch != -1)
            m_Pitch = pitch;
        if (yaw != -1)
            m_Yaw = yaw;
        return this;
    }

    public CameraController SetLookAt(Transform lookAtTrans)
    {
        tf_CameraLookAt = lookAtTrans;
        return this;
    }

    public void ForceSetCamera() => UpdateCameraPositionRotation(1f,1f);


    public bool InputRayCheck(Vector2 inputPos, int layerMask, ref RaycastHit rayHit)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return false;

        return Physics.Raycast(m_Camera.ScreenPointToRay(inputPos), out rayHit, 1000, layerMask);
    }
    #endregion
    #region Calculate
    protected virtual Vector3 GetUnbindRootOffset() => Vector3.zero;
    protected virtual Quaternion GetUnbindRotation() => Quaternion.Euler(m_Pitch, m_Yaw, m_Roll);

    protected virtual void LateUpdate()
    {
        if (!tf_AttachTo)
            return;

        UpdateCameraPositionRotation(F_CameraMoveSmooth, F_CameraRotateSmooth);
    }

    void UpdateCameraPositionRotation(float moveLerp,float rotateLerp)
    {
        Vector3 cameraPosition = tf_AttachTo.position;
        Quaternion cameraRotation = tf_AttachTo.rotation;
        if(!m_BindCamera)
        {
            Matrix4x4 rootMatrix = Matrix4x4.TRS(cameraPosition, Quaternion.Euler(0, m_Yaw, 0), Vector3.one);
            cameraPosition = rootMatrix.MultiplyPoint(GetUnbindRootOffset());
            m_RootForward = rootMatrix.MultiplyVector(Vector3.forward);
            m_RootRightward = rootMatrix.MultiplyVector(Vector3.right);

            cameraRotation = tf_CameraLookAt ? Quaternion.LookRotation(tf_CameraLookAt.position - tf_MainCamera.position, Vector3.up) : GetUnbindRotation();
        }

        tf_MainCamera.position = Vector3.Lerp(tf_MainCamera.position, cameraPosition, moveLerp);
        tf_MainCamera.rotation = Quaternion.Lerp(tf_MainCamera.rotation, cameraRotation, rotateLerp);
    }

    #endregion
    #region Get/Set
    public static Camera MainCamera => Instance.m_Camera;
    public static Quaternion CameraXZRotation=> Quaternion.LookRotation(CameraXZForward, Vector3.up);
    public static Vector3 CameraXZForward => Instance.m_RootForward;
    public static Vector3 CameraXZRightward => Instance.m_RootRightward;
    public static Quaternion CameraProjectionOnPlane(Vector3 position)=> Quaternion.LookRotation(Vector3.ProjectOnPlane(position - MainCamera.transform.position, MainCamera.transform.right), MainCamera.transform.up);
    #endregion
}
