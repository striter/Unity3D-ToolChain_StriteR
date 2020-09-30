using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Legacy
{ 
#region CommandBuffer
public class CommandBufferBase : CameraEffectBase
{
    public override enum_CameraEffectSorting m_Sorting => enum_CameraEffectSorting.CommandBuffer;
    protected CommandBuffer m_Buffer;
    protected virtual CameraEvent m_BufferEvent => 0;
    public override void InitEffect(CameraEffectManager _manager)
    {
        base.InitEffect(_manager);
        m_Buffer = new CommandBuffer();
        m_Buffer.name = this.GetType().ToString();
        m_Manager.m_Camera.AddCommandBuffer(m_BufferEvent, m_Buffer);
    }
    public override void OnDestroy()
    {
        m_Buffer.Clear();
        m_Manager.m_Camera.RemoveCommandBuffer(m_BufferEvent, m_Buffer);
    }
}

public class CB_GenerateTransparentOverlayTexture : CommandBufferBase
{
    protected override CameraEvent m_BufferEvent => CameraEvent.BeforeImageEffects;
    public bool m_TransparentBlurTextureEnabled { get; private set; } = false;
    readonly int ID_GlobalTransparentBlurTexure = Shader.PropertyToID("_CameraUIOverlayBlurTexture");
    RenderTexture m_TransparentBlurTexture1, m_TransparentBlurTexture2;
    public PE_Blurs m_Blur { get; private set; }
    public override void InitEffect(CameraEffectManager _manager)
    {
        base.InitEffect(_manager);
        m_Blur = new PE_Blurs();
        m_Blur.InitEffect(_manager);
    }
    public CB_GenerateTransparentOverlayTexture SetOpaqueBlurTextureEnabled(bool enable, float blurSpread = 1.5f, int downSample = 2, int iteration = 3)
    {
        m_TransparentBlurTextureEnabled = enable;
        m_Blur.SetEffect(PE_Blurs.enum_BlurType.GaussianBlur, blurSpread);
        m_Buffer.Clear();
        ReleaseTexture();
        if (m_TransparentBlurTextureEnabled)
        {
            m_TransparentBlurTexture1 = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth / downSample, m_Manager.m_Camera.pixelHeight / downSample, 0, RenderTextureFormat.ARGB32);
            m_TransparentBlurTexture1.filterMode = FilterMode.Bilinear;
            m_TransparentBlurTexture1.name = "Transparent Blur Copy 1";

            m_TransparentBlurTexture2 = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth / downSample, m_Manager.m_Camera.pixelHeight / downSample, 0, RenderTextureFormat.ARGB32);
            m_TransparentBlurTexture2.filterMode = FilterMode.Bilinear;
            m_TransparentBlurTexture2.name = "Transparent Blur Copy 1";

            m_Buffer.Blit(BuiltinRenderTextureType.CurrentActive, m_TransparentBlurTexture1);
            for (int i = 0; i < iteration; i++)
            {
                m_Buffer.Blit(m_TransparentBlurTexture1, m_TransparentBlurTexture2, m_Blur.m_Material, 1);
                m_Buffer.Blit(m_TransparentBlurTexture2, m_TransparentBlurTexture1, m_Blur.m_Material, 2);
            }
            m_Buffer.SetGlobalTexture(ID_GlobalTransparentBlurTexure, m_TransparentBlurTexture1);
        }
        return this;
    }

    void ReleaseTexture()
    {
        m_Blur.SetEffect(PE_Blurs.enum_BlurType.Invalid);
        if (!m_TransparentBlurTexture1)
            return;
        RenderTexture.ReleaseTemporary(m_TransparentBlurTexture1);
        RenderTexture.ReleaseTemporary(m_TransparentBlurTexture2);
        m_TransparentBlurTexture1 = null;
        m_TransparentBlurTexture2 = null;
    }

    public override void OnDestroy()
    {
        m_Blur.OnDestroy();
        ReleaseTexture();
        base.OnDestroy();
    }
}
public class CB_DepthOfFieldSpecificStatic : CommandBufferBase
{
    public PE_Blurs m_GaussianBlur { get; private set; }
    List<Renderer> m_targets = new List<Renderer>();
    protected override CameraEvent m_BufferEvent => CameraEvent.AfterImageEffects;
    public override void InitEffect(CameraEffectManager _manager)
    {
        base.InitEffect(_manager);
        m_GaussianBlur = new PE_Blurs();
        m_GaussianBlur.InitEffect(_manager);
    }
    public void SetStaticTarget(params Renderer[] targets)
    {
        targets.Traversal((Renderer renderer) => {
            if (m_targets.Contains(renderer))
                return;
            m_targets.Add(renderer);
            renderer.enabled = false;
            m_Buffer.DrawRenderer(renderer, renderer.sharedMaterial);
        });
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        m_GaussianBlur.OnRenderImage(source, destination);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        m_GaussianBlur.OnDestroy();
        m_targets.Traversal((Renderer renderer) => {
            renderer.enabled = true;
        });
        m_targets.Clear();
    }
}
#endregion
#region PostEffect
public class PE_Blurs : PostEffectBase       //Blur Base Collection
{
    public enum enum_BlurType
    {
        Invalid = -1,
        AverageBlur,
        GaussianBlur,
    }
    enum enum_BlurPass
    {
        Invalid = -1,
        Average = 0,
        GaussianHorizontal = 1,
        GaussianVertical = 2,
    }
    public enum_BlurType m_BlurType { get; private set; } = enum_BlurType.Invalid;
    float F_BlurSpread;
    int I_Iterations;
    RenderTexture buffer0, buffer1;
    int m_textureWidth, m_textureHeight;
    public void SetEffect(enum_BlurType blurType = enum_BlurType.AverageBlur, float _blurSpread = 2f, int _iterations = 5, int _downSample = 4)
    {
        m_BlurType = blurType;
        bool enable = m_BlurType != enum_BlurType.Invalid;
        if (enable)
        {
            F_BlurSpread = _blurSpread;
            I_Iterations = _iterations;
            _downSample = _downSample > 0 ? _downSample : 1;
            m_textureWidth = m_Manager.m_Camera.scaledPixelWidth >> _downSample;
            m_textureHeight = m_Manager.m_Camera.scaledPixelHeight >> _downSample;
        }
        EnableTextures(enable);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EnableTextures(false);
    }

