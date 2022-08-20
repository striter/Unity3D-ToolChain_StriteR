using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace PolyGrid
{
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Extensions;
    [ExecuteInEditMode]
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

        private readonly UEditorTime kEditorTime = new UEditorTime();
        void EditorTick() => Tick(kEditorTime.deltaTime);
        
        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay(sceneView.GetScreenPoint());
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometryIntersect.RayPlaneDistance(plane, ray));
            var hitCoord = hitPos.ToCoord();
            var hitHex=hitCoord.ToCube();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                        ValidateArea(hitArea);
                        break;
                    case 1: break;
                }
        
            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R:
                    {
                        Clear(); 
                        break;
                    }
                    
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

            //Check Invalid Quads
            List<HexQuad> quads = new List<HexQuad>();
            foreach (var area in m_Areas.Values)
                quads.AddRange(area.m_Quads);

            List<HexCoord> invalidCoords = new List<HexCoord>();
            foreach (var quad in quads)
            {
                if(quads.Count(p=>p.MatchVertexCount(quad) == 2)==4)
                    continue;
                invalidCoords.TryAddRange(quad);
            }

            List<GridAreaData> areaData = new List<GridAreaData>();
            foreach (var area in m_Areas.Values)
            {
                if(area.m_State!= EConvexIterate.Relaxed)
                    continue;
                GridAreaData data = new GridAreaData {
                    identity = area.m_Area, m_Quads = new HexQuad[area.m_Quads.Count],m_Vertices = new GridVertexData[area.m_Vertices.Count]
                };
                foreach (var valueTuple in area.m_Quads.LoopIndex())
                    data.m_Quads[valueTuple.index]= valueTuple.value;
                foreach (var valueTuple in area.m_Vertices.LoopIndex())
                {
                    var identity = valueTuple.value.Key;
                    var coord = valueTuple.value.Value * 1f / 6f * UMath.SQRT2;
                    var invalid = invalidCoords.Contains(identity);
                    data.m_Vertices[valueTuple.index] = new GridVertexData(){identity = identity,coord = coord,invalid=invalid};
                }
                areaData.Add(data);
            }
            
            GridRuntimeData _data = ScriptableObject.CreateInstance<GridRuntimeData>();
            _data.areaData = areaData.ToArray();
            UEAsset.CreateOrReplaceMainAsset(_data,UEPath.FileToAssetPath( filePath));
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