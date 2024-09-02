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
        public float2 CellWorldSpaceSizeNormalized => 1f / (int)count;
        
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
            var cellWorldMin = _cellIndexActual * CellWorldSpaceSizeNormalized;
            var worldRect = G2Box.Minmax(cellWorldMin, cellWorldMin + CellWorldSpaceSizeNormalized);
            var texelMinRect = _cellIndexActual * CellTexelSizeNormalized;
            return new ImposterCorner() {cellIndex = _cellIndexActual, uvRect = G2Box.Minmax(texelMinRect, texelMinRect + CellTexelSizeNormalized),worldRect = worldRect, direction = mapping.UVToSphere((_cellIndexActual + kfloat2.one * .5f) * CellWorldSpaceSizeNormalized) };
        }

        public struct ImposterCorner
        {
            public G2Box uvRect;
            public G2Box worldRect;
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
                var rect = corner.worldRect;
                var direction = corner.direction;
                if (math.dot(direction, _viewDirection) < kGizmosSkipBias)
                    continue;
                var color = rect.center.to4(0, 1f).toColor();
                Gizmos.color =color.SetA(1f);
                Gizmos.DrawWireSphere(direction, .015f);
                var cell = rect.center * (int)count;
                UGizmos.DrawString($"{cell.x} | {cell.y}",direction, 0.02f);

                Gizmos.color = color.SetA(.2f);
                var bottom = rect.min;
                var top = rect.max;
                var left = new float2(bottom.x,top.y);
                var right = new float2(top.x,bottom.y);
                UGizmos.DrawLinesConcat(mapping.UVToSphere(bottom), mapping.UVToSphere(left), mapping.UVToSphere(top), mapping.UVToSphere(right));
            }
        }

        #if UNITY_EDITOR
        public void DrawHandles(float3 viewDirection)
        {
            foreach (var corner in GetImposterViewsNormalized())
            {
                var rect = corner.worldRect;
                var direction = corner.direction;
                if (math.dot(direction, viewDirection) < kGizmosSkipBias)
                    continue;
                
                var color = rect.center.to4(0, 1f).toColor();
                UnityEditor.Handles.color =color.SetA(1f);
                UnityEditor.UHandles.DrawWireSphere(direction, .015f);
                var cell = rect.center * (int)count;
                UnityEditor.UHandles.DrawString($"{cell.x} | {cell.y}",direction, 0.02f);

                UnityEditor.Handles.color = color.SetA(.2f);
                var bottom = rect.min;
                var top = rect.max;
                var left = new float2(bottom.x,top.y);
                var right = new float2(top.x,bottom.y);
                UnityEditor.UHandles.DrawLinesConcat(mapping.UVToSphere(bottom), mapping.UVToSphere(left), mapping.UVToSphere(top), mapping.UVToSphere(right));
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