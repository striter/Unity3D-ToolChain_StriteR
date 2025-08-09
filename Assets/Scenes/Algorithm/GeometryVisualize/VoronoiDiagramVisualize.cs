using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Scenes.Algorithm.GeometryVisualize
{
    [ExecuteInEditMode]
    public class VoronoiDiagramVisualize : MonoBehaviour
    {
        public uint m_RandomCount = 128;
        public G2Box m_Bounds = G2Box.kDefault;
        public List<float2> m_Vertices = new List<float2>();

        [InspectorButton(true)]
        void Randomize()
        {
            m_Vertices.Clear();
            for (int i = 0; i < m_RandomCount; i++)
            {
                // points[i] = new float3(ldsPoints[i].x,0,ldsPoints[i].y)*kRandomRadius;
                var point = m_Bounds.GetPoint(URandom.Random2DQuad());
                m_Vertices.Add(point);
            }
        }

        [InspectorButton(true)]
        void Sequence()
        {
            m_Vertices.Clear();
            for (uint i = 0; i < m_RandomCount; i++)
            {
                var p = ULowDiscrepancySequences.Hammersley2D(i, m_RandomCount);
                m_Vertices.Add(m_Bounds.GetPoint(p));
            }
        }
        
        [InspectorButton(true)]
        void PoissonDisk()
        {
            var size = (int)math.sqrt(m_RandomCount);
            ULowDiscrepancySequences.PoissonDisk2D(size).Select(p=>m_Bounds.GetPoint(p) ).FillList(m_Vertices);
        }

        public bool m_Solid;
        [Foldout(nameof(m_Solid),true)] public ColorPalette m_VisualizationColor = ColorPalette.kDefault;
        public bool m_Dynamic = true;
        private float2 m_MousePosition;
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one);
            var vertices =  PoolList<float2>.Empty(nameof(VoronoiDiagramVisualize));
            vertices.Add(m_MousePosition);
            if (m_Dynamic)
                vertices.AddRange(m_Vertices.Select((p,index)=> m_Bounds.Clamp(p + math.sin(UTime.time * ULowDiscrepancySequences.Hammersley2D((uint)index,(uint)m_Vertices.Count)))));
            else
                vertices.AddRange(m_Vertices);

            var diagram = G2VoronoiDiagram.FromPositions(vertices);
            var initialSite = diagram.sites.nodes[0].position;
            foreach (var (index,cell) in diagram.ToCells(m_Bounds).WithIndex())
            {
                if (m_Solid)
                {
                    Gizmos.color = m_VisualizationColor.Evaluate( umath.repeat(math.distance(initialSite,cell.site) / m_Bounds.extent.magnitude(),1f));
                    cell.simplex.DrawGizmosSolidTriangle();
                }
                else
                {
                    Gizmos.color = UColor.IndexToColor(index);
                    Gizmos.DrawWireSphere(cell.site.to3xz(),.1f);
                    cell.simplex.Collapse(.99f).DrawGizmos();
                }
            }

            if (m_Solid)
                return;

            Gizmos.color = Color.white.SetA(.1f);
            diagram.DrawGizmos();
            foreach (var point in vertices)
                Gizmos.DrawWireSphere(point.to3xz(),.1f);
        }


        private Counter m_PointInserter = new(.05f);
        private void Update()
        {
            if (!Application.isPlaying)
                return;
            if (m_Vertices.Count < 256 && m_PointInserter.TickTrigger(Time.deltaTime))
            {
                m_Vertices.Add(m_Bounds.center);
                m_PointInserter.Replay();
            }
            UGeometry.LlyodRelaxation(m_Vertices,m_Bounds);
        }

#if UNITY_EDITOR
        private void OnEnable() => UnityEditor.SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => UnityEditor.SceneView.duringSceneGui -= OnSceneGUI;

        private void OnSceneGUI(UnityEditor.SceneView _sceneView)
        {
            if (Application.isPlaying)
                return;
            
            GRay ray = _sceneView.camera.ScreenPointToRay(UnityEditor.Extensions.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            ray.IntersectPoint(plane,out var hitPoint);
            m_MousePosition = m_Bounds.Clamp(((float3)transform.worldToLocalMatrix.MultiplyPoint(hitPoint)).xz);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
                UGeometry.LlyodRelaxation(m_Vertices,m_Bounds);
        }
#endif
    }
}