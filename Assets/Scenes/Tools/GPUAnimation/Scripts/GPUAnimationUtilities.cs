using Rendering;
using UnityEngine;

namespace Runtime.Optimize.GPUAnimation
{
    public static class UGPUAnimation
    {
        #region ShaderProperties
        static readonly int ID_AnimationTex = Shader.PropertyToID("_AnimTex");
        static readonly int ID_FrameBegin = Shader.PropertyToID("_AnimFrameBegin");
        static readonly int ID_FrameEnd = Shader.PropertyToID("_AnimFrameEnd");
        static readonly int ID_FrameInterpolate = Shader.PropertyToID("_AnimFrameInterpolate");
        #endregion
        
        public static void ApplyMaterial(this GPUAnimationData _data,Material _sharedMaterial)
        {
            _sharedMaterial.SetTexture(ID_AnimationTex,_data.m_BakeTexture);
            _sharedMaterial.EnableKeywords(_data.m_Mode);
        }

        public static void ApplyPropertyBlock(this AnimationTickerOutput _output, MaterialPropertyBlock _block)
        {
            _block.SetInt(ID_FrameBegin, _output.cur);
            _block.SetInt(ID_FrameEnd, _output.next);
            _block.SetFloat(ID_FrameInterpolate, _output.interpolate);
        }
        
        public static Int2 GetTransformPixel(int _transformIndex,int row, int frame)
        {
            return new Int2(_transformIndex * 3 + row, frame);
        }

        public static Int2 GetVertexPositionPixel(int _vertexIndex, int frame)
        {
            return new Int2(_vertexIndex*2, frame);
        }
        public static Int2 GetVertexNormalPixel(int _vertexIndex, int frame)
        {
            return new Int2(_vertexIndex*2+1, frame);
        }
    }
}