    void EnableTextures(bool enable)
    {
        if (buffer0) RenderTexture.ReleaseTemporary(buffer0);
        if (buffer1) RenderTexture.ReleaseTemporary(buffer1);
        buffer0 = null;
        buffer1 = null;
        if (!enable)
            return;

        buffer0 = RenderTexture.GetTemporary(m_textureWidth, m_textureHeight, 0);
        buffer0.filterMode = FilterMode.Bilinear;
        buffer1 = RenderTexture.GetTemporary(m_textureWidth, m_textureHeight, 0);
        buffer0.filterMode = FilterMode.Bilinear;
    }

    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_BlurType == enum_BlurType.Invalid)
        {
            Debug.LogError("Invalid Blur Detected!");
            Graphics.Blit(source, destination);
            return;
        }
        RenderTexture targetBuffer = null;
        Graphics.Blit(source, buffer0);
        for (int i = 0; i < I_Iterations; i++)
        {
            m_Material.SetFloat("_BlurSpread", 1 + i * F_BlurSpread);
            switch (m_BlurType)
            {
                case enum_BlurType.AverageBlur:
                    Graphics.Blit(buffer0, buffer1, m_Material, (int)enum_BlurPass.Average);
                    targetBuffer = buffer1;
                    break;
                case enum_BlurType.GaussianBlur:
                    Graphics.Blit(buffer0, buffer1, m_Material, (int)enum_BlurPass.GaussianHorizontal);
                    Graphics.Blit(buffer1, buffer0, m_Material, (int)enum_BlurPass.GaussianVertical);
                    targetBuffer = buffer0;
                    break;
            }
        }
        Graphics.Blit(targetBuffer, destination);
    }
}
public class PE_MotionBlur : PostEffectBase     //Camera Motion Blur ,Easiest
{
    private RenderTexture rt_Accumulation;
    public void SetEffect(float _BlurSize = 1f)
    {
        m_Material.SetFloat("_BlurAmount", 1 - _BlurSize);
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (rt_Accumulation == null || rt_Accumulation.width != source.width || rt_Accumulation.height != source.height)
        {
            GameObject.Destroy(rt_Accumulation);
            rt_Accumulation = new RenderTexture(source.width, source.height, 0);
            rt_Accumulation.hideFlags = HideFlags.DontSave;
            Graphics.Blit(source, rt_Accumulation);
        }

        Graphics.Blit(source, rt_Accumulation, m_Material);
        Graphics.Blit(rt_Accumulation, destination);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (rt_Accumulation != null)
            GameObject.Destroy(rt_Accumulation);
    }
}
public class PE_MotionBlurDepth : PE_MotionBlur
{
    private Matrix4x4 mt_CurVP;
    public override void InitEffect(CameraEffectManager _manager)
    {
        base.InitEffect(_manager);
        mt_CurVP = m_Manager.m_Camera.projectionMatrix * m_Manager.m_Camera.worldToCameraMatrix;
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        base.OnRenderImage(source, destination);
        m_Material.SetMatrix("_PreviousVPMatrix", mt_CurVP);
        mt_CurVP = m_Manager.m_Camera.projectionMatrix * m_Manager.m_Camera.worldToCameraMatrix;
        m_Material.SetMatrix("_CurrentVPMatrixInverse", mt_CurVP.inverse);
        Graphics.Blit(source, destination, m_Material);
    }
}
public class PE_DepthCircleScan : PostEffectBase
{
    public override bool m_DepthFrustumCornors => true;
    readonly int ID_Origin = Shader.PropertyToID("_Origin");
    readonly int ID_Color = Shader.PropertyToID("_Color");
    readonly int ID_Texture = Shader.PropertyToID("_Texture");
    readonly int ID_TexScale = Shader.PropertyToID("_TextureScale");
    readonly int ID_MinSqrDistance = Shader.PropertyToID("_MinSqrDistance");
    readonly int ID_MaxSqrDistance = Shader.PropertyToID("_MaxSqrDistance");

