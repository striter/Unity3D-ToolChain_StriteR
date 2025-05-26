using System;
using System.Collections.Generic;
using System.Reflection;
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
         public static bool EnableKeyword(this Material _material, string _keyword, bool _enable)
    {
        if (_material == null)
        {
            Debug.LogWarning("Mull Material Found.");
            return false;
        }
        
        if (_enable)
            _material.EnableKeyword(_keyword);
        else
            _material.DisableKeyword(_keyword);
        return _enable;
    }

    public static void EnableKeywords(this Material _material, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _material.EnableKeyword(_keywords[i], i + 1 == _target);
    }

    public static bool EnableKeywords<T>(this Material _material, T _target) where T:Enum
    {
        var index = UEnum.GetIndex(_target);
        var keywords = UEnum.GetEnums<T>();
        for (var i = 0; i < keywords.Length; i++)
            _material.EnableKeyword(keywords[i].ToString(), i == index);

        return index != 0;
    }
    //Compute Shader
    public static void EnableKeyword(this ComputeShader _computeShader, string _keyword, bool _enable)
    {
        if (_enable)
            _computeShader.EnableKeyword(_keyword);
        else
            _computeShader.DisableKeyword(_keyword);
    }
    public static void EnableKeywords<T>(this ComputeShader _computeShader, string[] _keywords, T _target) where T : Enum => EnableKeywords(_computeShader, _keywords, Convert.ToInt32(_target));
    public static void EnableKeywords(this ComputeShader _computeShader, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _computeShader.EnableKeyword(_keywords[i], i + 1 == _target);
    }
    //Global
    public static bool EnableGlobalKeywords<T>(T _target) where T : Enum
    {
        int index = UEnum.GetIndex(_target);
        var keywords = UEnum.GetEnums<T>();
        for (int i = 0; i < keywords.Length; i++)
            EnableGlobalKeyword(keywords[i].ToString(), i==index);
        return index != 0;
    }

    public static bool EnableGlobalKeyword(string _keyword, bool _enable)
    {
        if (_enable)
            Shader.EnableKeyword(_keyword);
        else
            Shader.DisableKeyword(_keyword);
        return _enable;
    }

    public static void EnableKeyword(this CommandBuffer _buffer,string _keyword, bool _enable)
    {
        if (_enable)
            _buffer.EnableShaderKeyword(_keyword);
        else
            _buffer.DisableShaderKeyword(_keyword);
    }


    public static LocalKeyword[] GetLocalKeywords<T>(this ComputeShader _compute) where T:Enum
    {
        var keywords = UEnum.GetEnums<T>();
        LocalKeyword[] localKeywords = new LocalKeyword[keywords.Length];
        for (int i = 0; i < keywords.Length; i++)
            localKeywords[i] = new LocalKeyword(_compute,keywords[i].ToString());
        return localKeywords;
    }

    public static void EnableLocalKeywords<T>(this CommandBuffer _buffer,ComputeShader _shader,LocalKeyword[] _keywords,T _value) where T:Enum
    {
        int index = UEnum.GetIndex(_value);
        var keywords = UEnum.GetEnums<T>();
        LocalKeyword[] localKeywords = new LocalKeyword[keywords.Length];
        for (int i = 0; i < keywords.Length; i++)
        {
            if(i==index)
                _buffer.EnableKeyword(_shader,_keywords[i]);
            else
                _buffer.DisableKeyword(_shader,_keywords[i]);
        }
    }
    
    public static void CopyCameraProperties(Camera workingCamera, Camera refCamera)
    {
        workingCamera.cullingMask = refCamera.cullingMask;
        workingCamera.nearClipPlane = refCamera.nearClipPlane;
        workingCamera.farClipPlane = refCamera.farClipPlane;
        workingCamera.depthTextureMode = refCamera.depthTextureMode;
        workingCamera.clearFlags = refCamera.clearFlags;
        workingCamera.allowHDR = refCamera.allowHDR;
        workingCamera.allowMSAA = refCamera.allowMSAA;
        workingCamera.allowDynamicResolution = refCamera.allowDynamicResolution;
        // CopyPublicMembers(workingCamera,refCamera);     //Something bad will happen if you copy camera properties?
        var workingData = workingCamera.GetComponent<UniversalAdditionalCameraData>();
        var refData = refCamera.GetComponent<UniversalAdditionalCameraData>();
        UReflection.CopyPublicMembers(workingData, refData);
        workingData.SetRenderer((int)typeof(UniversalAdditionalCameraData).GetField("m_RendererIndex",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(refData));
    }

    public static void CopyCameraProjection(Camera workingCamera, Camera refCamera)
    {
        workingCamera.fieldOfView = refCamera.fieldOfView;
        var refTransform = refCamera.transform;
        workingCamera.transform.SetPositionAndRotation( refTransform.position, refTransform.rotation);
    }

    public static void CalculateOrthographicPositions(this Camera camera, out Vector3 tl, out Vector3 tr,out Vector3 bl, out Vector3 br)
    {
        var aspect = camera.aspect;
        var halfHeight = camera.orthographicSize;
        var cameraTrans = camera.transform;
        var toRight = cameraTrans.right * halfHeight * aspect;
        var toTop = cameraTrans.up * halfHeight;
        var startPos = cameraTrans.position+cameraTrans.forward*camera.nearClipPlane;
        tl = startPos - toRight + toTop;
        tr = startPos + toRight + toTop;
        bl = startPos - toRight - toTop;
        br = startPos + toRight - toTop;
    }
    
    

    }
}

