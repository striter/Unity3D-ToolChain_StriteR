using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IShape<Dimension>:IShapeGizmos where Dimension:struct 
    {
        Dimension Center { get; }
    }

    public interface IRound<Dimenison>  where Dimenison : struct
    {
        public float Radius { get; }
        public int kMaxBoundsCount { get; }
        public IRound<Dimenison> Create(IList<Dimenison> _positions);
    }
    
    public interface ISDF<Dimension> : IShape<Dimension> where Dimension :struct
    {
        public float SDF(Dimension _position);
    }
    
    public interface ISDF:ISDF<float3>{}

    public static class ISDF_Extension
    {
        public static bool Contains<T>(this ISDF<T> _sdf, T _position,float _epsilon = 0) where T:struct => _sdf.SDF(_position) <= _epsilon;
        public static float Distance<T>(this ISDF<T> _sdf,T _position) where T:struct => _sdf.SDF(_position);
    }
    
    public interface IShapeGizmos { }
    
    public static class IShape_Extension
    {
        public static void DrawGizmos(this IShapeGizmos _shape)
        { 
#if UNITY_EDITOR
            var method = typeof(Gizmos_Geometry).GetMethod("DrawGizmos", new[] {_shape.GetType()});
            if (method == null)
                throw new NotImplementedException($"Create a DrawGizmos method in {nameof(Gizmos_Geometry)} for {_shape.GetType()}");
            method.Invoke(null,new object[]{_shape});
#endif
        }
    }
}