using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IGeometry<Dimension> where Dimension:struct 
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

    public interface IConvex<Dimension> : IEnumerable<Dimension> where Dimension : struct
    {
        
    }
    
    public interface ISDF<Dimension> : IGeometry<Dimension> where Dimension :struct
    {
        public float SDF(Dimension _position);
    }

}