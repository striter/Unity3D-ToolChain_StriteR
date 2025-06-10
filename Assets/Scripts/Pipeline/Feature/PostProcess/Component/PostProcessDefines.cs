using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public enum EPostProcess
    {
        Opaque=0,
        Volumetric,
        DepthOfField,
        AntiAliasing,
        ColorGrading,
        VideoHomeSystem,
        UVMapping,
        Stylize,
    }

    public interface IPostProcessParameter
    {
        bool Validate();
    }
    
    public interface IPostProcessBehaviour
    {
        bool OpaqueProcess { get; }
        EPostProcess Event { get; }
        void Configure( CommandBuffer _buffer, RenderTextureDescriptor _descriptor);
        void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _executeData, ScriptableRenderContext _context, ref RenderingData _renderingData);
        void FrameCleanUp(CommandBuffer _buffer);
        bool Validate(ref RenderingData _renderingData);
    }
}