using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeSupportPoints : MonoBehaviour
    {
        public G2Box box = G2Box.kDefault;
        public G2Polygon polygon = G2Polygon.kDefault;
        public G2Circle sphere = G2Circle.kOne;
        [PostNormalize] public float2 supportPointDirection = kfloat2.up;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            int index = 0;
            foreach (var shape in new I2Shape[] {box, sphere,polygon})
            {
                Gizmos.color = index++ != 0 && box.Intersect(shape) ? Color.yellow : Color.white;
                shape.DrawGizmos();
                UGizmos.DrawArrow(shape.Center.to3xz(), supportPointDirection.to3xz(), .5f, .1f);
                Gizmos.DrawWireSphere(shape.GetSupportPoint(supportPointDirection).to3xz(), .1f);
            }
            
        }
    }
}