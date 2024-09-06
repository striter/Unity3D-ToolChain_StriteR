using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension.Sphere;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Optimize.Imposter
{
    public enum EImposterCount
    {
        _4 = 4,
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
    }
    
    [Serializable]
    public struct ImposterInput 
    {
        public ESphereMapping mapping;
        public EImposterCount count;
        public int cellResolution;

        public float2 CellTexelSizeNormalized => 1f / (float)count;
        public int2 CellCount => (int)count;

        public float4 GetImposterTexel() => new float4(CellCount.x, CellCount.y, CellTexelSizeNormalized.x, CellTexelSizeNormalized.y);
        public int2 TextureResolution =>  CellCount * cellResolution;
        
        public static readonly ImposterInput kDefault = new ImposterInput() {
            mapping = ESphereMapping.OctahedralHemisphere,
            count = EImposterCount._8,
            cellResolution = 256,
        };

        public IEnumerable<ImposterCorner> GetImposterViewsNormalized()
        {
            for (var j = 0; j < CellCount.y; j++)
            for (var i = 0; i < CellCount.x; i++)
                yield return CellIndexToCorner(new int2(i, j ));
        }

        public ImposterCorner GetImposterCorner(float3 _directionOS)
        {
            var count = (int)this.count;
            var uvCS = mapping.SphereToUV(_directionOS);
            var cellIndexActual = (int2)math.floor(uvCS * count) % count;
            return CellIndexToCorner(cellIndexActual);
        }

        public ImposterCorner GetImposterCorner(float2 _uv)
        {
            var count = (int)this.count;
            var cellIndexActual = (int2)math.floor(_uv * count) % count;
            return CellIndexToCorner(cellIndexActual);
        }

        public ImposterCorner CellIndexToCorner(int2 _cellIndexActual)
        {
            var texelMinRect = _cellIndexActual * CellTexelSizeNormalized;
            var mappingUV = (_cellIndexActual + kfloat2.one * .5f) / (int)count;
            return new ImposterCorner() {cellIndex = _cellIndexActual, uvRect = G2Box.Minmax(texelMinRect, texelMinRect + CellTexelSizeNormalized), direction = mapping.UVToSphere(mappingUV) };
        }

        public struct ImposterCorner
        {
            public G2Box uvRect;
            public float3 direction;
            public int2 cellIndex;
        }

        public (G2Quad corners, float4 weights, float3 centroid) GetImposterViews(float3 _directionOS)
        {
            mapping.InvBilinearInterpolate(_directionOS,CellCount.x,out var corners,out var weights );
            return (corners, weights, _directionOS);
        }

        private const float kGizmosSkipBias = 0.2f;
        public void DrawGizmos(float3 _viewDirection)
        {
            foreach (var corner in GetImposterViewsNormalized())
            {
                var direction = corner.direction;
                if (math.dot(direction, _viewDirection) < kGizmosSkipBias)
                    continue;
                var color = (corner.cellIndex/(float2)(int)count).to4(0, 1f).toColor();
                Gizmos.color =color.SetA(1f);
                Gizmos.DrawWireSphere(direction, .015f);
                UGizmos.DrawString($"{corner.cellIndex}",direction, 0.02f);
            }
        }

        #if UNITY_EDITOR
        public void DrawHandles(float3 _viewDirection)
        {
            foreach (var corner in GetImposterViewsNormalized())
            {
                var direction = corner.direction;
                if (math.dot(direction, _viewDirection) < kGizmosSkipBias)
                    continue;
                var color = (corner.cellIndex/(float2)(int)count).to4(0, 1f).toColor();
                UnityEditor.Handles.color =color.SetA(1f);
                UnityEditor.UHandles.DrawWireSphere(direction, .015f);
                UnityEditor.UHandles.DrawString($"{corner.cellIndex}",direction, 0.02f);
            }
        }
        #endif
    }

    public class ImposterShaderProperties
    {
        public static readonly int kRotation = Shader.PropertyToID("_Rotation");
        public static readonly int kWeights = Shader.PropertyToID("_Weights");

        public static readonly string kBounding = "_ImposterBoundingSphere";
        public static readonly int kBoundingID = Shader.PropertyToID(kBounding);
        
        public static readonly string kTexel = "_ImposterTexel";
        public static readonly int kTexelID = Shader.PropertyToID(kTexel);
        
        public static readonly string kMode = "_MAPPING";
        public static readonly int kModeID = Shader.PropertyToID(kMode);
    }
}