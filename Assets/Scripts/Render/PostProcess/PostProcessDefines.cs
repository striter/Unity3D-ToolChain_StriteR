using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public enum EPostProcess
    {
        Opaque=0,
        Volumetric=1,
        DepthOfField=2,
        ColorUpgrade=3,
        ColorDegrade=4,
        Stylize=5,
        Default=6,
    }

    
    public interface IPostProcessPipelineCallback<T> where T:struct
    {
        void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor,ref  T _data);
        void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData,ref T _data);
        void FrameCleanUp(CommandBuffer _buffer,ref T _data);
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
        void Configure( CommandBuffer _buffer, RenderTextureDescriptor _descriptor);
        void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData);
        void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _executeData);
        void FrameCleanUp(CommandBuffer _buffer);
        void ValidateParameters();
    }
}