using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraEffectManager :MonoBehaviour, ICoroutineHelperClass
{
    List<CameraEffectBase> m_CameraEffects=new List<CameraEffectBase>();
    public Camera m_Camera { get; protected set; }
    public bool m_MainTextureCamera { get; private set; }
    public bool m_DepthToWorldMatrix { get; private set; } = false;
    public bool m_DoGraphicBlitz { get; private set; } = false;
    RenderTexture m_BlitzTempTexture1, m_BlitzTempTexture2;

    public CameraEffectManager Init()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.depthTextureMode = DepthTextureMode.None;
        m_DepthToWorldMatrix = false;
        m_MainTextureCamera = false;
        m_DoGraphicBlitz = false;
        m_BlitzTempTexture1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        m_BlitzTempTexture2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        return this;
    }
    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_DepthToWorldMatrix)
        {
            CalculateFrustumCornorsRay();
            CalculateViewProjectionMatrixInverse();
        }

        if(!m_DoGraphicBlitz)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, m_BlitzTempTexture1);
        for (int i = 0; i < m_CameraEffects.Count; i++)
        {
            if (! m_CameraEffects[i].m_Enabled)
                continue;

            m_CameraEffects[i].OnRenderImage(m_BlitzTempTexture1,m_BlitzTempTexture2);
            Graphics.Blit(m_BlitzTempTexture2, m_BlitzTempTexture1);
        }
        Graphics.Blit(m_BlitzTempTexture1,destination);
    }
    private void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(m_BlitzTempTexture2);
        RenderTexture.ReleaseTemporary(m_BlitzTempTexture1);
        RemoveAllPostEffect();
    }

    #region Calculations
    public float Get01Depth(Vector3 target) => m_Camera.WorldToViewportPoint(target).z / (m_Camera.farClipPlane - m_Camera.nearClipPlane);
    public float Get01DepthLength(float length) => length / (m_Camera.farClipPlane - m_Camera.nearClipPlane);
    static readonly int m_GlobalCameraDepthTextureMode = Shader.PropertyToID("_CameraDepthTextureMode");
    static readonly int ID_VPMatrixInverse = Shader.PropertyToID("_VPMatrixInverse");
    static readonly int ID_FrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
    static readonly int ID_FrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
    static readonly int ID_FrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
    static readonly int ID_FrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");

    protected void CalculateViewProjectionMatrixInverse()=>Shader.SetGlobalMatrix(ID_VPMatrixInverse, (m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix).inverse);
    protected void CalculateFrustumCornorsRay()
    {
        float fov = m_Camera.fieldOfView;
        float near = m_Camera.nearClipPlane;
        float far = m_Camera.farClipPlane;
        float aspect = m_Camera.aspect;

        Transform cameraTrans = m_Camera.transform;
        float halfHeight = near * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
        Vector3 toRight = cameraTrans.right * halfHeight * aspect;
        Vector3 toTop = cameraTrans.up * halfHeight;

        Vector3 topLeft = cameraTrans.forward * near + toTop - toRight;
        float scale = topLeft.magnitude / near;
        topLeft.Normalize();
        topLeft *= scale;

        Vector3 topRight = cameraTrans.forward * near + toTop + toRight;
        topRight.Normalize();
        topRight *= scale;

        Vector3 bottomLeft = cameraTrans.forward * near - toTop - toRight;
        bottomLeft.Normalize();
        bottomLeft *= scale;
        Vector3 bottomRight = cameraTrans.forward * near - toTop + toRight;
        bottomRight.Normalize();
        bottomRight *= scale;
        

        Shader.SetGlobalVector(ID_FrustumCornersRayBL, bottomLeft);
        Shader.SetGlobalVector(ID_FrustumCornersRayBR, bottomRight);
        Shader.SetGlobalVector(ID_FrustumCornersRayTL, topLeft);
        Shader.SetGlobalVector(ID_FrustumCornersRayTR, topRight);
        
    }
    #endregion

    #region Interact
    public T GetOrAddCameraEffect<T>() where T : CameraEffectBase, new()
    {
        T existingEffect = GetCameraEffect<T>();
        if (existingEffect != null)
            return existingEffect;

        T effectBase = new T();
        if (effectBase.m_Supported)
        {
            effectBase.InitEffect(this);
            m_CameraEffects.Add(effectBase);
            ResetCameraEffectParams();
            return effectBase;
        }
        return null;
    }

    public T GetCameraEffect<T>() where T : CameraEffectBase => m_CameraEffects.Find(p => p.GetType() == typeof(T)) as T;
    public void RemoveCameraEffect<T>() where T : CameraEffectBase, new()
    {
        T effect = GetCameraEffect<T>();
        if (effect == null)
            return;

        m_CameraEffects.Remove(effect);
        ResetCameraEffectParams();
    }
    public void RemoveAllPostEffect()
    {
        m_CameraEffects.Traversal((CameraEffectBase effect) => { effect.OnDestroy(); });
        m_CameraEffects.Clear();
        ResetCameraEffectParams();
    }

    public void SetMainTextureCamera(bool enabled)
    {
        //m_Camera.depthTextureMode = enabled ? DepthTextureMode.Depth : DepthTextureMode.None;
        m_MainTextureCamera = enabled;
        if (m_MainTextureCamera)
            GetOrAddCameraEffect<CE_MainCameraTexture>().SetTextureEnable(true, true);
        else
            RemoveCameraEffect<CE_MainCameraTexture>();
    }
    protected void ResetCameraEffectParams()
    {
        Shader.SetGlobalInt(m_GlobalCameraDepthTextureMode, m_MainTextureCamera ? 1 : 0);
        m_DoGraphicBlitz = false;
        m_DepthToWorldMatrix = false;
        m_CameraEffects.Sort((a, b) => a.m_Sorting - b.m_Sorting);
        m_CameraEffects.Traversal((CameraEffectBase effectBase) =>
        {
            if (!effectBase.m_Enabled)
                return;

            m_DoGraphicBlitz |= effectBase.m_DoGraphicBlitz;
            m_DepthToWorldMatrix |= effectBase.m_DepthFrustumCornors;
        });
    }
    public PE_DepthCircleScan StartDepthScanCircle(Vector3 origin, Color scanColor, float width = 1f, float radius = 20, float duration = 1.5f)
    {
        PE_DepthCircleScan scan = GetOrAddCameraEffect<PE_DepthCircleScan>().SetEffect(origin, scanColor);
        this.StartSingleCoroutine(0, TIEnumerators.ChangeValueTo((float value) => {
            scan.SetElapse(radius * value, width);
        }, 0, 1, duration, () => {
            RemoveCameraEffect<PE_DepthCircleScan>();
        }));
        return scan;
    }
    public PE_DepthCircleArea SetDepthAreaCircle(bool begin, Vector3 origin, float radius = 10f, float edgeWidth = .5f, float duration = 1.5f)
    {
        PE_DepthCircleArea area = GetOrAddCameraEffect<PE_DepthCircleArea>().SetOrigin(origin);
        this.StartSingleCoroutine(1, TIEnumerators.ChangeValueTo((float value) => { area.SetRadius(radius * value, edgeWidth); },
            begin ? 0 : 1, begin ? 1 : 0, duration,
            () => {
                if (!begin)
                    RemoveCameraEffect<PE_DepthCircleArea>();
            }));
        return area;
    }
    #endregion
}

