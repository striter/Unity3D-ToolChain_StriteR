using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Explicit;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    using static USphereExplicit;
    
    [Serializable]
    public struct ImposterInput : ISerializationCallbackReceiver
    {
        public ESphereMapping mapping;
        public bool hemisphere;
        public int width;
        public int height;
        public int cellResolution;

        [Readonly] public float2 cellSizeNormalized; 
        [Readonly] public int2 cellCount;

        public float4 GetImposterTexel() =>
            new float4(cellCount.x, cellCount.y, cellSizeNormalized.x, cellSizeNormalized.y);
        public int2 TextureResolution =>  cellCount * cellResolution;
        public ImposterInput Ctor()
        {
            cellCount = new int2(width, hemisphere ? height / 2 : height);
            cellSizeNormalized = 1f / new float2(cellCount);
            return this;
        }

        public static readonly ImposterInput kDefault = new ImposterInput()
        {
            hemisphere = false,
            width = 8,
            height = 4,
            cellResolution = 256,
        }.Ctor();

        public float3 TransformUVToWorld(float2 _uv)
        {
            if(hemisphere)
                _uv.y = math.lerp(0.5f,1f, _uv.y);
            return Mapping(_uv,mapping);
        }

        public float2 TransformWorldToUV(float3 _direction)
        {
            if (hemisphere)
            {
                _direction.y = math.max(_direction.y,0.01f);
                _direction = _direction.normalize();
            }

            var uv = InvMapping(_direction,mapping);
            if(hemisphere)
                uv.y = umath.invLerp(0.5f,1f, uv.y);
            return uv;
        }
        
        public IEnumerable<ImposterCorner> GetImposterViewsNormalized()
        {
            for (var j = 0; j < cellCount.y; j++)
            for (var i = 0; i < cellCount.x; i++)
                yield return GetImposterCorner(new float2(i, j)  * cellSizeNormalized);
        }

        public ImposterCorner GetImposterCorner(float2 _rectMin)
        {
            var rect = G2Box.Minmax(_rectMin, _rectMin + cellSizeNormalized);
            var direction = TransformUVToWorld(rect.center);
            return new ImposterCorner() { rect = rect, direction = direction };
        }

        public struct ImposterCorner
        {
            public G2Box rect;
            public float3 direction;
        }

        public class ImposterOutput
        {
            public Triangle<ImposterCorner> corners = new ();
            public float3 weights = new ();
            public float3 centroid;
        }
        
        private static List<ImposterCorner> kCorners = new();
        private static readonly ImposterOutput kOuptut = new();
        public ImposterOutput GetImposterViews(float3 directionOS)
        {
            // var value = GetImposterViewsNormalized().MinElement(p => math.dot(p.direction, -_directionOS));
            // yield return (value, 1);
            kOuptut.corners = default;
            kOuptut.weights = default;
            kOuptut.centroid = kfloat3.zero;
            
            var uv = DirectionToUV(directionOS);
            var viewDirection = UVToDirection(math.round(uv * cellCount) / cellCount);
            kCorners.Clear();
            foreach (var corner in GetImposterViewsNormalized())
            {
                foreach (var rectCorner in corner.rect)
                {
                    if ((TransformUVToWorld(rectCorner) - viewDirection).sqrmagnitude() > 0.001f)
                        continue;
                    
                    kCorners.Add( corner);
                    break;
                }
            }
            
            var F = -viewDirection;    
            var cellRotation = Quaternion.LookRotation(F, Vector3.up);
            var U = math.mul(cellRotation, kfloat3.up);
            var R = math.mul(cellRotation, kfloat3.right);
            U = math.cross(R,viewDirection).normalize();
            R = math.cross(viewDirection,U).normalize();

            var axis = new GAxis(viewDirection,U,R);
            kCorners.Sort( (a, b) => axis.ProjectRadClockwise(a.direction) . CompareTo(axis.ProjectRadClockwise(b.direction)));
            
            switch (kCorners.Count)
            {
                default:
                {
                    Debug.LogError("kCorners.Count = " + kCorners.Count);
                    return kOuptut;
                }
                case 0:
                    return kOuptut;
                case 1:
                    kOuptut.corners[0] = kCorners[0];
                    kOuptut.weights[0] = 1;
                    kOuptut.centroid = kCorners[0].direction;
                    return kOuptut;
                case 2:
                {
                    var corner0 = kCorners[0].direction;
                    var corner1 = kCorners[1].direction;
                    var offset = corner1 - corner0;
                    var ray = new GRay(corner0,offset.normalize());
                    var length = offset.magnitude();
                    var projection= ray.Projection(directionOS);
                    var weight1Normalized = 1 - (projection / length);
                    // var weight1 = umath.invLerp(0.4f, 0.5f, weight1Normalized).saturate();
                    // var weight2 = umath.invLerp(0.4f,0.5f,  1 - weight1Normalized).saturate();

                    kOuptut.centroid = ray.GetPoint(projection);
                    if (weight1Normalized > .5f)
                    {
                        kOuptut.corners[0] = kCorners[0];
                        kOuptut.weights[0] = weight1Normalized;
                        kOuptut.corners[1] = kCorners[1];
                        kOuptut.weights[1] = 1 - weight1Normalized;
                    }
                    else
                    {
                        kOuptut.corners[0] = kCorners[1];
                        kOuptut.weights[0] = 1 - weight1Normalized;
                        kOuptut.corners[1] = kCorners[0];
                        kOuptut.weights[1] = weight1Normalized;
                    }
                    return kOuptut;
                }
                case 4:
                {
                    var weights = new GQuad(kCorners[0].direction,kCorners[1].direction,kCorners[2].direction,kCorners[3].direction);
            
                    weights.GetTriangles(out var triangle1,out var triangle2);
                    var weight1 = triangle1.GetWeightsToPoint(directionOS);
                    // weight1 = umath.invLerp( 0f,0.35f, weight1).saturate();
                    
                    kOuptut.centroid = directionOS;
                    if (!weight1.anyLesser(-0.05f))
                    {
                        kOuptut.corners[0] = kCorners[0];
                        kOuptut.corners[1] = kCorners[1];
                        kOuptut.corners[2] = kCorners[2];
                        kOuptut.weights = weight1;
                        return kOuptut;
                    }
                    var weight2 = triangle2.GetWeightsToPoint(directionOS);
                    kOuptut.corners[0] = kCorners[0];
                    kOuptut.corners[1] = kCorners[2];
                    kOuptut.corners[2] = kCorners[3];
                    kOuptut.weights = weight2;
                    return kOuptut;
                }
            }
        }

        public float3 UVToDirection(float2 _uv) => TransformUVToWorld(_uv); 
        public float2 DirectionToUV(float3 _direction) => TransformWorldToUV(_direction);
        public void DrawGizmos()
        {
            foreach (var corner in GetImposterViewsNormalized())
            {
                var rect = corner.rect;
                var direction = corner.direction;
                var color = rect.center.to4(0, 1f).toColor();
                Gizmos.color =color.SetA(1f);
                Gizmos.DrawWireSphere(direction, .015f);
                var cell = rect.min * cellCount;
                UGizmos.DrawString($"{cell.x} | {cell.y}",direction, 0.02f);

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
        public static readonly int kRotation = Shader.PropertyToID("_Rotation");
        public static readonly int kWeights = Shader.PropertyToID("_Weights");

        public static readonly int kBounding = Shader.PropertyToID("_ImposterBoundingSphere");
        public static readonly int kTexel = Shader.PropertyToID("_ImposterTexel");
    }
}