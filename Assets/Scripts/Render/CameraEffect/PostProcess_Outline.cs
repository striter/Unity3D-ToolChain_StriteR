using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostProcess_Outline : PostProcessComponentBase<PPCore_Outline, PPData_Outline>
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

    [Serializable]
    public struct PPData_Outline
    {
        public bool m_OpaquePostProcessing;
        [ColorUsage(true, true)] public Color m_Color;
        [MTitle] public enum_OutlineType m_Type;
        [Header("Options")]
        [MFoldout(nameof(m_Type), enum_OutlineType.EdgeDetect)] [Range(.1f, 3f)] public float m_Width;
        [MFoldout(nameof(m_Type), enum_OutlineType.EdgeDetect)] public enum_Convolution m_Convolution;
        [MFoldout(nameof(m_Type), enum_OutlineType.EdgeDetect)] public enum_DetectType m_DetectType;
        [MFoldout(nameof(m_Type), enum_OutlineType.EdgeDetect)] [Range(0, 10f)] public float m_Strength;
        [MFoldout(nameof(m_Type), enum_OutlineType.EdgeDetect)] [Range(0.01f, 5f)] public float m_Bias;

        [MFoldout(nameof(m_Type), enum_OutlineType.MaskBlur)] [CullingMask] public int m_CullingMask;
        [MFoldout(nameof(m_Type), enum_OutlineType.MaskBlur)] public bool m_ZClip;
        [MFoldout(nameof(m_Type), enum_OutlineType.MaskBlur, nameof(m_ZClip), true)] public bool m_ZLesser;
        [MFoldout(nameof(m_Type), enum_OutlineType.MaskBlur,nameof(m_ZClip),true)] [Range(0.01f,1f)] public float m_ZOffset;
        [MFoldout(nameof(m_Type), enum_OutlineType.MaskBlur)] public PPData_Blurs m_OutlineBlur;
        public static readonly PPData_Outline m_Default = new PPData_Outline()
        {
            m_OpaquePostProcessing = false,
            m_Type = enum_OutlineType.EdgeDetect,
            m_Color = Color.white,
            m_Width = 1,
            m_Convolution = enum_Convolution.Prewitt,
            m_DetectType = enum_DetectType.Depth,
            m_Strength = 2f,
            m_Bias = .5f,
            m_CullingMask = int.MaxValue,
            m_ZClip=true,
            m_ZOffset=.2f,
            m_ZLesser=true,
        };
    }

    public class PPCore_Outline : PostProcessCore<PPData_Outline>,ImageEffectPipeline<PPData_Outline>
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
        static readonly int ID_MaskDepth = Shader.PropertyToID("_OUTLINE_MASK_DEPTH");
        static readonly RenderTargetIdentifier RT_ID_MaskDepth = new RenderTargetIdentifier(ID_MaskDepth);
        static readonly int ID_MaskRenderBlur = Shader.PropertyToID("_OUTLINE_MASK_BLUR");
        static readonly RenderTargetIdentifier RT_ID_MaskRenderBlur = new RenderTargetIdentifier(ID_MaskRenderBlur);

        const string KW_DepthForward = "_CSFORWARD";
        static readonly int ID_ZTest = Shader.PropertyToID("_ZTest");
        static readonly int ID_DepthForwardAmount = Shader.PropertyToID("_ClipSpaceForwardAmount");
        #endregion
        RenderTextureDescriptor m_Descriptor;
        Material m_RenderMaterial;
        Material m_RenderDepthMaterial;
        List<ShaderTagId> m_ShaderTagIDs = new List<ShaderTagId>();
        PPCore_Blurs _mCoreBlur;
        public override void Create()
        {
            base.Create();

            _mCoreBlur = new PPCore_Blurs();
            m_RenderMaterial = new Material(Shader.Find("Game/Unlit/Color")) { hideFlags = HideFlags.HideAndDontSave };
            m_RenderDepthMaterial = new Material(Shader.Find("Hidden/CopyDepth")) { hideFlags = HideFlags.HideAndDontSave };
            m_RenderMaterial.SetColor("_Color", Color.white);
            m_ShaderTagIDs.FillWithDefaultTags();
        }
        public override void Destroy()
        {
            base.Destroy();
            UnityEngine.Object.DestroyImmediate(m_RenderMaterial);
        }
        public override void OnValidate(PPData_Outline _data)
        {
            base.OnValidate(_data);
            _mCoreBlur.OnValidate(_data.m_OutlineBlur);
            m_Material.SetColor(ID_EdgeColor, _data.m_Color);
            m_Material.SetFloat(ID_OutlineWidth, _data.m_Width);
            m_Material.EnableKeywords(KW_Convolution, (int)_data.m_Convolution);
            m_Material.EnableKeywords(KW_DetectType, (int)_data.m_DetectType);
            m_Material.SetFloat(ID_Strength, _data.m_Strength);
            m_Material.SetFloat(ID_Bias, _data.m_Bias);

            m_RenderMaterial.EnableKeyword(KW_DepthForward, _data.m_ZClip);
            if (_data.m_ZClip)
            {
                m_RenderMaterial.SetInt(ID_ZTest, (int)(_data.m_ZLesser ?CompareFunction.Less:CompareFunction.Greater));
                m_RenderMaterial.SetFloat(ID_DepthForwardAmount, _data.m_ZOffset);
            }
        }

        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, PPData_Outline _data)
        {
            if (_data.m_Type != enum_OutlineType.MaskBlur)
                return;
            m_Descriptor = new RenderTextureDescriptor(_descriptor.width, _descriptor.height, RenderTextureFormat.R8, 0, 0);

            _buffer.GetTemporaryRT(ID_MaskRender, m_Descriptor, FilterMode.Bilinear);
            _buffer.GetTemporaryRT(ID_MaskRenderBlur, m_Descriptor, FilterMode.Bilinear);

            if (!_data.m_ZClip)
                return;
            var depthDescriptor = new RenderTextureDescriptor(_descriptor.width, _descriptor.height, RenderTextureFormat.Depth, 32, 0);
            _buffer.GetTemporaryRT(ID_MaskDepth, depthDescriptor);
            _buffer.Blit(RenderTargetHandle.CameraTarget.id, RT_ID_MaskDepth, m_RenderDepthMaterial);
        }

        public void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData, PPData_Outline _data)
        {
            if (_data.m_Type != enum_OutlineType.MaskBlur)
                return;
            CommandBuffer buffer = CommandBufferPool.Get("Outline Execute");
            if (!_data.m_ZClip)
                buffer.SetRenderTarget(RT_ID_MaskRender);
            else
                buffer.SetRenderTarget(RT_ID_MaskRender, RT_ID_MaskDepth);
            buffer.ClearRenderTarget(false, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);

            DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.overrideMaterial = m_RenderMaterial;
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = _data.m_CullingMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            buffer.Clear();
            buffer.SetRenderTarget(_renderer.cameraColorTarget);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor, PPData_Outline _data)
        {
            switch (_data.m_Type)
            {
                case enum_OutlineType.EdgeDetect:
                    {
                        _buffer.Blit(_src, _dst, m_Material, 0);
                    }
                    break;
                case enum_OutlineType.MaskBlur:
                    {
                        _mCoreBlur.ExecutePostProcessBuffer(_buffer, RT_ID_MaskRender, RT_ID_MaskRenderBlur, m_Descriptor, _data.m_OutlineBlur);
                        _buffer.Blit(_src, _dst, m_Material, 1);
                    }
                    break;
            }
        }
        public void FrameCleanUp(CommandBuffer _buffer, PPData_Outline _data)
        {
            if (_data.m_Type != enum_OutlineType.MaskBlur)
                return;
            _buffer.ReleaseTemporaryRT(ID_MaskRender);
            _buffer.ReleaseTemporaryRT(ID_MaskRenderBlur);
            if (!_data.m_ZClip)
                return;
            _buffer.ReleaseTemporaryRT(ID_MaskDepth);
        }
    }
}