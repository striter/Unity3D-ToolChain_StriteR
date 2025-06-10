using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public enum EStylize
    {
        None=-1,
        Pixel=0,
        OilPaint=1,
        BilateralFilter=2,
        ObraDithering = 3,
    }
    public enum EPixelBound
    {
        None=0,
        _PIXEL_GRID,
        _PIXEL_CIRCLE,
    }

    public class PostProcess_Stylize : APostProcessBehaviour<FStylizeCore, DStylize>
    {
        public override bool OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.Stylize;
    }
    
    [Serializable]
    public struct DStylize:IPostProcessParameter
    {
        [Title]public EStylize m_Stylize;
        [Foldout(nameof(m_Stylize),EStylize.Pixel)] [ Range(2,20)] public int downSample;
        [Foldout(nameof(m_Stylize), EStylize.Pixel)] public EPixelBound pixelGrid;
        [Foldout(nameof(m_Stylize), EStylize.Pixel)] [Fold(nameof(pixelGrid), EPixelBound.None)] [Range(0.01f, 0.49f)] public float gridWidth;
        [Foldout(nameof(m_Stylize), EStylize.Pixel)] [Fold(nameof(pixelGrid), EPixelBound.None)] public Color pixelGridColor;
        [Foldout(nameof(m_Stylize), EStylize.OilPaint)] [Range(1,20)]public int oilPaintKernel;
        [Foldout(nameof(m_Stylize), EStylize.OilPaint)] [Range(0.1f, 5f)] public float oilPaintSize;
        [Foldout(nameof(m_Stylize), EStylize.ObraDithering)] [Range(0.001f,1f)]public float obraDitherScale;
        [Foldout(nameof(m_Stylize), EStylize.ObraDithering)] [Range(0.001f,1f)]public float obraDitherStrength;
        [Foldout(nameof(m_Stylize), EStylize.ObraDithering)] public Color obraDitherColor;
        [Foldout(nameof(m_Stylize), EStylize.ObraDithering)] [Range(0,1f)] public float obraDitherStep;
        [Foldout(nameof(m_Stylize), EStylize.BilateralFilter)] [Range(0.1f, 5f)] public float bilaterailSize;
        [Foldout(nameof(m_Stylize), EStylize.BilateralFilter)] [Range(0.01f, 1f)] public float bilateralFactor;
        public bool Validate() => m_Stylize != EStylize.None;
        public static readonly DStylize kDefault = new DStylize()
        {
            m_Stylize = EStylize.Pixel,
            downSample = 7,
            pixelGrid = EPixelBound.None,
            gridWidth = .1f,
            pixelGridColor = Color.white.SetA(.5f),
            oilPaintKernel = 10,
            oilPaintSize = 2f,
            obraDitherColor = Color.yellow * .3f,
            obraDitherScale = .33f,
            obraDitherStrength = .5f,
            bilaterailSize = 5f,
            bilateralFactor=.5f,
            obraDitherStep = 0f,
        };

    }

    public class FStylizeCore:PostProcessCore<DStylize>
    {
        #region ShaderProperties
        static readonly int ID_PixelizeDownSample = Shader.PropertyToID("_STYLIZE_PIXEL_DOWNSAMPLE");
        static readonly RenderTargetIdentifier RT_PixelizeDownSample = new RenderTargetIdentifier(ID_PixelizeDownSample);
        static readonly int ID_PixelGridColor = Shader.PropertyToID("_PixelGridColor");
        static readonly int ID_PixelGridWidth = Shader.PropertyToID("_PixelGridWidth");

        static readonly int ID_OilPaintKernel = Shader.PropertyToID("_OilPaintKernel");
        static readonly int ID_OilPaintSize = Shader.PropertyToID("_OilPaintSize");

        static readonly int ID_ObraDitherScale = Shader.PropertyToID("_ObraDitherScale");
        static readonly int ID_ObraDitherStrength = Shader.PropertyToID("_ObraDitherStrength");
        static readonly int ID_ObraDitherColor = Shader.PropertyToID("_ObraDitherColor");
        private static readonly int ID_ObraDitherStep = Shader.PropertyToID("_ObraDitherStep");

        static readonly int ID_BilateralSize = Shader.PropertyToID("_BilateralSize");
        static readonly int ID_BilateralFactor = Shader.PropertyToID("_BilateralFactor");
        #endregion
        public override bool Validate(ref RenderingData _renderingData,ref DStylize _data)
        {
            switch (_data.m_Stylize)
            {
                case EStylize.Pixel:
                {
                    if (m_Material.EnableKeywords(_data.pixelGrid))
                    {
                        m_Material.SetColor(ID_PixelGridColor,_data.pixelGridColor);
                        m_Material.SetVector(ID_PixelGridWidth, new Vector2(_data.gridWidth, 1f - _data.gridWidth));
                    }

                } break;
                case EStylize.BilateralFilter:
                {
                    m_Material.SetFloat(ID_BilateralSize, _data.bilaterailSize);
                    m_Material.SetFloat(ID_BilateralFactor, _data.bilateralFactor);
                } break;
                case EStylize.ObraDithering:
                {
                    m_Material.SetFloat(ID_ObraDitherScale, _data.obraDitherScale);
                    m_Material.SetFloat(ID_ObraDitherStrength, _data.obraDitherStrength);
                    m_Material.SetColor(ID_ObraDitherColor, _data.obraDitherColor);
                    m_Material.SetFloat(ID_ObraDitherStep,_data.obraDitherStep);
                } break;
                case EStylize.OilPaint:
                {
                    int kernel = _data.oilPaintKernel / 2;
                    m_Material.SetVector(ID_OilPaintKernel, new Vector2(-kernel, kernel+ 1));
                    m_Material.SetFloat(ID_OilPaintSize, _data.oilPaintSize);
                } break;
            }
            return base.Validate(ref _renderingData,ref _data);
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref DStylize _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            switch (_data.m_Stylize)
            {
                default:
                    base.Execute(_descriptor, ref _data, _buffer, _src, _dst, _context, ref _renderingData);
                    break;
                case EStylize.Pixel:
                    _buffer.GetTemporaryRT(ID_PixelizeDownSample, _descriptor.width / _data.downSample, _descriptor.height / _data.downSample, 0, FilterMode.Point, _descriptor.colorFormat);
                    _buffer.Blit(_src, RT_PixelizeDownSample);
                    _buffer.Blit(RT_PixelizeDownSample, _dst, m_Material, 0);
                    _buffer.ReleaseTemporaryRT(ID_PixelizeDownSample);
                    break;
                case EStylize.OilPaint:
                    _buffer.Blit(_src, _dst, m_Material, 1);
                    break;
                case EStylize.ObraDithering:
                    _buffer.Blit(_src, _dst, m_Material, 2);
                    break;
                case EStylize.BilateralFilter:
                    _buffer.Blit(_src, _dst, m_Material, 3);
                    break;
            }
        }
    }
}
