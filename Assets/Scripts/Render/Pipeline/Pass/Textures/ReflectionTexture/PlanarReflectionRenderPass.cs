using Geometry;
using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    class FGeometryReflectionMirrorSpace : APlanarReflectionBase
    {
        private const string kReflectionDepth = "_CameraReflectionDepthComparer";
        static readonly int kCameraWorldPosition = Shader.PropertyToID("_WorldSpaceCameraPos");

         int m_ReflectionDepth;
         RenderTargetIdentifier m_ReflectionDepthID;

         public FGeometryReflectionMirrorSpace(PlanarReflectionData _data, FBlursCore _blur, PlanarReflection _component, ScriptableRenderer _renderer, int _index) : base(_data, _blur, _component, _renderer, _index)
         {
             m_ReflectionDepth = Shader.PropertyToID(kReflectionDepth + _index);
             m_ReflectionDepthID = new RenderTargetIdentifier(m_ReflectionDepth);
         }
         protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref PlanarReflectionData _data)
         {
             base.ConfigureColorDescriptor(ref _descriptor, ref _data);
             _descriptor.colorFormat = RenderTextureFormat.ARGB32;
         }

         protected override void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor, RTHandle _colorTarget)
         {
             base.DoConfigure(_cmd, _descriptor, _colorTarget);
             var depthDescriptor = _descriptor;
             depthDescriptor.colorFormat = RenderTextureFormat.Depth;
             depthDescriptor.depthBufferBits = 32;
             depthDescriptor.enableRandomWrite = false;
             _cmd.GetTemporaryRT(m_ReflectionDepth, depthDescriptor, FilterMode.Point);
             ConfigureTarget(_colorTarget,RTHandles.Alloc(m_ReflectionDepthID));
             ConfigureClear(ClearFlag.All,Color.clear);
         }


         public override void OnCameraCleanup(CommandBuffer _cmd)
         {
             base.OnCameraCleanup(_cmd);
             _cmd.ReleaseTemporaryRT(m_ReflectionDepth);
         }

         protected override void Execute(ref PlanarReflectionData _data, ScriptableRenderContext _context, ref RenderingData _renderingData,
             CommandBuffer _cmd, ref PlanarReflection _config,  ref RenderTextureDescriptor _descriptor, ref RTHandle _target,
             ref ScriptableRenderer _renderer)
         {
             ref var cameraData = ref _renderingData.cameraData;
             ref Camera camera = ref cameraData.camera;
            
            Matrix4x4 planeMirrorMatrix = _config.m_PlaneData.GetMirrorMatrix();
            Matrix4x4 cullingMatrix = camera.cullingMatrix;
            camera.cullingMatrix = cullingMatrix * planeMirrorMatrix;

            DrawingSettings drawingSettings = CreateDrawingSettings(UPipeline.kDefaultShaderTags, ref _renderingData,  SortingCriteria.CommonOpaque);
            FilteringSettings filterSettings = new FilteringSettings(_data.m_IncludeTransparent? RenderQueueRange.all : RenderQueueRange.opaque);
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), cameraData.IsCameraProjectionMatrixFlipped());
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
            viewMatrix*= planeMirrorMatrix;
            
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, viewMatrix , projectionMatrix, false);
            var cameraPosition = camera.transform.position;
            _cmd.SetGlobalVector( kCameraWorldPosition,planeMirrorMatrix.MultiplyPoint(cameraPosition));
            _cmd.SetInvertCulling(true);
            _context.ExecuteCommandBuffer(_cmd);

            if (_data.m_Recull)
            {
                if (cameraData.camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
                {
                    cullingParameters.maximumVisibleLights = _data.m_AdditionalLightcount;
                    _context.DrawRenderers(_context.Cull(ref cullingParameters), ref drawingSettings, ref filterSettings);
                }
            }
            else
            {
                _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
            }
            
            _cmd.Clear();
            _cmd.SetInvertCulling(false);
            _cmd.SetGlobalVector( kCameraWorldPosition,cameraPosition);
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
            camera.ResetCullingMatrix();
         }

    }
}