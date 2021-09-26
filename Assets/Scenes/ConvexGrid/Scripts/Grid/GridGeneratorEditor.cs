using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using LinqExtentions;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace ConvexGrid
{
    #if UNITY_EDITOR
    using TEditor;
    using UnityEditor;
    [ExecuteInEditMode]
    public partial class GridGenerator
    {
        public readonly Dictionary<HexCoord, Coord> m_ExistVertices = new Dictionary<HexCoord, Coord>();
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
            var hitPos = ray.GetPoint(UGeometryIntersect.RayPlaneDistance(plane, ray));
            var hitCoord = hitPos.ToCoord();
            var hitHex=hitCoord.ToCube();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                        ValidateArea(hitArea, m_ExistVertices, area =>
                        {
                            Debug.Log($"Area{area.m_Area.coord} Constructed!");
                            foreach (var pair in area.m_Vertices)
                                m_ExistVertices.TryAdd(pair.Key,pair.Value);
                        });
                        break;
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

        public void Bake()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset")) 
                return;

            List<GridAreaData> areaData = new List<GridAreaData>();
            foreach (var area in m_Areas.Values)
            {
                if(area.m_State!= EConvexIterate.Relaxed)
                    continue;
                GridAreaData data = new GridAreaData();
                data.identity = area.m_Area;
                data.m_Quads = new HexQuad[area.m_Quads.Count];
                foreach (var valueTuple in area.m_Quads.LoopIndex())
                    data.m_Quads[valueTuple.index]= valueTuple.value;
                data.m_Vertices = new GridVertexData[area.m_Vertices.Count];
                foreach (var valueTuple in area.m_Vertices.LoopIndex())
                    data.m_Vertices[valueTuple.index] = new GridVertexData(){identity = valueTuple.value.Key,coord = valueTuple.value.Value};
                areaData.Add(data);
            }
            
            GridRuntimeData _data = ScriptableObject.CreateInstance<GridRuntimeData>();
            _data.areaData = areaData.ToArray();
            UEAsset.CreateAssetCombination(UEPath.FileToAssetPath( filePath), _data);
        }
    }

    [CustomEditor(typeof(GridGenerator))]
    public class GridGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bake"))
                (target as GridGenerator).Bake();
            EditorGUILayout.EndHorizontal();
        }
        
    }
    #endif
}