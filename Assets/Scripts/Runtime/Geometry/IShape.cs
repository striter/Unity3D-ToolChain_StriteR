using System;
using Unity.Mathematics;

namespace Geometry
{
    public interface IShape
    {
        float3 GetSupportPoint(float3 _direction);
        float3 Center { get; }
    }

    public interface IShape2D
    {
        float2 GetSupportPoint(float2 _direction);
        float2 Center { get; }
    }

    public static class UGizmos
    {
        public static void DrawGizmos(this IShape _shape)
        { 
            var method = typeof(Gizmos_Extend).GetMethod("DrawGizmos", new[] {_shape.GetType()});
            if (method == null)
                throw new NotImplementedException($"Create a DrawGizmos method in {nameof(Gizmos_Extend)} for {_shape.GetType()}");
            method.Invoke(null,new object[]{_shape});
        }
    }
}