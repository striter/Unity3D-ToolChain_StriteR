using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering
{
    public static class UPipeline
    {
        public static DrawingSettings CreateDrawingSettings(bool fillDefault, Camera _camera)
        {
            DrawingSettings settings = new DrawingSettings();
            settings.sortingSettings = new SortingSettings(_camera);
            settings.perObjectData = PerObjectData.None;
            settings.enableDynamicBatching = true;
            settings.enableInstancing = true;
            if (fillDefault)
            {
                settings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                settings.SetShaderPassName(1, new ShaderTagId("UniversalForward"));
                settings.SetShaderPassName(2, new ShaderTagId("UniversalForwardOnly"));
                settings.SetShaderPassName(3, new ShaderTagId("LightweightForward"));
            }
            return settings;
        }
        public static void FillWithDefaultTags(this List<ShaderTagId> _tagList)
        {
            _tagList.Clear();
            _tagList.Add(new ShaderTagId("SRPDefaultUnlit"));
            _tagList.Add(new ShaderTagId("UniversalForward"));
            _tagList.Add(new ShaderTagId("UniversalForwardOnly"));
            _tagList.Add(new ShaderTagId("LightweightForward"));
        }

        public static Material CreateMaterial(Type _type)
        {
            string lastname = _type.Name.Split('_')[1];
            string name ="Hidden/PostProcess/"+lastname;
            Shader _shader = Shader.Find(name);

            if (_shader == null)
                throw new NullReferenceException("Invalid ImageEffect Shader Found:" + name);

            if (!_shader.isSupported)
                throw new NullReferenceException("Shader Not Supported:" + _type.Name);

            return new Material(_shader) { hideFlags = HideFlags.HideAndDontSave };
        }
        public static T GetDefaultPostProcessData<T>() where T : struct => (T)typeof(T).GetField("m_Default", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null);
        public static bool IsEnabled(this CameraOverrideOption _override,bool _default)=>_override == CameraOverrideOption.On || (_override == CameraOverrideOption.UsePipelineSettings && _default);
        public static Vector4 GetTexelSize(this RenderTextureDescriptor _descriptor) => new Vector4(1f/_descriptor.width,1f/_descriptor.height,_descriptor.width,_descriptor.height);
    }
}

