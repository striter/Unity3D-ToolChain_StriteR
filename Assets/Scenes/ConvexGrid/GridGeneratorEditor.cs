using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using UnityEditor;
using UnityEngine;

namespace ConvexGrid
{
    #if UNITY_EDITOR
    public partial class GridGenerator
    {
        private void OnValidate()
        {
            Setup();
            Clear();
        }
        private void OnEnable()
        {
            if (Application.isPlaying)
                return;
            
            Setup();
            EditorApplication.update += EditorTick;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;

            Clear();
            EditorApplication.update -= EditorTick;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void EditorTick() => Tick(TEditor.EditorTime.deltaTime);

        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay(TEditor.UECommon.GetScreenPoint(sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometryVoxel.RayPlaneDistance(plane, ray));
            var hitCoord = hitPos.ToCoord();
            var hitHex=hitCoord.ToCube();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0: ValidateArea(hitArea,new Dictionary<HexCoord, ConvexVertex>(),area=>Debug.Log($"Area{area.m_Area.m_Coord} Constructed!"));  break;
                    case 1: break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R: Clear(); break;
                }
            }
        }
        
                
        #region Gizmos

        public bool m_Gizmos;
        private void OnDrawGizmos()
        {
            if (!m_Gizmos)
                return;
            foreach (RelaxArea area in m_Areas.Values)
                area.DrawProceduralGizmos();
        }

        #endregion

    }
    #endif
}