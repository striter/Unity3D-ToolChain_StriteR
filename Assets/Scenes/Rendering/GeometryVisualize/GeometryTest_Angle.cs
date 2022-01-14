using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEditor;
using UnityEngine;

namespace ExampleScenes.Rendering.GeometryVisualize
{
    #if UNITY_EDITOR
    using TEditor;
    public class GeometryTest_Angle : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            if (SceneView.currentDrawingSceneView == null)
                return;
            var plane = new GPlane(Vector3.up, Vector3.zero);
            var ray=SceneView.currentDrawingSceneView.camera.ScreenPointToRay(SceneView.currentDrawingSceneView.GetScreenPoint());
            var distance=UGeometryIntersect.RayPlaneDistance(plane, ray);
            if (distance < 0)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero,Vector3.forward);


            Gizmos.color = Color.green;
            var hitPositionWS = ray.GetPoint(distance);
            var hitPositionLS = transform.worldToLocalMatrix.MultiplyPoint(hitPositionWS);
            Gizmos.DrawLine(Vector3.zero,hitPositionLS);

            var rad = UMath.GetRadClockWise(Vector2.up,new Vector2(hitPositionLS.x,hitPositionLS.z));
            Gizmos_Extend.DrawString( Vector3.zero,(UMath.Rad2Deg*rad).ToString());
        }
    }
    #endif
}