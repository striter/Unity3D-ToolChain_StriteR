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
    [MFoldout(nameof(m_ManualCast),false)][PostNormalize] public float3 m_CastDirection = kfloat3.forward;
    [MFoldout(nameof(m_ManualCast),true)] public GRay m_ManualCastRay = GRay.kDefault;
    private IShape[] drawingShapes = {GTriangle.kDefault,GDisk.kDefault,GQuad.kDefault,   GBox.kDefault, GCapsule.kDefault, GCylinder.kDefault, GSphere.kOne, GEllipsoid.kDefault, GCone.kDefault};
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
            var center = volume.Center;
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

            var position = boundingBox.GetPoint(math.sin(UTime.time) *1.2f / 2);
            if (volume is ISDF sdf)
            {
                Gizmos.color =  sdf.Contains(position) ? Color.green : Color.yellow;
                Gizmos.DrawSphere(position, .02f);
            }
            
            var ray = m_ManualCast
                ? m_ManualCastRay
                : new GRay(boundingBox.GetSupportPoint(-m_CastDirection) - m_CastDirection * .2f + math.sin(UTime.time),
                    m_CastDirection);
            Gizmos.color = Color.blue.SetA(.5f);
            UGizmos.DrawArrow(ray.origin, ray.direction, 0.5f, .1f);
            if (volume is IRayVolumeIntersection volumeIntersection)
            {
                var intersect = volumeIntersection.RayIntersection(ray, out var distances);
                if (!intersect)
                    continue;
                Gizmos.color = Color.blue;

                var distancePoint = ray.GetPoint(distances.x);
                Gizmos.DrawSphere(distancePoint, .02f);

                Gizmos.color = KColor.kIndigo;
                var distancePoint2 = ray.GetPoint(distances.sum());
                Gizmos.DrawSphere(distancePoint2, .02f);

                Gizmos.DrawLine(ray.origin, distancePoint2);
            }
            else if (volume is IRayIntersection rayIntersection)
            {
                var intersect = rayIntersection.RayIntersection(ray, out var distance);
                if (!intersect)
                    continue;
                Gizmos.color = KColor.kRoyalBlue;

                var distancePoint = ray.GetPoint(distance);
                Gizmos.DrawSphere(distancePoint, .02f);
                Gizmos.DrawLine(ray.origin, distancePoint);
            }

        }
    }
#endif
}
