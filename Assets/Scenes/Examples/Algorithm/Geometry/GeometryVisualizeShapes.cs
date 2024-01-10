using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Curves;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

public class GeometryVisualizeShapes : MonoBehaviour
{
    private IShape3D[] drawingShapes = new IShape3D[]
    {
        // GPlane.kDefault, GTriangle.kDefault, GQuad.kDefault,GDisk.kDefault,
        GBox.kDefault, GCapsule.kDefault, GCylinder.kDefault, GSphere.kOne, GEllipsoid.kDefault, GCone.kDefault,
    };

    private void OnDrawGizmos()
    {
        
        foreach (var (index,value) in drawingShapes.LoopIndex())
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*1.5f * index);
            Gizmos.color = Color.white;
            value.DrawGizmos();

            Gizmos.color = KColor.kOrange;
            if(value is IBoundingBox3D bounds)
                bounds.GetBoundingBox().DrawGizmos();
        }
    }
}
