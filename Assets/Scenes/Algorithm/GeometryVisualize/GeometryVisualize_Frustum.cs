using System;
using Geometry.Voxel;
using UnityEngine;

namespace ExampleScenes.Algorithm.Geometry
{
    public class GeometryVisualize_Frustum : MonoBehaviour
    {
        public GFrustum m_Frustum;
        public bool m_DrawPlanes;
        public bool m_DrawRays;
        public bool m_DrawBounding;

        public GBox[] m_IntersectionAABBs;
        private void OnDrawGizmos()
        {
            var frustumPlanes = m_Frustum.GetFrustumPlanes();
            var frustumRays = m_Frustum.GetFrustumRays();
            var frustumPoints = frustumRays.GetFrustumPoints();
            
            if (m_DrawPlanes)
            {
                foreach (var tuple in frustumPlanes.LoopIndex())
                {
                    var frustumPlane = tuple.value;
                    Gizmos.color = UColor.IndexToColor(tuple.index);
                    var localToWorldMatrix = transform.localToWorldMatrix;
                    Gizmos.matrix = localToWorldMatrix;
                    Gizmos.DrawRay(frustumPlane.position,frustumPlane.normal);
                    Gizmos.matrix =  localToWorldMatrix * Matrix4x4.TRS( frustumPlane.position,Quaternion.LookRotation(frustumPlane.normal),Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero,Vector3.one.SetZ(0f));
                }
            }
            
            Gizmos.matrix = transform.localToWorldMatrix;
            if (m_DrawRays)
            {
                foreach (var tuple in frustumRays.LoopIndex())
                {
                    Gizmos.color = UColor.IndexToColor(tuple.index);
                    Gizmos_Extend.DrawArrow(tuple.value.origin,tuple.value.direction,1f,.05f);
                }
            }
            Gizmos.color = Color.white;
            frustumPoints.DrawGizmos();
            foreach (var aabb in m_IntersectionAABBs)
            {
                Gizmos.color = frustumPlanes.FrustumPlaneAABBIntersection(aabb,frustumPoints) ? Color.green : Color.red;
                aabb.DrawGizmos();
            }
            if (m_DrawBounding)
            {
                Gizmos.color = Color.white.SetAlpha(.5f);
                frustumPoints.bounding.DrawGizmos();
            }
            
        }
    }
}