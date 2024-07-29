using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IShape<Dimension> where Dimension:struct 
    {
        Dimension Origin { get; }
        public void DrawGizmos();
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
}