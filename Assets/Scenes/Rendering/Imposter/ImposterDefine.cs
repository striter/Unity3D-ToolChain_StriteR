using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Explicit;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    using static USphereExplicit;

    public enum EImposterLightMode
    {
        Static,
        Normal,
        Depth,
    }
    
    [Serializable]
    public struct ImposterInput : ISerializationCallbackReceiver
    {
        public ESphereMapping mapping;
        public EImposterLightMode lightMode;
        public bool hemisphere;
        public int width;
        public int height;
        public int cellResolution;

        private float2 cellSizeNormalized;
        private int2 cellCount;
        public int2 TextureResolution => new int2(width, height) * cellResolution;
        public ImposterInput Ctor()
        {
            cellCount = new int2(width, height);
            cellSizeNormalized = 1f / new float2(cellCount);
            return this;
        }

        public static readonly ImposterInput kDefault = new ImposterInput()
        {
            hemisphere = false,
            width = 8,
            height = 4,
            cellResolution = 256,
            lightMode = EImposterLightMode.Normal,
        }.Ctor();

        public float3 TransformUVToWorld(float2 _uv)
        {
            if(hemisphere)
                _uv.y = math.lerp(0.5f,1f, _uv.y);
            return Mapping(_uv,mapping);
        }

        public float2 TransformWorldToUV(float3 _direction)
        {
            var uv = InvMapping(_direction,mapping);
            if(hemisphere)
                uv.y = umath.invLerp(0.5f,1f, uv.y);
            return uv;
        }
        
        public IEnumerable<(G2Box _uvRect, float3 direction)> GetImposterViewsNormalized()
        {
            for (var j = 0; j < height; j++)
            for (var i = 0; i < width; i++)
            {
                var rectMin =  new float2(i, j)  * cellSizeNormalized;
                var rect = G2Box.Minmax(rectMin, rectMin + cellSizeNormalized);
                var direction = TransformUVToWorld(rect.center);
                yield return (rect,direction);
            }
        }

        public float3 UVToDirection(float2 _uv) => TransformUVToWorld(_uv);

        public G2Box UVToUVRect(float2 _uvCenter)
        {
            var min = _uvCenter - cellSizeNormalized * .5f;
            return G2Box.Minmax(min, min + cellSizeNormalized);
        }


        public void DrawGizmos()
        {
            foreach (var (rect,direction) in GetImposterViewsNormalized())
            {
                var color = rect.center.to4(0, 1f).toColor();
                Gizmos.color =color.SetA(1f);
                Gizmos.DrawWireSphere(direction, .015f);
                var cell = rect.min * cellCount;
                UGizmos.DrawString(direction, $"{cell.x} | {cell.y}",0.02f);

                Gizmos.color = color.SetA(.2f);
                var bottom = rect.min;
                var top = rect.max;
                var left = new float2(bottom.x,top.y);
                var right = new float2(top.x,bottom.y);
                UGizmos.DrawLinesConcat(TransformUVToWorld(bottom), TransformUVToWorld(left), TransformUVToWorld(top), TransformUVToWorld(right));
            }
            
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => Ctor();
    }

    public class ImposterDefine
    {
        public static readonly int kBounding = Shader.PropertyToID("_BoundingSphere");
        public static readonly int kRotation = Shader.PropertyToID("_Rotation");

    }
}