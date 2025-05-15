using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Curves;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

public class GeometryVisualizeShapes : MonoBehaviour
{
    [PostNormalize] public float3 m_SupportDirection = kfloat3.forward;
    public bool m_ManualCast = false;
    [Foldout(nameof(m_ManualCast),false)][PostNormalize] public float3 m_CastDirection = kfloat3.forward;
    [Foldout(nameof(m_ManualCast),true)] public GRay m_ManualCastRay = GRay.kDefault;
    private IGeometry[] drawingShapes = {GTriangle.kDefault,GDisk.kDefault,GQuad.kDefault, GPolygon.kBunny,   GBox.kDefault, GCapsule.kDefault, GCylinder.kDefault, GSphere.kOne, GEllipsoid.kDefault, GCone.kDefault };
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        m_SupportDirection = m_SupportDirection.normalize();
        foreach (var (index, value) in drawingShapes.LoopIndex())
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * 3f * index);
            Gizmos.color = Color.white;
            value.DrawGizmos();

            if (value is not IVolume volume)
                continue;

            Gizmos.color = Color.green;
            var center = volume.Origin;
            Gizmos.DrawSphere(center, .05f);

            Gizmos.color = Color.red;
            var supportPoint = volume.GetSupportPoint(m_SupportDirection);
            Gizmos.DrawLine(center, supportPoint);
            Gizmos.DrawSphere(supportPoint, .02f);

            var boundingBox = volume.GetBoundingBox();
            Gizmos.color = KColor.kOrange.SetA(.2f);
            boundingBox.DrawGizmos();

            Gizmos.color = KColor.kSienna.SetA(.2f);
            volume.GetBoundingSphere().DrawGizmos();

            var position = boundingBox.GetCenteredPoint(math.sin(UTime.time) *1.2f / 2);
            if (volume is ISDF sdf)
            {
                Gizmos.color =  sdf.Contains(position) ? Color.green : Color.yellow;
                Gizmos.DrawSphere(position, .02f);
            }
            
            var ray = m_ManualCast
                ? m_ManualCastRay
                : new GRay(boundingBox.GetSupportPoint(-m_CastDirection) - m_CastDirection * .2f + math.sin(UTime.time),
                    m_CastDirection);
            
            if (volume is IRayVolumeIntersection volumeIntersection)
            {
                Gizmos.color = KColor.kIndigo;
                UGizmos.DrawArrow(ray.origin, ray.direction, 0.5f, .1f);
                var intersect = volumeIntersection.RayIntersection(ray, out var distances);
                if (!intersect)
                    continue;

                var distancePoint = ray.GetPoint(distances.x);
                Gizmos.DrawSphere(distancePoint, .02f);

                var distancePoint2 = ray.GetPoint(distances.sum());
                Gizmos.DrawSphere(distancePoint2, .02f);

                Gizmos.DrawLine(ray.origin, distancePoint2);
            }
            else if (volume is IRayIntersection rayIntersection)
            {
                Gizmos.color = KColor.kRoyalBlue;
                UGizmos.DrawArrow(ray.origin, ray.direction, 0.5f, .1f);
                var intersect = rayIntersection.RayIntersection(ray, out var distance);
                if (!intersect)
                    continue;

                var distancePoint = ray.GetPoint(distance);
                Gizmos.DrawSphere(distancePoint, .02f);
                Gizmos.DrawLine(ray.origin, distancePoint);
            }

        }
    }
#endif
}
