using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Rendering.ImageEffect;
public class PE_ColorGrading_RenderFeature : ScriptableRendererFeature
{
    public ImageEffectParam_ColorGrading m_Data;
    CRenderPass pass;
    public bool m_ScenePreview;
    public override void Create()
    {
        pass = new CRenderPass();
        pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        pass.Validate(m_Data);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!m_ScenePreview && (renderingData.cameraData.isSceneViewCamera || renderingData.cameraData.isPreviewCamera))
            return;

        renderer.EnqueuePass(pass.Setup(renderer.cameraColorTarget));
    }
}

public class CRenderPass : ScriptableRenderPass
{
    #region ShaderProperties
    readonly int ID_Weight = Shader.PropertyToID("_Weight");

    const string KW_LUT = "_LUT";
    readonly int ID_LUT = Shader.PropertyToID("_LUTTex");
    readonly int ID_LUTCellCount = Shader.PropertyToID("_LUTCellCount");

    const string KW_BSC = "_BSC";
    readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
    readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
    readonly int ID_Contrast = Shader.PropertyToID("_Contrast");

    const string KW_MixChannel = "_CHANNEL_MIXER";
    readonly int ID_MixRed = Shader.PropertyToID("_MixRed");
    readonly int ID_MixGreen = Shader.PropertyToID("_MixGreen");
    readonly int ID_MixBlue = Shader.PropertyToID("_MixBlue");
    #endregion
    RenderTargetIdentifier identifier;
    Material material;
    RenderTargetIdentifier m_RT;
    public void Validate(ImageEffectParam_ColorGrading _data)
    {
        if(!material)
           
            material = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/ImageEffect_ColorGrading"));

        material.SetFloat(ID_Weight, _data.m_Weight);

        material.EnableKeyword(KW_LUT, _data.m_LUT);
        material.SetTexture(ID_LUT, _data.m_LUT);
        material.SetInt(ID_LUTCellCount, (int)_data.m_LUTCellCount);

        material.EnableKeyword(KW_BSC, _data.m_brightness != 1 || _data.m_saturation != 1f || _data.m_contrast != 1);
        material.SetFloat(ID_Brightness, _data.m_brightness);
        material.SetFloat(ID_Saturation, _data.m_saturation);
        material.SetFloat(ID_Contrast, _data.m_contrast);

        material.EnableKeyword(KW_MixChannel, _data.m_MixRed != Vector3.zero || _data.m_MixBlue != Vector3.zero || _data.m_MixGreen != Vector3.zero);
        material.SetVector(ID_MixRed, _data.m_MixRed);
        material.SetVector(ID_MixGreen, _data.m_MixGreen);
        material.SetVector(ID_MixBlue, _data.m_MixBlue);
    }
    public CRenderPass Setup(RenderTargetIdentifier _identifier)
    {
        this.identifier = _identifier;
        return this;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);

        cmd.GetTemporaryRT(Shader.PropertyToID("_Test") ,cameraTextureDescriptor.width,cameraTextureDescriptor.height,0,FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        m_RT = new RenderTargetIdentifier(Shader.PropertyToID("_Test"));
        ConfigureTarget(m_RT);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("Test");
        cmd.Blit(identifier, m_RT, material,0);
        cmd.Blit(m_RT, identifier);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}