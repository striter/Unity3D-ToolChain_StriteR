using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering
{
    [Serializable]
    public struct GCloudShadowData
    {
        public Texture2D m_ShadowTexture;
        [Range(0,1)] public float m_ShadowStrength ;
        [Range(1,200)]public float m_ShadowScale;
        [Range(0,100)]public float m_ShadowPlaneDistance;
        [Range(0f,1f)] public float m_StepBegin;
        [Range(0f,.5f)]public float m_StepWidth;
        public Vector2 m_Flow;

        public static readonly GCloudShadowData kDefault = new GCloudShadowData()
        {
            m_ShadowStrength = 1f,
            m_ShadowScale = 50f,
            m_ShadowPlaneDistance =  50f,
            m_StepBegin = 0.5f,
            m_StepWidth =  0.1f,
        };
        
        static void Apply(GCloudShadowData _data, int _shapeID, int _flowID, int _idTexture)
        {
            Shader.EnableKeyword(kKeyword);
            Shader.SetGlobalVector(_shapeID,new Vector4(1f-_data.m_ShadowStrength,_data.m_ShadowScale,_data.m_ShadowPlaneDistance));
            Shader.SetGlobalVector(_flowID,new Vector4(_data.m_StepBegin,_data.m_StepBegin+_data.m_StepWidth,_data.m_Flow.x,_data.m_Flow.y));
            Shader.SetGlobalTexture(_idTexture,_data.m_ShadowTexture);
        }
        
        public static void Dispose()
        {
            Shader.DisableKeyword(kKeyword);
            Shader.SetGlobalTexture(kCloudTexture,null);
            Shader.SetGlobalTexture(kCloudTextureInterpolate,null);
        }
        
        private static readonly string kKeyword = "_CLOUDSHADOW";
        private static readonly int kCloudTexture = Shader.PropertyToID("_CloudShadowTexture"); 
        private static readonly int kCloudShape = Shader.PropertyToID("_CloudParam1");
        private static readonly int kCloudFlow = Shader.PropertyToID("_CloudParam2");
        public void Apply() => Apply(this,kCloudShape,kCloudFlow,kCloudTexture);
        
        private static readonly int kCloudTextureInterpolate = Shader.PropertyToID("_CloudShadowTexture_Interpolate");
        private static readonly int kCloudShapeInterpolate = Shader.PropertyToID("_CloudParam1_Interpolate");
        private static readonly int kCloudFlowInterpolate = Shader.PropertyToID("_CloudParam2_Interpolate");
        public static void Interpolate(GCloudShadowData _src,GCloudShadowData _dst,float _interpolation)
        {
            if (1f - _interpolation < float.Epsilon)
            {
                _dst.Apply();
                return;
            }

            _src.Apply();
            if (_interpolation < float.Epsilon)
                return;
            Apply( _dst,kCloudShapeInterpolate,kCloudFlowInterpolate,kCloudTextureInterpolate);
        }
        

    }

}