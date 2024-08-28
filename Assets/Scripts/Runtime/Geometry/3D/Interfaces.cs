using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public interface IGeometry  : IGeometry<float3> { }

    public interface ILine : IGeometry { }
    public interface ICurve : IGeometry {
        float3 Evaluate(float _value);
    }
    public interface ICurveTangents : ICurve
    {
        float3 EvaluateTangent(float _value);
    }

    public interface ISpline : ICurve
    {
        IEnumerable<float3> Coordinates { get; }
    }
    
    public interface ISurface : IGeometry
    {
        public float3 Normal { get; }
    }
    
    public interface IVolume : IGeometry
    {
        float3 GetSupportPoint(float3 _direction);
        public GBox GetBoundingBox();
        public GSphere GetBoundingSphere();
    }
    
    public interface IConvex : IGeometry , IEnumerable<float3>
    {
        public IEnumerable<GLine> GetEdges();
        public IEnumerable<float3> GetAxes();
    }

}