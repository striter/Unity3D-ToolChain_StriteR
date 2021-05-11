using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using ImageEffect;
    public class SRP_CameraReflectionTexture : ScriptableRenderPass,ISRPBase
    {
        #region ID
        static readonly int ID_ReflectionTexture = Shader.PropertyToID("_CameraReflectionTexure");
        static readonly RenderTargetIdentifier RT_ID_ReflectionTexture = new RenderTargetIdentifier(ID_ReflectionTexture);
        static readonly int ID_ReflectionDepthComparer = Shader.PropertyToID("_CameraReflectionDepthComaparer");
        static readonly RenderTargetIdentifier RT_ID_ReflectionDepthComparer = new RenderTargetIdentifier(ID_ReflectionDepthComparer);
        static readonly int ID_ReflectionTempTexture = Shader.PropertyToID("_CameraReflectionTemp");
        static readonly RenderTargetIdentifier RT_ID_ReflectionTempTexture = new RenderTargetIdentifier(ID_ReflectionTempTexture);

        static readonly int ID_SampleCount = Shader.PropertyToID( "_SAMPLE_COUNT");
        static readonly int ID_Result_TexelSize = Shader.PropertyToID("_Result_TexelSize");
        static readonly int ID_PlaneNormal = Shader.PropertyToID("_PlaneNormal");
        static readonly int ID_PlanePosition = Shader.PropertyToID("_PlanePosition");

        static readonly int ID_Input = Shader.PropertyToID("_Input");
        static readonly int ID_Depth = Shader.PropertyToID("_Depth");
        static readonly int ID_Result = Shader.PropertyToID("_Result");
        #endregion

        SRD_ReflectionPlane m_Plane;
        ComputeShader m_ComputeShader;
        RenderTargetIdentifier m_ColorTarget;
        RenderTargetIdentifier m_ReflectionResult;
        RenderTextureDescriptor m_ResultDescriptor;
        ImageEffect_Blurs m_Blur;
        Int3 m_Kernels;
        public SRP_CameraReflectionTexture()
        {
            m_Blur = new ImageEffect_Blurs();
        }
        public void Dispose()
        {
            m_Blur.Destroy();
        }
        public SRP_CameraReflectionTexture Setup(RenderTargetIdentifier _color, ComputeShader _shader, SRD_ReflectionPlane _plane,bool _lowEnd)
        {
            m_ColorTarget = _color;
            m_Plane = _plane;
            m_ComputeShader = _shader;
            string keyword = _lowEnd ? "Low" : "Medium";
            int groupCount = _lowEnd ? 1 : 8;
            m_Kernels = new Int3(m_ComputeShader.FindKernel("Clear" + keyword),m_ComputeShader.FindKernel("Generate"+ keyword), groupCount);
            m_Blur.DoValidate(_plane.m_BlurParam);
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            m_ResultDescriptor = new RenderTextureDescriptor(cameraTextureDescriptor.width, cameraTextureDescriptor.height, RenderTextureFormat.ARGB32, -1) { enableRandomWrite = true };

            cmd.GetTemporaryRT(ID_ReflectionTexture, m_ResultDescriptor,FilterMode.Bilinear);
            m_ReflectionResult = RT_ID_ReflectionTexture;

            if (m_Plane.m_EnableBlur)
            {
                cmd.GetTemporaryRT(ID_ReflectionTempTexture, m_ResultDescriptor, FilterMode.Bilinear);
                m_ReflectionResult = RT_ID_ReflectionTempTexture;
            }
            ConfigureTarget(m_ReflectionResult);
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
            cmd.GetTemporaryRT(ID_ReflectionDepthComparer, m_ResultDescriptor);

            cmd.SetComputeIntParam(m_ComputeShader, ID_SampleCount, m_Plane.m_Sample);
            cmd.SetComputeVectorParam(m_ComputeShader, ID_PlaneNormal, planeData.m_Normal.normalized);
            cmd.SetComputeVectorParam(m_ComputeShader, ID_PlanePosition, planeData.m_Distance*planeData.m_Normal);
            cmd.SetComputeVectorParam(m_ComputeShader, ID_Result_TexelSize, m_ResultDescriptor.GetTexelSize());

            int groupX = m_ResultDescriptor.width / m_Kernels.m_Z;
            int groupY=m_ResultDescriptor.height / m_Kernels.m_Z;

            cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_X, ID_Depth,RT_ID_ReflectionDepthComparer);
            cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_X, ID_Result,m_ReflectionResult);
            cmd.DispatchCompute(m_ComputeShader, m_Kernels.m_X, groupX,groupY,1);

            cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_Y, ID_Input, m_ColorTarget);
            cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_Y, ID_Depth, RT_ID_ReflectionDepthComparer);
            cmd.SetComputeTextureParam(m_ComputeShader, m_Kernels.m_Y, ID_Result, m_ReflectionResult);
            cmd.DispatchCompute(m_ComputeShader, m_Kernels.m_Y, groupX,groupY, 1);
            if (m_Plane.m_EnableBlur)
                m_Blur.ExecuteBuffer(cmd, m_ResultDescriptor, m_ReflectionResult, RT_ID_ReflectionTexture, m_Plane.m_BlurParam);

            cmd.ReleaseTemporaryRT(ID_ReflectionDepthComparer);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}