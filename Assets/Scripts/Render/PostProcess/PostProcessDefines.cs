using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public enum EPostProcess
    {
        Opaque=0,
        AntiAliasing,
        Volumetric,
        DepthOfField,
        ColorUpgrade,
        ColorDegrade,
        Stylize,
    }

    public interface IPostProcessParameter
    {
        bool Validate();
    }
    
    public interface IPostProcessBehaviour
    {
        string m_Name { get; }
        bool m_OpaqueProcess { get; }
        bool m_Enabled { get; }
        EPostProcess Event { get; }
        void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData);
        void Configure( CommandBuffer _buffer, RenderTextureDescriptor _descriptor);
        void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _executeData);
        void FrameCleanUp(CommandBuffer _buffer);
        void ValidateParameters();
    }
}