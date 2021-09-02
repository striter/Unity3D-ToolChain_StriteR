
using System;
using Geometry.Three;
using GridTest;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using TTouchTracker;
using UnityEditor;
using UnityEngine;

namespace ConvexGrid
{
    #if UNITY_EDITOR
    public partial class ConvexGrid
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
            var hitPos = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            var hitCoord = (transform.InverseTransformPoint(hitPos) / m_CellRadius).ToCoord();
            var hitHex=hitCoord.ToCube();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0: ValidateArea(hitArea);  break;
                    case 1: break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R: Clear(); break;
                }
            }
            m_GridSelected.Check( ValidateSelection(hitCoord,out m_QuadSelected));
        }
        
        #region Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.matrix = m_TransformMatrix;
            foreach (ConvexArea area in m_Areas.Values)
                area.DrawProceduralGizmos();
            DrawGrid();
            DrawSelection();
        }

        void DrawGrid()
        {
            Gizmos.color = Color.green.SetAlpha(.3f);
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.2f);
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLines(quad.m_HexQuad.ConstructIteratorArray(p=>m_Vertices[p].m_Coord.ToWorld()));
        }

        void DrawSelection()
        {
            if (m_QuadSelected == -1)
                return;
            Gizmos.color = Color.white.SetAlpha(.3f);
            Gizmos_Extend.DrawLines(m_Quads[m_QuadSelected].m_HexQuad.ConstructIteratorArray(p=>m_Vertices[p].m_Coord.ToWorld()));
            Gizmos.color = Color.cyan;
            var vertex = m_Vertices[m_GridSelected];
            Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.5f);
            Gizmos.color = Color.yellow;
            foreach (var quad in vertex.m_RelativeQuads)
                Gizmos.DrawSphere(((Coord)(quad.m_GeometryQuad.center)).ToWorld(),.3f);
        }
        #endregion
    }
    #endif
}