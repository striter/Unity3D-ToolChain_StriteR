using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.GeometryVisualize
{
    
    public class GeometryVisualizeClipping : MonoBehaviour
    {
        public bool m_DynamicPlane;
        public GTriangle gTriangle = GTriangle.kDefault;
        public GPlane gPlane = GPlane.kDefault;
        
        [Header("2D Clipping")]
        public G2Polygon door = G2Polygon.kDefault;
        public G2Plane plane = G2Plane.kDefault;
        public G2Triangle triangle = G2Triangle.kDefault;

        [Header("2D Box Clip")]
        public G2Line line = G2Line.kDefault;
        public G2Box box = G2Box.kDefault;

        private void OnDrawGizmos()
        {
            //Door Clip
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetA(.5f);
            door.DrawGizmos();
            var movedPlane = plane;
            if (m_DynamicPlane)
                movedPlane = new G2Plane(plane.normal,plane.distance * math.sin(UTime.time)*2);
            movedPlane.DrawGizmos(G2Circle.GetBoundingCircle(door.positions).radius);

            if (door.Clip(movedPlane, out var clippedPolygon))
            {
                Gizmos.color = Color.yellow;
                clippedPolygon.DrawGizmos();
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*5f);
            Gizmos.color = Color.white.SetA(.5f);
            movedPlane.DrawGizmos(G2Circle.GetBoundingCircle(triangle.triangle.Iterate()).radius);
            triangle.DrawGizmos();
            if (triangle.Clip(movedPlane, out var clippedG2Shape))
            {
                Gizmos.color = Color.yellow;
                clippedG2Shape.DrawGizmos();
            }

            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back*5f);
            Gizmos.color = Color.white.SetA(.5f);
            var movedGPlane  = gPlane;
            if (m_DynamicPlane)
                movedGPlane = new GPlane(gPlane.normal,gPlane.distance * math.sin(UTime.time)*2);
            movedGPlane.DrawGizmos(G2Circle.GetBoundingCircle(triangle.triangle.Iterate()).radius);
            gTriangle.DrawGizmos();
            if (gTriangle.Clip(movedGPlane, out var clippedGShape))
            {
                Gizmos.color = Color.yellow;
                clippedGShape.DrawGizmos();
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back*10f);
            Gizmos.color = Color.white.SetA(.5f);
            var newLine = line + kfloat2.up * math.sin(UTime.time) * 1f;
            box.DrawGizmos();
            newLine.DrawGizmos();
            var success = box.Clip(newLine,out var clippedLine);
            if (success)
            {
                Gizmos.color = Color.yellow;
                clippedLine.DrawGizmos();
            }
        }
    }
}