using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    public static class UPipeline
    {
        static List<ShaderTagId>  kInternalDefaultSahderTags;
        public static List<ShaderTagId> kDefaultShaderTags
        {
            get
            {
                if (kInternalDefaultSahderTags == null)
                {
                    kInternalDefaultSahderTags= new List<ShaderTagId>()
                    {
                        new ShaderTagId("SRPDefaultUnlit"),
                        new ShaderTagId("UniversalForward"),
                        new ShaderTagId("UniversalForwardOnly"),
                        new ShaderTagId("LightweightForward"),
                    };
                }
                return kInternalDefaultSahderTags;
            }
        }
        

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
                for (int i = 0; i < kDefaultShaderTags.Count; i++)
                    settings.SetShaderPassName(i, kDefaultShaderTags[i]);
            }
            return settings;
        }
        public static void FillWithDefaultTags(this List<ShaderTagId> _tagList)
        {
            _tagList.Clear();
            _tagList.AddRange(kDefaultShaderTags);
        }

        public static T GetDefaultPostProcessData<T>() where T : struct => (T)typeof(T).GetField("kDefault", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null);
        public static bool IsEnabled(this CameraOverrideOption _override,bool _default)=>_override == CameraOverrideOption.On || (_override == CameraOverrideOption.UsePipelineSettings && _default);
        public static Vector4 GetTexelSize(this RenderTextureDescriptor _descriptor) => new Vector4(1f/_descriptor.width,1f/_descriptor.height,_descriptor.width,_descriptor.height);
    }
}

