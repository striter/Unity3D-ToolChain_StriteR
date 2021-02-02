using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_Blurs : PostEffectBase<ImageEffect_Blurs,ImageEffectParam_Blurs>
    {
    }

    public enum enum_BlurType
    {
        AverageSinglePass = 0,
        Average = 1,
        Gaussian = 2,
        Hexagon=3,
    }

    enum enum_BlurPass
    {
        Average_Simple=0,
        Average_Horizontal=1,
        Average_Vertical=2,
        Gaussian_Horizontal=3,
        Gaussian_Vertical=4,
        Average_Directional=5,
        Hexagon_CombineCell=6,
    }

    [Serializable]
    public struct ImageEffectParam_Blurs 
    {
        [Range(0.25f, 2.5f)] public float blurSize;
        [Range(1, 4)] public int downSample;
        [Range(1, 8)] public int iteration;
        public enum_BlurType blurType;
        public static readonly ImageEffectParam_Blurs m_Default = new ImageEffectParam_Blurs()
        {
            blurSize = 1.0f,
            downSample = 2,
            iteration = 1,
            blurType = enum_BlurType.AverageSinglePass,
        };
    }

    public class ImageEffect_Blurs : ImageEffectBase<ImageEffectParam_Blurs>
    {
        public ImageEffect_Blurs() : base()
        {

        }
        #region ShaderProperties
        const string KW_ClipAlpha = "CLIP_ZERO_ALPHA";
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        static readonly int ID_BlurDirection = Shader.PropertyToID("_BlurDirection");
        static readonly int ID_HexagonCell1 = Shader.PropertyToID("_HexagonCell1");
        static readonly int ID_HexagonCell2 = Shader.PropertyToID("_HexagonCell2");
        static readonly int ID_HexagonCell3 = Shader.PropertyToID("_HexagonCell3");

        #endregion
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, ImageEffectParam_Blurs _param)
        {
            if (_material.passCount <= 1)
            {
                Debug.LogWarning("Invalid Material Pass Found Of Blur!");
                Graphics.Blit(_src, _dst);
                return;
            }

            int rtW = _src.width / _param.downSample;
            int rtH = _src.height / _param.downSample;
            float blurSize = _param.blurSize / _param.downSample;

            RenderTexture rt1 = _src;
            for (int i = 0; i < _param.iteration; i++)
            {
                switch(_param.blurType)
                {
                    case enum_BlurType.AverageSinglePass:
                        {
                            int pass = (int)enum_BlurPass.Average_Simple;
                            if (i != _param.iteration - 1)
                            {
                                RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                                _material.SetFloat(ID_BlurSize, blurSize * (1 + i));
                                Graphics.Blit(rt1, rt2, _material, pass);
                                if (i != 0)
                                    RenderTexture.ReleaseTemporary(rt1);
                                rt1 = rt2;
                                continue;
                            }
                            Graphics.Blit(rt1, _dst, _material, pass);
                        }
                        break;

                    case enum_BlurType.Average:
                    case enum_BlurType.Gaussian:
                        {
                            int horizontalPass =  (int)(_param.blurType - 1) * 2 + 1;
                            int verticalPass = horizontalPass + 1;

                            // vertical blur
                            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                            _material.SetFloat(ID_BlurSize, blurSize * (1 + i));
                            Graphics.Blit(rt1, rt2, _material, horizontalPass);
                            if (i != 0)
                                RenderTexture.ReleaseTemporary(rt1);
                            rt1 = rt2;

                            if (i != _param.iteration - 1)
                            {
                                // horizontal blur
                                rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                                Graphics.Blit(rt1, rt2, _material, verticalPass);
                                RenderTexture.ReleaseTemporary(rt1);
                                rt1 = rt2;
                                continue;
                            }
                            Graphics.Blit(rt1, _dst, _material, horizontalPass);
                        }
                        break;
                    case enum_BlurType.Hexagon:
                        {
                            int cellPass = (int)enum_BlurPass.Average_Directional;
                            int combinePass = (int)enum_BlurPass.Hexagon_CombineCell;
                            _material.SetFloat(ID_BlurSize, blurSize * (1 + i));

                            RenderTexture cell1 = RenderHexagonCell(rt1, rtW, rtH, _material, cellPass, TTile_Hexagon.HexagonHelper.C_UnitHexagonPoints[0], TTile_Hexagon.HexagonHelper.C_UnitHexagonPoints[1]);
                            RenderTexture cell2 = RenderHexagonCell(rt1, rtW, rtH, _material, cellPass, TTile_Hexagon.HexagonHelper.C_UnitHexagonPoints[2], TTile_Hexagon.HexagonHelper.C_UnitHexagonPoints[3]);
                            RenderTexture cell3 = RenderHexagonCell(rt1, rtW, rtH, _material, cellPass, TTile_Hexagon.HexagonHelper.C_UnitHexagonPoints[4], TTile_Hexagon.HexagonHelper.C_UnitHexagonPoints[5]);

                            _material.SetTexture(ID_HexagonCell1, cell1);
                            _material.SetTexture(ID_HexagonCell2, cell2);
                            _material.SetTexture(ID_HexagonCell3, cell3);
                            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                            Graphics.Blit(rt1,rt2, _material, combinePass);
                            if (i != 0)
                                RenderTexture.ReleaseTemporary(rt1);
                            rt1 = rt2;

                            RenderTexture.ReleaseTemporary(cell1);
                            RenderTexture.ReleaseTemporary(cell2);
                            RenderTexture.ReleaseTemporary(cell3);

                            if (i!=_param.iteration-1)
                                continue;
                            Graphics.Blit(rt1, _dst, _material, cellPass);
                        }
                        break;
                }
            }

            if(_param.iteration!=1)
                RenderTexture.ReleaseTemporary(rt1);
        }

        RenderTexture RenderHexagonCell(RenderTexture _src,int rtW,int rtH,Material _material,int pass,Vector2 _hexagonPoint1,Vector2 _hexagonPoint2)
        {
            RenderTexture temporaryTexture = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
            _material.SetVector(ID_BlurDirection,_hexagonPoint1);
            Graphics.Blit(_src, temporaryTexture,_material, pass);

            RenderTexture targetTexture = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
            _material.SetVector(ID_BlurDirection, _hexagonPoint2-_hexagonPoint1);
            Graphics.Blit(temporaryTexture, targetTexture,_material, pass);

            RenderTexture.ReleaseTemporary(temporaryTexture);
            return targetTexture;
        }

    }
}