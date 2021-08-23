using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hexagon;

namespace GridTest
{
    public class GridTest_HexOffset : MonoBehaviour
    {
        public bool m_Flat = false;
        public int m_CellSizeX = 3, m_CellSizeY = 4;
        public float m_Radius = 1;
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            UHexagon.flat=m_Flat;
            for (int i = -m_CellSizeX; i < m_CellSizeX+1; i++)
            for (int j = -m_CellSizeY; j < m_CellSizeY+1; j++)
            {
                Gizmos.color = Color.green;
                Vector3 axisPos =  new HexOffset(i,j).ToPixel(m_Flat).ToWorld(m_Radius);
                axisPos+= (i * j + i) * Vector3.up * 0.01f;
                Vector3[] hexagonList = UHexagon.GetPoints().Select(p=>p.ToWorld(m_Radius) + axisPos).ToArray();
                Gizmos_Extend.DrawLines(hexagonList);
            }
        }
        #endif
    }

    public static class GridHelper
    {
        public static Vector3 ToWorld(this HexPixel _pixel,float _scale)
        {
            _pixel *= _scale;
            return new Vector3(_pixel.x,0,_pixel.y);
        }

        public static HexPixel ToPixel(this Vector3 _world,float _scale)
        {
            _world/=_scale;
            return new HexPixel(_world.x,  _world.z);
        }
        
        #if UNITY_EDITOR
        public static void DrawHexagon(this HexAxial _axial,float _size,float _offset=0)
        {
            Vector3[] hexagonList = UHexagon.GetPoints().Select(p=>p.ToWorld(_size) + _axial.ToPixel().ToWorld(_size)+Vector3.up*_offset).ToArray();
            Gizmos_Extend.DrawLines(hexagonList);
        }
        public static Vector3 SceneRayHit(Vector3 _planePosition)
        {
            UnityEditor.SceneView _sceneView=UnityEditor.SceneView.currentDrawingSceneView;
            if (!_sceneView)
                return Vector3.zero;
            GRay ray = _sceneView.camera.ScreenPointToRay( TEditor.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, _planePosition);
            var hitPoint = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            return hitPoint;
        }
        #endif
    }
}