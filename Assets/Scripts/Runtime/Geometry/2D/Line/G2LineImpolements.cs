using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Line: ISDF<float2> , IRayIntersection2 , ISerializationCallbackReceiver
    {
        
        public G2Line Clip(G2Box _box) => _box.Clip(this,out var _clipped) ? _clipped : this;
        public float2 Origin => start;
        public void DrawGizmos() => Gizmos.DrawLine(start.to3xz(),end.to3xz());
        public void DrawGizmosXY() => Gizmos.DrawLine(start.to3xy(), end.to3xy());

        public float SDF(float2 _position)
        {
            var lineDirection = direction;
            var pointToStart = _position - start;
            return math.length(umath.cross(lineDirection, pointToStart));
        }

        public bool Intersect(G2Ray _ray, out float distance)
        {
            var projection = _ray.Projection(this);
            distance = projection.x;
            return distance > 0 && projection.y >= 0 && projection.y <= length;
        }

        public float2 Projection( G2Line _line2)
        {
            var proj = ((G2Ray)this).Projection(_line2);
            return new float2(math.clamp(proj.x, 0, length), math.clamp(proj.y, 0, _line2.length));
        }
        
        public float2 GetPoint(float _distance) => start + direction * _distance;
        public float2 GetPointNormalized(float _normalizedDistance) => start + direction * _normalizedDistance * length;

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
    }
}