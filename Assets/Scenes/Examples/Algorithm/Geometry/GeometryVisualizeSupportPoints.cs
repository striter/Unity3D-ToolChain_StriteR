using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeSupportPoints : MonoBehaviour
    {
        public GPolygon polygon = GPolygon.kDefault;
        public GSphere sphere = GSphere.kOne;
        public GBox box = GBox.kDefault;
        [PostNormalize] public float3 supportPointDirection = kfloat3.forward;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var shape in new IShape[] {polygon, sphere, box})
            {
                shape.DrawGizmos();
                Gizmos_Extend.DrawArrow(shape.Center, supportPointDirection, .5f, .1f);
                Gizmos.DrawWireSphere(shape.GetSupportPoint(supportPointDirection), .1f);
            }
        }
    }
}