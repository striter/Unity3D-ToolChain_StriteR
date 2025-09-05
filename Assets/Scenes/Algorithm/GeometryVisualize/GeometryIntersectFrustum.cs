using System;
using Runtime.Geometry;
using Unity.Mathematics;
using System.Linq.Extensions;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.GeometryVisualize
{
    public class GeometryIntersectFrustum : MonoBehaviour
    {
        public GFrustum m_Frustum;
        public bool m_DrawPlanes;
        public bool m_DrawRays;
        public bool m_DrawBounding;

        public GBox[] m_IntersectionAABBs;
        public GSphere[] m_IntersectBSs = { GSphere.kDefault };
#if UNITY_EDITOR
        private float time;
        private void OnDrawGizmos()
        {
            time += UTime.deltaTime;
            
            var frustumPlanes = m_Frustum.planes;
            var frustumRays = m_Frustum.rays;
            var frustumPoints = frustumRays.GetFrustumPoints();
            
            if (m_DrawPlanes)
            {
                foreach (var tuple in frustumPlanes.WithIndex())
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
                foreach (var tuple in frustumRays.WithIndex())
                {
                    Gizmos.color = UColor.IndexToColor(tuple.index);
                    UGizmos.DrawArrow(tuple.value.origin,tuple.value.direction,1f,.05f);
                }
            }
            Gizmos.color = Color.white;
            frustumPoints.DrawGizmos();
            int index = 0;
            foreach (var aabb in m_IntersectionAABBs)
            {
                var deltaPosition = math.cos(time * 2 * math.PI * Noise.Value.Unit1f1((float)index++/m_IntersectionAABBs.Length)) * .5f;
                var deltaAABB= aabb.Move(deltaPosition) ;
                Gizmos.color = m_Frustum.Intersect(deltaAABB) ? Color.green : Color.red;
                deltaAABB.DrawGizmos();
            }

            foreach (var bs in m_IntersectBSs)
            {
                Gizmos.color = m_Frustum.Intersect(bs) ? Color.green : Color.red;
                bs.DrawGizmos();
            }
            if (m_DrawBounding)
            {
                Gizmos.color = Color.white.SetA(.5f);
                frustumPoints.bounding.DrawGizmos();
            }
        }
#endif
    }
}