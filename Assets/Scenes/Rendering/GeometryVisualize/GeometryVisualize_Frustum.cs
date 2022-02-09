using System;
using Geometry.Voxel;
using UnityEngine;

namespace ExampleScenes.Rendering.GeometryVisualize
{
    public class GeometryVisualize_Frustum : MonoBehaviour
    {
        public GFrustum m_Frustum;
        public bool m_DrawPlanes;
        public bool m_DrawRays;

        public GBox[] m_IntersectionAABBs;
        private void OnDrawGizmos()
        {
            var frustumPlanes = m_Frustum.GetFrustumPlanes();
            var frustumRays = m_Frustum.GetFrustumRays(Vector3.zero, Quaternion.identity);
            
            if (m_DrawPlanes)
            {
                foreach (var tuple in frustumPlanes.LoopIndex())
                {
                    var frustumPlane = tuple.value;
                    Gizmos.color = UColor.IndexToColor(tuple.index);
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawRay(frustumPlane.position,frustumPlane.normal);
                    Gizmos.matrix =  transform.localToWorldMatrix * Matrix4x4.TRS( frustumPlane.position,Quaternion.LookRotation(frustumPlane.normal),Vector3.one);
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
            
            
            Gizmos.color = Color.white.SetAlpha(.5f);
            frustumRays.DrawGizmos();

            foreach (var aabb in m_IntersectionAABBs)
            {
                Gizmos.color = frustumPlanes.FrustumAABBIntersection(aabb) ? Color.green : Color.red;
                aabb.DrawGizmos();
            }
        }
    }
}