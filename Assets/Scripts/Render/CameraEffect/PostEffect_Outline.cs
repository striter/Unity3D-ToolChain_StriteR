using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostEffect_Outline : PostEffectBase<CameraEffect_Outline, CameraEffectParam_Outline>
    {
        public override bool m_IsOpaqueProcess => m_EffectData.m_OpaquePostProcessing;
    }

    public enum enum_OutlineType
    {
        MaskBlur,
        EdgeDetect,
    }
    public enum enum_Convolution
    {
        Prewitt = 1,
        Sobel = 2,
    }

    public enum enum_DetectType
    {
        Depth = 1,
        Normal = 2,
        Color = 3,
    }

    [System.Serializable]
    public struct CameraEffectParam_Outline
    {
        public bool m_OpaquePostProcessing;
        [ColorUsage(true, true)] public Color m_OutlineColor;
        [MTitle] public enum_OutlineType m_OutlineType;
        [MFoldout(nameof(m_OutlineType), enum_OutlineType.EdgeDetect)] [Range(.1f, 3f)] public float m_OutlineWidth;
        [MFoldout(nameof(m_OutlineType), enum_OutlineType.EdgeDetect)] public enum_Convolution m_Convolution;
        [MFoldout(nameof(m_OutlineType), enum_OutlineType.EdgeDetect)] public enum_DetectType m_DetectType;
        [MFoldout(nameof(m_OutlineType), enum_OutlineType.EdgeDetect)] [Range(0, 10f)] public float m_Strength;
        [MFoldout(nameof(m_OutlineType), enum_OutlineType.EdgeDetect)] [Range(0.01f, 5f)] public float m_Bias;

        [MFoldout(nameof(m_OutlineType), enum_OutlineType.MaskBlur)] [CullingMask] public int m_CullingMask;
        [MFoldout(nameof(m_OutlineType), enum_OutlineType.MaskBlur)] public ImageEffectParam_Blurs m_BlurData;
        public static readonly CameraEffectParam_Outline m_Default = new CameraEffectParam_Outline()
        {
            m_OpaquePostProcessing = false,
            m_OutlineType = enum_OutlineType.EdgeDetect,
            m_OutlineColor = Color.white,
            m_OutlineWidth = 1,
            m_Convolution = enum_Convolution.Prewitt,
            m_DetectType = enum_DetectType.Depth,
            m_Strength = 2f,
            m_Bias = .5f,
            m_CullingMask = int.MaxValue,
        };
    }

    public class CameraEffect_Outline : ImageEffectBase<CameraEffectParam_Outline>,ImageEffectPipeline<CameraEffectParam_Outline>
    {
        #region ShaderProperties
        static readonly int ID_EdgeColor = Shader.PropertyToID("_OutlineColor");
        static readonly int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        static readonly string[] KW_Convolution = new string[2] { "_CONVOLUTION_PREWITT", "_CONVOLUTION_SOBEL" };
        static readonly string[] KW_DetectType = new string[3] { "_DETECT_DEPTH", "_DETECT_NORMAL", "_DETECT_COLOR" };
        static readonly int ID_Strength = Shader.PropertyToID("_Strength");
        static readonly int ID_Bias = Shader.PropertyToID("_Bias");

        static readonly int ID_MaskRender = Shader.PropertyToID("_OUTLINE_MASK");
        static readonly RenderTargetIdentifier RT_ID_MaskRender = new RenderTargetIdentifier(ID_MaskRender);
        static readonly int ID_MaskRenderBlur = Shader.PropertyToID("_OUTLINE_MASK_BLUR");
        static readonly RenderTargetIdentifier RT_ID_MaskRenderBlur = new RenderTargetIdentifier(ID_MaskRenderBlur);
        #endregion
        RenderTextureDescriptor m_Descriptor;
        Material m_RenderMaterial;
        List<ShaderTagId> m_ShaderTagIDs = new List<ShaderTagId>();
        ImageEffect_Blurs m_Blur;
        public override void Create()
        {
            base.Create();
            m_Blur = new ImageEffect_Blurs();
            m_RenderMaterial = new Material(Shader.Find("Game/Unlit/Color")) { hideFlags = HideFlags.HideAndDontSave };
            m_RenderMaterial.SetColor("_Color", Color.white);
            m_ShaderTagIDs.FillWithDefaultTags();
        }
        public override void Destroy()
        {
            base.Destroy();
            GameObject.DestroyImmediate(m_RenderMaterial);
        }
        public override void OnValidate(CameraEffectParam_Outline _data)
        {
            base.OnValidate(_data);
            m_Blur.OnValidate(_data.m_BlurData);
            m_Material.SetColor(ID_EdgeColor, _data.m_OutlineColor);
            m_Material.SetFloat(ID_OutlineWidth, _data.m_OutlineWidth);
            m_Material.EnableKeywords(KW_Convolution, (int)_data.m_Convolution);
            m_Material.EnableKeywords(KW_DetectType, (int)_data.m_DetectType);
            m_Material.SetFloat(ID_Strength, _data.m_Strength);
            m_Material.SetFloat(ID_Bias, _data.m_Bias);
        }
        public void Configure(ScriptableRenderer _renderer, CommandBuffer _buffer, RenderTextureDescriptor _descriptor, ScriptableRenderPass _pass, CameraEffectParam_Outline _data)
        {
            if (_data.m_OutlineType != enum_OutlineType.MaskBlur)
                return;

            m_Descriptor = new RenderTextureDescriptor(_descriptor.width,_descriptor.height,RenderTextureFormat.R8,0,0);
            _buffer.GetTemporaryRT(ID_MaskRender, m_Descriptor, FilterMode.Bilinear);
            _buffer.GetTemporaryRT(ID_MaskRenderBlur, m_Descriptor, FilterMode.Bilinear);
            _buffer.SetRenderTarget(RT_ID_MaskRender);
            _buffer.ClearRenderTarget(false,true,Color.black);
        }

        public void FrameCleanUp(CommandBuffer _buffer, CameraEffectParam_Outline _data)
        {
            if (_data.m_OutlineType != enum_OutlineType.MaskBlur)
                return;

            _buffer.ReleaseTemporaryRT(ID_MaskRender);
            _buffer.ReleaseTemporaryRT(ID_MaskRenderBlur);
        }
        public void Execute(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData, CameraEffectParam_Outline _data)
        {
            if (_data.m_OutlineType != enum_OutlineType.MaskBlur)
                return;

            DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.overrideMaterial = m_RenderMaterial;
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = _data.m_CullingMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor, CameraEffectParam_Outline _data)
        {
            switch (_data.m_OutlineType)
            {
                case enum_OutlineType.EdgeDetect:
                    {
                        _buffer.Blit(_src, _dst, m_Material, 0);
                    }
                    break;
                case enum_OutlineType.MaskBlur:
                    {
                        _descriptor.colorFormat = RenderTextureFormat.R8;

                        m_Blur.ExecutePostProcessBuffer(_buffer, RT_ID_MaskRender, RT_ID_MaskRenderBlur, m_Descriptor, _data.m_BlurData);
                        _buffer.Blit(_src, _dst, m_Material, 1);
                    }
                    break;
            }
        }
    }
}