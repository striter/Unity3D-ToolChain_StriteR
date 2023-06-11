using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    
    public class GeometryVisualizeClipping : MonoBehaviour
    {
        [Header("2D Clipping")]
        public G2Polygon door = G2Polygon.kDefault;
        public G2Plane plane = G2Plane.kDefault;
        public G2Triangle triangle = G2Triangle.kDefault;
        
        private void OnDrawGizmos()
        {
            //Door Clip
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetAlpha(.5f);
            door.DrawGizmos();
            var movedPlane = new G2Plane(plane.normal,plane.distance * math.sin(UTime.time)*2);
            movedPlane.DrawGizmos(UBounds.GetBoundingCircle(door.positions).radius);

            if (door.DoorClip(movedPlane, out var clippedPolygon))
            {
                Gizmos.color = Color.yellow;
                clippedPolygon.DrawGizmos();
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*5f);
            Gizmos.color = Color.white.SetAlpha(.5f);
            movedPlane.DrawGizmos(UBounds.GetBoundingCircle(triangle.triangle.Iterate()).radius);
            triangle.DrawGizmos();
            if (triangle.Clip(movedPlane, out var clippedQuad))
            {
                Gizmos.color = Color.yellow;
                clippedQuad.DrawGizmos();
            }

        }
    }
}