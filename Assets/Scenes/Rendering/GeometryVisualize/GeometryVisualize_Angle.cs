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
    public class GeometryVisualize_Angle : MonoBehaviour
    {
        public Vector3 m_Position;
        private void OnDrawGizmos()
        {
            if (SceneView.currentDrawingSceneView == null)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero,Vector3.forward);


            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero,m_Position);

            var rad = UMath.GetRadClockWise(Vector2.up,new Vector2(m_Position.x,m_Position.z));
            Gizmos_Extend.DrawString( Vector3.zero,(UMath.Rad2Deg*rad).ToString());
        }
    }
    #endif
}