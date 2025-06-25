using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class MotionVectorTexturePass : ScriptableRenderPass
    {
        private readonly PassiveInstance<Material> m_CameraMaterial =  new(() => new Material(RenderResources.FindInclude("Hidden/MotionVectorCamera")) { hideFlags = HideFlags.HideAndDontSave },GameObject.DestroyImmediate);
        private readonly PassiveInstance<Material> m_ObjectMaterial = new(() => new Material(RenderResources.FindInclude("Hidden/MotionVectorObject")) { hideFlags = HideFlags.HideAndDontSave },GameObject.DestroyImmediate);
        private static readonly int kMatrix_VP_Pre = Shader.PropertyToID("_Matrix_VP_Pre");
        private static Dictionary<int, Matrix4x4> m_PreViewProjectionMatrixes = new();

        public override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
        {
            _cmd.GetTemporaryRT(KRenderTextures.kCameraMotionVector, _cameraTextureDescriptor.width, _cameraTextureDescriptor.height,16, FilterMode.Point, RenderTextureFormat.RGFloat);
            ConfigureTarget(RTHandles.Alloc(KRenderTextures.kCameraMotionVectorRT));
            ConfigureClear(ClearFlag.All,Color.black);
        }
        
        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(KRenderTextures.kCameraMotionVector);
        }
        
        static readonly string kSampling = nameof(MotionVectorTexturePass); 
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var cameraID = _renderingData.cameraData.camera.GetInstanceID();
            
            _renderingData.cameraData.camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;    //Dude wtf

            var projection = GL.GetGPUProjectionMatrix(_renderingData.cameraData.camera.projectionMatrix,_renderingData.cameraData.IsCameraProjectionMatrixFlipped());
            var view = _renderingData.cameraData.camera.worldToCameraMatrix;
            var vp = projection * view;

            if (!m_PreViewProjectionMatrixes.ContainsKey(cameraID))
                m_PreViewProjectionMatrixes.Add(cameraID,vp);

            var cmd = CommandBufferPool.Get(kSampling);
            cmd.BeginSample(kSampling);
            cmd.SetGlobalMatrix(kMatrix_VP_Pre,m_PreViewProjectionMatrixes[cameraID]);
            m_PreViewProjectionMatrixes[cameraID] = vp;
            cmd.DrawProcedural(Matrix4x4.identity,m_CameraMaterial,0,MeshTopology.Triangles,3,1);
            _context.ExecuteCommandBuffer(cmd);

            var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.overrideMaterial = m_ObjectMaterial;
            drawingSettings.perObjectData = PerObjectData.MotionVectors;
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque) { layerMask = int.MaxValue};
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
            
            cmd.Clear();
            cmd.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle);
            cmd.EndSample(kSampling);
            _context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            m_CameraMaterial.Dispose();
            m_PreViewProjectionMatrixes.Clear();
        }
    }

}