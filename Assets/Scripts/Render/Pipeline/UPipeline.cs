using System.Collections.Generic;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    public static class UPipeline
    {
        public static PassiveInstance<List<ShaderTagId>> kDefaultShaderTags => new PassiveInstance<List<ShaderTagId>>(() =>
            new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
            });

        public static readonly PassiveInstance<ShaderTagId> kLightVolumeTag =new PassiveInstance<ShaderTagId>(()=>new ShaderTagId("LightVolume"));
        

        public static DrawingSettings CreateDrawingSettings(bool _fillDefault, Camera _camera)
        {
            DrawingSettings settings = new DrawingSettings
            {
                sortingSettings = new SortingSettings(_camera),
                enableDynamicBatching = true,
                enableInstancing = true
            };
            if (_fillDefault)
            {
                for (int i = 0; i < kDefaultShaderTags.Value.Count; i++)
                    settings.SetShaderPassName(i, kDefaultShaderTags.Value[i]);
            }
            return settings;
        }

        public static bool IsEnabled(this CameraOverrideOption _override,bool _default)=>_override == CameraOverrideOption.On || (_override == CameraOverrideOption.UsePipelineSettings && _default);
        public static Vector4 GetTexelSize(this RenderTextureDescriptor _descriptor) => new Vector4(1f/_descriptor.width,1f/_descriptor.height,_descriptor.width,_descriptor.height);


        public static void ClearRenderTextureWithComputeShader(RenderTexture _texture,Color _clearColor = default)
        {
            var compute = RenderResources.FindComputeShader("Clear");

            var kernel = compute.FindKernel("Clear");
            compute.SetTexture(kernel,"_MainTex",_texture);
            compute.SetVector("_MainTex_ST",_texture.GetTexelSizeParameters());
            compute.SetVector("_ClearColor",_clearColor.to4().sqrmagnitude() >0 ? _clearColor : Color.black);
            compute.Dispatch(kernel,_texture.width/8,_texture.height/8,1);
        }
    }
}

