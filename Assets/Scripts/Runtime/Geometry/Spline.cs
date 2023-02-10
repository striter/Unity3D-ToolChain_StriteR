using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct GSplineCoords
    {
        public float3 position;
        public float weight;

        public GSplineCoords(float3 _position, float _weight)
        {
            position = _position;
            weight = _weight;
        }
    }
    
    [Serializable]
    public struct GSpline
    {
        public GSplineCoords[] coordinates;
        public bool closed;
        
        public static readonly GSpline kDefault = new GSpline()
        {
            coordinates = new GSplineCoords[]{new GSplineCoords(new float3(-1,0,-1),1)
                ,new GSplineCoords(new float3(0,0,1),.5f)
                ,new GSplineCoords(new float3(1,0,-1),1f)},
            closed = true,
        };
        
        public GSpline(GSplineCoords[] _coordinates,bool _closed)
        {
            coordinates = _coordinates;
            closed = _closed;
        }
        
        
    }

}