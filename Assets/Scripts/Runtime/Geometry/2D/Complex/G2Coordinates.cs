using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Coordinates
    {
        public float2 origin;
        [PostNormalize] public float2 right;
        [PostNormalize] public float2 up;
        public G2Coordinates(float2 _origin, float2 _right)
        {
            this = default;
            origin = _origin;
            right = _right.normalize();
            Ctor();
        }
        void Ctor()
        {
            up = (-right.normalize()).cross();
        }

    }

    [Serializable]
    public partial struct G2Coordinates : ISerializationCallbackReceiver
    {
        public G2Coordinates Flip()
        {
            right = -right;
            up = -up;
            return this;
        }

        public static G2Coordinates PrincipleComponentAnalysis(IEnumerable<float2> _points)
        {
            var center = _points.Average();
            var m = center;
            var a00 = _points.Average(p => umath.pow2(p.x - m.x));
            var a11 = _points.Average(p => umath.pow2(p.y - m.y));
            var a01mirror = _points.Average(p => (p.x - m.x)*(p.y-m.y));
            new float2x2(a00,a01mirror,a01mirror,a11).GetEigenVectors(out var _,out var right);
            return new G2Coordinates(center,right);
        }
        
        public G2Line Right() => new (origin,origin + right);
        public G2Line Up() => new (origin,origin + up);
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();

        public static G2Coordinates kDefault = new(kfloat2.zero,kfloat2.right);
        
        public G2Plane UpPlane() => new (up,origin);
        public G2Plane RightPlane() => new (right,origin);
        
        public void DrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(origin.to3xz(),0.05f);
            Gizmos.color = Color.red;
            Right().DrawGizmos();
            Gizmos.color = Color.green;
            Up().DrawGizmos();
        }

        public float2 Origin => origin;
        public float2 Normal => up;
    }
}