    public void SetElapse(float elapse, float width)
    {
        float minDistance = elapse - width;
        float maxDistance = elapse;
        m_Material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
        m_Material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
    }

    public PE_DepthCircleScan SetEffect(Vector3 origin, Color scanColor)
    {
        m_Material.SetVector(ID_Origin, origin);
        m_Material.SetColor(ID_Color, scanColor);
        return this;
    }

    public PE_DepthCircleScan SetTexture(Texture scanTex = null, float _scanTexScale = 15f)
    {
        m_Material.SetTexture(ID_Texture, scanTex);
        m_Material.SetFloat(ID_TexScale, _scanTexScale);
        return this;
    }
}

public class PE_DepthCircleArea : PostEffectBase
{
    public override bool m_DepthFrustumCornors => true;

    readonly int ID_Origin = Shader.PropertyToID("_Origin");
    readonly int ID_FillColor = Shader.PropertyToID("_FillColor");
    readonly int ID_FillTexture = Shader.PropertyToID("_FillTexture");
    readonly int ID_FillTextureScale = Shader.PropertyToID("_TextureScale");
    readonly int ID_FillTextureFlow = Shader.PropertyToID("_TextureFlow");
    readonly int ID_EdgeColor = Shader.PropertyToID("_EdgeColor");
    readonly int ID_SqrEdgeMin = Shader.PropertyToID("_SqrEdgeMin");
    readonly int ID_SqrEdgeMax = Shader.PropertyToID("_SqrEdgeMax");

    public void SetRadius(float radius, float edge)
    {
        float edgeMax = radius;
        float edgeMin = radius - edge;
        m_Material.SetFloat(ID_SqrEdgeMax, edgeMax * edgeMax);
        m_Material.SetFloat(ID_SqrEdgeMin, edgeMin * edgeMin);
    }

    public PE_DepthCircleArea SetOrigin(Vector3 origin)
    {
        m_Material.SetVector(ID_Origin, origin);
        return this;
    }

    public PE_DepthCircleArea SetColor(Color fillColor, Color edgeColor)
    {
        m_Material.SetColor(ID_FillColor, fillColor);
        m_Material.SetColor(ID_EdgeColor, edgeColor);
        return this;
    }

    public PE_DepthCircleArea SetTexture(Texture fillTex, float texScale, Vector2 texFlow)
    {
        m_Material.SetTexture(ID_FillTexture, fillTex);
        m_Material.SetFloat(ID_FillTextureScale, texScale);
        m_Material.SetVector(ID_FillTextureFlow, texFlow);
        return this;
    }
}

public class PE_DepthSSAO : PostEffectBase
{
    static readonly Vector4[] m_DepthSampleArray = new Vector4[16] {
            new Vector3( 0.5381f, 0.1856f,-0.4319f),  new Vector3( 0.1379f, 0.2486f, 0.4430f),new Vector3( 0.3371f, 0.5679f,-0.0057f),  new Vector3(-0.6999f,-0.0451f,-0.0019f),
            new Vector3( 0.0689f,-0.1598f,-0.8547f),  new Vector3( 0.0560f, 0.0069f,-0.1843f),new Vector3(-0.0146f, 0.1402f, 0.0762f),  new Vector3( 0.0100f,-0.1924f,-0.0344f),
            new Vector3(-0.3577f,-0.5301f,-0.4358f),  new Vector3(-0.3169f, 0.1063f, 0.0158f),new Vector3( 0.0103f,-0.5869f, 0.0046f),  new Vector3(-0.0897f,-0.4940f, 0.3287f),
            new Vector3( 0.7119f,-0.0154f,-0.0918f),  new Vector3(-0.0533f, 0.0596f,-0.5411f),new Vector3( 0.0352f,-0.0631f, 0.5460f),  new Vector3(-0.4776f, 0.2847f,-0.0271f)};
    public override bool m_DoGraphicBlitz => false;

    public PE_DepthSSAO SetEffect(Color aoColor, float strength = 1f, float texelRadius = 15f, float _fallOff = 0.002f, Texture _noiseTex = null, int _sampleCount = 16)
    {
        Vector4[] array = new Vector4[m_DepthSampleArray.Length];
        for (int i = 0; i < m_DepthSampleArray.Length; i++)
            array[i] = m_DepthSampleArray[i] * texelRadius;
        m_Material.SetInt("_SampleCount", _sampleCount);
        m_Material.SetVectorArray("_SampleSphere", array);
        m_Material.SetColor("_AOColor", aoColor);
        m_Material.SetFloat("_Strength", strength);
        m_Material.SetFloat("_FallOff", _fallOff);
        m_Material.SetTexture("_NoiseTex", _noiseTex);
        return this;
    }
}
#endregion
}