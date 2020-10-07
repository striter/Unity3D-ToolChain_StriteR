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
#endregion
}