using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.ImageEffect
{
    public enum enum_Stylize
    {
        Pixel=0,
        OilPaint=1,
        ObraDithering=2,
    }
    public class PostEffect_Stylize : PostEffectBase<ImageEffect_Stylize, ImageEffectParam_Stylize> { }
    [Serializable]
    public struct ImageEffectParam_Stylize
    {
        [MTitle]public enum_Stylize m_Stylize;
        [MFoldout(nameof(m_Stylize),enum_Stylize.Pixel)] [ RangeInt(2,20)] public int m_DownSample;
        [MFoldout(nameof(m_Stylize), enum_Stylize.Pixel)] public bool m_PixelGrid;
        [MFoldout(nameof(m_Stylize), enum_Stylize.Pixel, nameof(m_PixelGrid), true)] [Range(0.01f, 0.49f)] public float m_GridWidth;
        [MFoldout(nameof(m_Stylize), enum_Stylize.Pixel, nameof(m_PixelGrid), true)] public Color m_PixelGridColor;
        [MFoldout(nameof(m_Stylize), enum_Stylize.OilPaint)] [RangeInt(1,20)]public int m_OilPaintKernel;
        [MFoldout(nameof(m_Stylize), enum_Stylize.OilPaint)] [Range(0.1f, 5f)] public float m_OilPaintSize;
        [MFoldout(nameof(m_Stylize), enum_Stylize.ObraDithering)] [Range(0.001f,1f)]public float m_ObraDitherScale;
        [MFoldout(nameof(m_Stylize), enum_Stylize.ObraDithering)] [Range(0.1f,1f)]public float m_ObraDitherStrength;
        [MFoldout(nameof(m_Stylize), enum_Stylize.ObraDithering)] public Color m_ObraDitherColor;
        public static readonly ImageEffectParam_Stylize m_Default = new ImageEffectParam_Stylize()
        {
            m_Stylize = enum_Stylize.Pixel,
            m_DownSample = 7,
            m_PixelGrid = false,
            m_GridWidth = .1f,
            m_PixelGridColor = Color.white.SetAlpha(.5f),
            m_OilPaintKernel = 10,
            m_OilPaintSize = 2f,
            m_ObraDitherColor = Color.yellow*.3f,
            m_ObraDitherScale=.33f,
            m_ObraDitherStrength=.5f,
        };
    }

    public class ImageEffect_Stylize:ImageEffectBase<ImageEffectParam_Stylize>
    {
        #region ShaderProperties
        static readonly int ID_PixelizeDownSample = Shader.PropertyToID("_STYLIZE_PIXEL_DOWNSAMPLE");
        static readonly RenderTargetIdentifier RT_PixelizeDownSample = new RenderTargetIdentifier(ID_PixelizeDownSample);
        const string KW_PixelGrid = "_PIXEL_GRID";
        static readonly int ID_PixelGridColor = Shader.PropertyToID("_PixelGridColor");
        static readonly int ID_PixelGridWidth = Shader.PropertyToID("_PixelGridWidth");

        static readonly int ID_OilPaintKernel = Shader.PropertyToID("_OilPaintKernel");
        static readonly int ID_OilPaintSize = Shader.PropertyToID("_OilPaintSize");

        static readonly int ID_ObraDitherScale = Shader.PropertyToID("_ObraDitherScale");
        static readonly int ID_ObraDitherStrength = Shader.PropertyToID("_ObraDitherStrength");
        static readonly int ID_ObraDitherColor = Shader.PropertyToID("_ObraDitherColor");
        #endregion
        protected override void OnValidate(ImageEffectParam_Stylize _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.EnableKeyword(KW_PixelGrid, _params.m_PixelGrid);
            _material.SetColor(ID_PixelGridColor,_params.m_PixelGridColor);
            _material.SetVector(ID_PixelGridWidth, new Vector2(_params.m_GridWidth, 1f - _params.m_GridWidth));
            _material.SetVector(ID_OilPaintKernel, new Vector2(-_params.m_OilPaintKernel / 2, _params.m_OilPaintKernel / 2 + 1));
            _material.SetFloat(ID_OilPaintSize, _params.m_OilPaintSize);
            _material.SetFloat(ID_ObraDitherScale, _params.m_ObraDitherScale);
            _material.SetFloat(ID_ObraDitherStrength, _params.m_ObraDitherStrength);
            _material.SetColor(ID_ObraDitherColor, _params.m_ObraDitherColor);
        }
        protected override void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, ImageEffectParam_Stylize _param)
        {
            switch (_param.m_Stylize)
            {
                default:
                    base.OnExecuteBuffer(_buffer, _descriptor, _src, _dst, _material, _param);
                    break;
                case enum_Stylize.Pixel:
                    _buffer.GetTemporaryRT(ID_PixelizeDownSample, _descriptor.width / _param.m_DownSample, _descriptor.height / _param.m_DownSample, 0, FilterMode.Point, _descriptor.colorFormat);
                    _buffer.Blit(_src, RT_PixelizeDownSample);
                    _buffer.Blit(RT_PixelizeDownSample, _dst, _material, 0);
                    _buffer.ReleaseTemporaryRT(ID_PixelizeDownSample);
                    break;
                case enum_Stylize.OilPaint:
                    _buffer.Blit(_src, _dst, _material, 1);
                    break;
                case enum_Stylize.ObraDithering:
                    _buffer.Blit(_src, _dst,_material, 2);
                    break;
            }
        }
    }

}
