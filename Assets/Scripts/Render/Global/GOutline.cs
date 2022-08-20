using System;
using UnityEngine;

namespace Rendering
{
    [Serializable]
    public struct GOutlineData
    {
        [Range(0,1)]public float width;
        public Color color;
        [MinMaxRange(nameof(distanceFadeEnd),10f, 100f)] public float distanceFade;
        [HideInInspector]public float distanceFadeEnd;
        public static readonly GOutlineData kDefault = new GOutlineData()
        {
            width = 0.01f,
            color =  Color.black.SetAlpha(.5f),
            distanceFade = 20f,
            distanceFadeEnd = 25f,
        };
        
        private static readonly int kOutlineWidth = Shader.PropertyToID("_gOutlineParameters");
        private static readonly int kOutlineColor = Shader.PropertyToID("_gOutlineColor");
        public void Apply()
        {
            Shader.SetGlobalVector(kOutlineWidth,new Vector4(width,0f,distanceFade,distanceFadeEnd));
            Shader.SetGlobalColor(kOutlineColor,color);
        }
    }
}