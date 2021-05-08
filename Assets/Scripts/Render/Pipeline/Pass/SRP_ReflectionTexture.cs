using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using ImageEffect;
    public class SRP_CameraReflectionTexture : ScriptableRenderPass,ISRPBase
    {
        #region ID
        const int C_KernalGroupCount = 8;
        static readonly int ID_ReflectionTexture = Shader.PropertyToID("_CameraReflectionTexure");
        static readonly RenderTargetIdentifier RT_ID_ReflectionTexture = new RenderTargetIdentifier(ID_ReflectionTexture);
        static readonly int ID_ReflectionTempTexture = Shader.PropertyToID("_CameraReflectionTemp");
        static readonly RenderTargetIdentifier RT_ID_ReflectionTempTexture = new RenderTargetIdentifier(ID_ReflectionTempTexture);

        static readonly int ID_Result_TexelSize = Shader.PropertyToID("_Result_TexelSize");
        static readonly int ID_PlaneNormal = Shader.PropertyToID("_PlaneNormal");
        static readonly int ID_PlanePosition = Shader.PropertyToID("_PlanePosition");
        static readonly int ID_DitherAmount = Shader.PropertyToID("_DitherAmount");

        static readonly int ID_Input = Shader.PropertyToID("_Input");
        static readonly int ID_Result = Shader.PropertyToID("_Result");
        #endregion

        SRD_ReflectionPlane m_Plane;
        ComputeShader m_ComputeShader;
        RenderTargetIdentifier m_ColorTarget;
        RenderTargetIdentifier m_ReflectionResult;
        RenderTextureDescriptor m_ResultDescriptor;
        ImageEffect_Blurs m_Blur;
        int m_KernalGenerate;
        int m_GroupX;
        int m_GroupY;
        public SRP_CameraReflectionTexture()
        {
            m_Blur = new ImageEffect_Blurs();
        }
        public void Dispose()
        {
            m_Blur.Destroy();
        }
        public SRP_CameraReflectionTexture Setup(RenderTargetIdentifier _color, ComputeShader _shader, SRD_ReflectionPlane _plane)
        {
            m_ColorTarget = _color;
            m_Plane = _plane;
            m_ComputeShader = _shader;
            m_KernalGenerate = m_ComputeShader.FindKernel("Generate");
            m_Blur.DoValidate(_plane.m_BlurParam);
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            m_ResultDescriptor = new RenderTextureDescriptor(cameraTextureDescriptor.width, cameraTextureDescriptor.height, RenderTextureFormat.ARGB32, -1) { enableRandomWrite = true, msaaSamples=1 };
            cmd.GetTemporaryRT(ID_ReflectionTexture, m_ResultDescriptor,FilterMode.Bilinear);
            ConfigureTarget(ID_ReflectionTexture);
            m_ReflectionResult = RT_ID_ReflectionTexture;

            if (m_Plane.m_EnableBlur)
            {
                cmd.GetTemporaryRT(ID_ReflectionTempTexture, m_ResultDescriptor, FilterMode.Bilinear);
                ConfigureTarget(ID_ReflectionTexture);
                m_ReflectionResult = RT_ID_ReflectionTempTexture;
                ConfigureTarget(ID_ReflectionTempTexture);
            }

            m_GroupX = m_ResultDescriptor.width / C_KernalGroupCount;
            m_GroupY = m_ResultDescriptor.height / C_KernalGroupCount;
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(ID_ReflectionTexture);
            if (m_Plane.m_EnableBlur)
                cmd.ReleaseTemporaryRT(ID_ReflectionTempTexture);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Reflection Texture");
            DistancePlane planeData = m_Plane.m_PlaneData;
            cmd.SetComputeVectorParam(m_ComputeShader, ID_PlaneNormal, planeData.m_Normal.normalized);
            cmd.SetComputeVectorParam(m_ComputeShader, ID_PlanePosition, planeData.m_Distance*planeData.m_Normal);
            cmd.SetComputeVectorParam(m_ComputeShader, ID_Result_TexelSize, m_ResultDescriptor.GetTexelSize());
            cmd.SetComputeFloatParam(m_ComputeShader, ID_DitherAmount, m_Plane.m_DitherAmount);

            cmd.ClearRenderTarget(true, true, m_Plane.m_ClearColor.SetAlpha(0));
            cmd.SetComputeTextureParam(m_ComputeShader, m_KernalGenerate, ID_Input, m_ColorTarget);
            cmd.SetComputeTextureParam(m_ComputeShader, m_KernalGenerate, ID_Result, m_ReflectionResult);
            cmd.DispatchCompute(m_ComputeShader, m_KernalGenerate, m_GroupX, m_GroupY, 1);
            if(m_Plane.m_EnableBlur)
                m_Blur.ExecuteBuffer(cmd, m_ResultDescriptor, m_ReflectionResult, RT_ID_ReflectionTexture, m_Plane.m_BlurParam);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}