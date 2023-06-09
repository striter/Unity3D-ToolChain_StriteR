using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    
    public class GeometryVisualizeDoorClip : MonoBehaviour
    {
        public G2Polygon door = G2Polygon.kDefault;
        public G2Plane plane = G2Plane.kDefault;
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetAlpha(.5f);
            door.DrawGizmos();
            var movedPlane = new G2Plane(plane.normal,plane.distance * math.sin(UTime.time)*2);
            movedPlane.DrawGizmos(UBounds.GetBoundingCircle(door.positions).radius);

            Gizmos.color = Color.yellow;
            door.Clip(movedPlane).DrawGizmos();
        }
    }

}