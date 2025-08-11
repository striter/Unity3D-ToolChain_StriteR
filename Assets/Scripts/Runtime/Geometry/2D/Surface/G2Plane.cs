using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Plane
    {
        [PostNormalize]public float2 normal;
        public float distance;
        [HideInInspector] public float2 position;
        public G2Plane(float2 _normal,float _distance)
        {
            normal = _normal;
            distance = _distance;
            position = default;
            Ctor();
        }

        public G2Plane(float2 _normal, float2 _position)
        {
            normal = _normal;
            position = _position;
            distance = math.dot(_normal, _position);
        }

        void Ctor()
        {
            position = normal * distance;
        }
    }

    [Serializable]
    public partial struct G2Plane : IGeometry<float2>, ISerializationCallbackReceiver
    {
        public static readonly G2Plane kDefault = new(new float2(0,1),0f);
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            normal = math.normalize(normal);
            Ctor();
        }
        
        public G2Plane Flip() => new G2Plane(-normal,position); 
        public static implicit operator float3(G2Plane _plane)=>_plane.normal.to3xy(_plane.distance);
        public bool IsPointFront(float2 _point) =>  math.dot(_point.to3xy(-1),this)>0;
        public float2 Origin => position;
        
        public void DrawGizmos() => DrawGizmos(5f);
        public void DrawGizmos(float _radius = 5f)
        {
            var direction = normal.cross();
            Gizmos.DrawLine((position + direction * _radius).to3xz(),( position - direction*_radius).to3xz() );
            UGizmos.DrawArrow(position.to3xz(),normal.to3xz(),1f,.1f);
        }
        
        public G2Ray ToRay() => new G2Ray(position, normal.cross());

        public static G2Plane FromEquation(float _A, float _C)
        {
            return new G2Plane(-new float2(1f, _A).normalize().cross(),new float2(0, _C));
        }
        //https://www.mathsisfun.com/data/least-squares-regression.html
        public static G2Plane LeastSquaresRegression(IEnumerable<float2> _positions)
        {
            var sumX = 0f;
            var sumY = 0f;
            var sumXX = 0f;
            var sumXY = 0f;
            var N = 0;
            foreach (var position in _positions)
            {
                N += 1;
                sumX += position.x;
                sumY += position.y;
                sumXX += position.x * position.x;
                sumXY += position.x * position.y;
            }
            
            var m = (N*sumXY - sumX*sumY) / (N*sumXX - sumX*sumX);
            var b = (sumY - m * sumX) / N;
            return FromEquation(m,b);
        }
    }

    public static class G2Plane_Extension
    {
        public static float dot(this G2Plane _plane, float2 _point) => math.dot(_point.to3xy(-1), _plane);
    }
}