using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    
    public class GeometryVisualizeClipping : MonoBehaviour
    {
        public GTriangle gTriangle = GTriangle.kDefault;
        public bool gDirected = false;
        public GPlane gPlane = GPlane.kDefault;
        
        [Header("2D Clipping")]
        public G2Polygon door = G2Polygon.kDefault;
        public G2Plane plane = G2Plane.kDefault;
        public G2Triangle triangle = G2Triangle.kDefault;
        
        private void OnDrawGizmos()
        {
            //Door Clip
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetA(.5f);
            door.DrawGizmos();
            var movedPlane = new G2Plane(plane.normal,plane.distance * math.sin(UTime.time)*2);
            movedPlane.DrawGizmos(UGeometry.GetBoundingCircle(door.positions).radius);

            if (door.DoorClip(movedPlane, out var clippedPolygon))
            {
                Gizmos.color = Color.yellow;
                clippedPolygon.DrawGizmos();
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*5f);
            Gizmos.color = Color.white.SetA(.5f);
            movedPlane.DrawGizmos(UGeometry.GetBoundingCircle(triangle.triangle.Iterate()).radius);
            triangle.DrawGizmos();
            if (triangle.Clip(movedPlane, out var clippedG2Shape))
            {
                Gizmos.color = Color.yellow;
                clippedG2Shape.DrawGizmos();
            }

            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back*5f);
            Gizmos.color = Color.white.SetA(.5f);
            var movedGPlane  = new GPlane(gPlane.normal,gPlane.distance * math.sin(UTime.time)*2);
                
            movedGPlane.DrawGizmos(UGeometry.GetBoundingCircle(triangle.triangle.Iterate()).radius);
            gTriangle.DrawGizmos();
            if (gTriangle.Clip(movedGPlane, out var clippedGShape,gDirected))
            {
                Gizmos.color = Color.yellow;
                clippedGShape.DrawGizmos();
            }
            
        }
    }
}