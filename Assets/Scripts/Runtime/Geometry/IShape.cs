using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry
{
    public interface IShapeGizmos
    {
        
    }

    public interface IShapeDimension<T> where T:struct
    {
        T Center { get; }
    }


    public interface IShape : IShapeDimension<float3>, IShapeGizmos
    {
        float3 GetSupportPoint(float3 _direction);
    }


    public static class UShape
    {
        public static void DrawGizmos(this IShapeGizmos _shape)
        { 
            var method = typeof(UGizmos).GetMethod("DrawGizmos", new[] {_shape.GetType()});
            if (method == null)
                throw new NotImplementedException($"Create a DrawGizmos method in {nameof(UGizmos)} for {_shape.GetType()}");
            method.Invoke(null,new object[]{_shape});
        }

    }
}