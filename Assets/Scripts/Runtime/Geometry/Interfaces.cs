using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IGeometry<Dimension>:IGizmos where Dimension:struct 
    {
        Dimension Origin { get; }
    }

    public interface IRound<Dimenison>  where Dimenison : struct
    {
        public float Radius { get; }
        public int kMaxBoundsCount { get; }
        public IRound<Dimenison> Create(IList<Dimenison> _positions);
    }

    public interface IConvex<Dimension> : IEnumerable<Dimension> where Dimension : struct
    {
        
    }
    
    public interface ISDF<Dimension> : IGeometry<Dimension> where Dimension :struct
    {
        public float SDF(Dimension _position);
    }

    public static class ISDF_Extension
    {
        public static bool Contains<T>(this ISDF<T> _sdf, T _position,float _epsilon = 0) where T:struct => _sdf.SDF(_position) <= _epsilon;
        public static float Distance<T>(this ISDF<T> _sdf,T _position) where T:struct => _sdf.SDF(_position);
    }
}