#region CameraEffectBase
public enum enum_CameraEffectSorting
{
    Invalid = -1,
    Main = 1,
    CommandBuffer = 2,
    PostEffect = 3,
}
public class CameraEffectBase
{
    public virtual enum_CameraEffectSorting m_Sorting => enum_CameraEffectSorting.Invalid;
    public virtual bool m_DepthFrustumCornors => false;
    public virtual bool m_DoGraphicBlitz => false;
    protected CameraEffectManager m_Manager { get; private set; }
    public bool m_Supported { get; private set; }
    public bool m_Enabled { get; protected set; }
    public CameraEffectBase()
    {
        m_Supported = Init();
    }
    protected virtual bool Init()
    {
        return true;
    }
    public virtual void InitEffect(CameraEffectManager _manager)
    {
        m_Manager = _manager;
        m_Enabled = true;
    }
    public virtual void SetEnable(bool enable) => m_Enabled = enable;
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
    }

    public virtual void OnDestroy()
    {
    }
}
public class CE_MainCameraTexture : CameraEffectBase        //Depth Texture Replacement
{
    public override enum_CameraEffectSorting m_Sorting => enum_CameraEffectSorting.Main;
    public bool m_DepthTextureEnabled { get; private set; } = false;
    public bool m_OpaqueTextureEnabled { get; private set; } = false;
    readonly int ID_GlobalDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
    readonly int ID_GlobalOpaqueTexture = Shader.PropertyToID("_CameraOpaqueTexture");
    CommandBuffer m_DepthTextureBuffer, m_OpaqueTextureBuffer;
    RenderTexture m_ColorBuffer, m_DepthBuffer, m_DepthTexture, m_OpaqueTexture;
    public override bool m_DoGraphicBlitz => true;
    public override void InitEffect(CameraEffectManager _manager)
    {
        base.InitEffect(_manager);
        m_ColorBuffer = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 0, RenderTextureFormat.RGB111110Float);
        m_ColorBuffer.name = "Main Color Buffer";
        m_DepthBuffer = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 24, RenderTextureFormat.Depth);
        m_DepthBuffer.name = "Main Depth Buffer";
        m_Manager.m_Camera.SetTargetBuffers(m_ColorBuffer.colorBuffer, m_DepthBuffer.depthBuffer);

        m_DepthTextureBuffer = new CommandBuffer() { name = "Depth Texture Copy" };
        m_OpaqueTextureBuffer = new CommandBuffer() { name = "Opaque Texture Copy" };
        m_Manager.m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthTextureBuffer);
        m_Manager.m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_OpaqueTextureBuffer);
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(m_ColorBuffer, destination);
    }
    public CE_MainCameraTexture SetTextureEnable(bool depthTexture, bool opaqueTexture)
    {
        m_DepthTextureEnabled = depthTexture;
        m_DepthTextureBuffer.Clear();
        RenderTexture.ReleaseTemporary(m_DepthTexture);
        if (m_DepthTextureEnabled)
        {
            m_DepthTexture = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 0, RenderTextureFormat.RFloat);
            m_DepthTexture.name = "Opaque Depth Texture";

            m_DepthTextureBuffer.Blit(m_DepthBuffer.depthBuffer, m_DepthTexture.colorBuffer);
            m_DepthTextureBuffer.SetGlobalTexture(ID_GlobalDepthTexture, m_DepthTexture);
        }

        m_OpaqueTextureEnabled = opaqueTexture;
        m_OpaqueTextureBuffer.Clear();
        RenderTexture.ReleaseTemporary(m_OpaqueTexture);
        if (m_OpaqueTextureEnabled)
        {
            m_OpaqueTexture = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth, m_Manager.m_Camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
            m_OpaqueTexture.name = "Opaque Texture";

            m_OpaqueTextureBuffer.Blit(m_ColorBuffer, m_OpaqueTexture);
            m_OpaqueTextureBuffer.SetGlobalTexture(ID_GlobalOpaqueTexture, m_OpaqueTexture);
        }
        return this;
    }


    public override void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(m_DepthTexture);
        RenderTexture.ReleaseTemporary(m_OpaqueTexture);
        RenderTexture.ReleaseTemporary(m_ColorBuffer);
        RenderTexture.ReleaseTemporary(m_DepthBuffer);
        m_Manager.m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthTextureBuffer);
        m_Manager.m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_OpaqueTextureBuffer);
        m_Manager.m_Camera.targetTexture = null;
        base.OnDestroy();
    }
}
public class PostEffectBase : CameraEffectBase
{
    public override enum_CameraEffectSorting m_Sorting => enum_CameraEffectSorting.PostEffect;
    const string S_ParentPath = "Hidden/PostEffect/";
    public Material m_Material { get; private set; }
    public override bool m_DoGraphicBlitz => true;
    protected override bool Init()
    {
        m_Material = CreateMaterial(this.GetType());
        return m_Material != null;
    }

    public static Material CreateMaterial(Type type)
    {
        try
        {
            Shader shader = Shader.Find(S_ParentPath + type.ToString());
            if (shader == null)
                throw new Exception("Shader:" + S_ParentPath + type.ToString() + " Not Found");
            if (!shader.isSupported)
                throw new Exception("Shader:" + S_ParentPath + type.ToString() + " Is Not Supported");

            return new Material(shader) { hideFlags = HideFlags.DontSave };
        }
        catch (Exception e)
        {
            Debug.LogError("Post Effect Error:" + e.Message);
            return null;
        }
    }

    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, m_Material);
    }
    public override void OnDestroy()
    {
        GameObject.Destroy(m_Material);
    }
}
#endregion