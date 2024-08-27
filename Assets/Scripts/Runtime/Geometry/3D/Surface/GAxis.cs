using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GAxis : ISerializationCallbackReceiver , IRayIntersection , ISurface
    {
        public float3 origin;
        [PostNormalize]public float3 right;
        [PostNormalize]public float3 up;
        [NonSerialized] public float3 forward;
        public GAxis(float3 _origin, float3 _right, float3 _up)
        {
            this = default;
            origin = _origin;
            right = _right;
            up = _up;
            Ctor();
        }
        void Ctor()
        {
            forward = math.cross(right, up);
        }

        public static GAxis ForwardBillboard(float3 origin,float3 forward)
        {
            var billboardRotation = Quaternion.LookRotation(forward, Vector3.up);
            var U = math.mul(billboardRotation, kfloat3.up);
            var R = math.mul(billboardRotation, kfloat3.right);
            U = math.cross(R,-forward).normalize();
            R = math.cross(-forward,U).normalize();
            return new GAxis(origin, R, U);
        }

        public GLine Right() => new GLine(origin, origin + right);
        public GLine Up() => new GLine(origin, origin + up);

        public float3 GetPoint(float2 _uv) => origin + _uv.x * right + _uv.y * up;
        public float2 GetUV(float3 _point)
        {
            var v0 = right;
            var v1 = up;
            var v2 = _point - origin;
            var dot00 = math.dot(v0, v0);
            var dot01 = math.dot(v0, v1);
            var dot02 = math.dot(v0, v2);
            var dot11 = math.dot(v1, v1);
            var dot12 = math.dot(v1, v2);

            var denominator = (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) / denominator;
            var v = (dot00 * dot12 - dot01 * dot02) / denominator;
            return new float2(u,v);
        }
        
        public float ProjectRadClockwise(float3 _point) => umath.getRadClockwise(this.GetUV(this.Projection(_point)),kfloat2.up);
        
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();

        public static GAxis kDefault = new GAxis(kfloat3.zero,kfloat3.right,kfloat3.forward);
        public static  implicit operator GPlane(GAxis _axis) => new GPlane(_axis.forward,_axis.origin);
        public bool RayIntersection(GRay _ray, out float distance) => ((GPlane)this).RayIntersection(_ray,out distance);

        public void DrawGizmos()
        {
            Gizmos.color = Color.red;
            Right().DrawGizmos();
            Gizmos.color = Color.green;
            Up().DrawGizmos();
        }

        public float3 Origin => origin;
        public float3 Normal => up;
    }
}