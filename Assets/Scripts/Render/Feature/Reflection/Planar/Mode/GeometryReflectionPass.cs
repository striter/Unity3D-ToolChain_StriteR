using Rendering.PostProcess;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    class FGeometryReflectionScreenSpace : APlanarReflectionBase
    {
        static readonly int kSampleCount = Shader.PropertyToID( "_SAMPLE_COUNT");
        static readonly int kResultTexelSize = Shader.PropertyToID("_Result_TexelSize");

        static readonly int kKernelInput = Shader.PropertyToID("_Input");
        static readonly int kKernelResult = Shader.PropertyToID("_Result");
        static readonly int kPlanePosition = Shader.PropertyToID("_PlanePosition");
        static readonly int kPlaneNormal = Shader.PropertyToID("_PlaneNormal");
        
        static readonly int kSpherePosition = Shader.PropertyToID("_SpherePosition");
        static readonly int kSphereRadius = Shader.PropertyToID("_SphereRadius");
        int m_Kernels;
        Int2 m_ThreadGroups;
        LocalKeyword[] kKeywords;
        
        private readonly PassiveInstance<ComputeShader> m_ReflectionComputeShader=new PassiveInstance<ComputeShader>(()=>RenderResources.FindComputeShader("PlanarReflection"));
        
        public FGeometryReflectionScreenSpace(PlanarReflectionData _data,  PlanarReflectionProvider _component,  int _index) : base(_data, _component, _index)
        {
        }
        protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref PlanarReflectionData _data)
        {
            base.ConfigureColorDescriptor(ref _descriptor, ref _data);
            _descriptor.enableRandomWrite = true;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            _descriptor.msaaSamples = 1;
            m_Kernels = ((ComputeShader)m_ReflectionComputeShader).FindKernel("Generate");
            kKeywords = m_ReflectionComputeShader.Value.GetLocalKeywords<EPlanarReflectionGeometry>();
            m_ThreadGroups = new Int2(_descriptor.width / 8, _descriptor.height / 8);
        }

        protected override void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor, RTHandle _colorTarget)
        {
            base.DoConfigure(_cmd, _descriptor, _colorTarget);
            ConfigureTarget(_colorTarget);
            ConfigureClear(ClearFlag.Color,Color.clear);
        }

        protected override void Execute(ref PlanarReflectionData _data, ScriptableRenderContext _context,
            ref RenderingData _renderingData, CommandBuffer _cmd, ref PlanarReflectionProvider _config,  ref RenderTextureDescriptor _descriptor, ref RTHandle _target)
        {
            _cmd.SetComputeIntParam(m_ReflectionComputeShader, kSampleCount, _data.m_Sample);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kResultTexelSize, _descriptor.GetTexelSize());
            
            _cmd.EnableLocalKeywords(m_ReflectionComputeShader.Value,kKeywords,_config.m_Geometry);
            if (_config.m_Geometry == EPlanarReflectionGeometry._SPHERE)
            {
                var sphere = _config.m_SphereData;
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kSpherePosition, sphere.center.to4());
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kSphereRadius,new Vector4(sphere.radius,sphere.radius*sphere.radius));
            }
            else if (_config.m_Geometry == EPlanarReflectionGeometry._PLANE)
            {
                var _plane = _config.m_PlaneData;
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kPlaneNormal, math.normalize(_plane.normal).to4());
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kPlanePosition, _plane.position.to4());
            }
            
            _cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, kKernelInput, _renderingData.cameraData.renderer.cameraColorTargetHandle);
            _cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, kKernelResult, _target);
            _cmd.DispatchCompute(m_ReflectionComputeShader, m_Kernels, m_ThreadGroups.x,m_ThreadGroups.y, 1);
        }

    }
}