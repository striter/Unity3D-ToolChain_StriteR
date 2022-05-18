using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Voxel;
using UnityEngine;
using UnityEditor;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;

namespace ExampleScenes.Algorithm.HexagonGrid
{
    [ExecuteInEditMode]
    public class HexagonGridGizmos : MonoBehaviour
    {
        public bool m_Flat;
        public float m_CellRadius = 1;
        [Header("Area")]
        public int m_AreaRadius = 8;
        public int m_Tilling = 1;
        public bool m_Welded;
        public int m_MaxAreaRadius = 4;
#if UNITY_EDITOR
        private Coord m_HitPointCS;
        private HexCoord m_HitAxialCS;

        public enum EAxisVisualize
        {
            Invalid,
            Axial,
            Cube,
        }

        [NonSerialized]
        private readonly Dictionary<HexCoord, HexagonArea> m_Areas = new Dictionary<HexCoord, HexagonArea>();

        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnValidate() => Clear();

        void Clear()
        {
            m_Areas.Clear();
        }

        private void OnDrawGizmos()
        {
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius, m_Tilling,m_Welded);
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * m_CellRadius);

            foreach (var coord in UHexagon.GetCoordsInRadius( HexCoord.zero,50))
            {
                var area = UHexagonArea.GetBelongingArea(coord);

                var index = (area.coord.x-area.coord.y + int.MaxValue / 2) % 3;
                // var index = (i - k + int.MaxValue) % 3;
                switch (index)
                {
                    case 0: Gizmos.color = Color.red; break;
                    case 1: Gizmos.color = Color.blue; break;
                    case 2:  Gizmos.color = Color.green; break;
                }
                coord.DrawHexagon();
            }
            
            DrawAxis();
            DrawAreas();
            DrawTestGrids(m_HitPointCS, m_HitAxialCS);
        }

        void ValidateArea(HexCoord _positionCS,bool _include)
        {
            var area = UHexagonArea.GetBelongingArea(_positionCS);

            if (!area.coord.InRange(m_MaxAreaRadius))
                return;
           
            if(_include&&!m_Areas.ContainsKey(area.coord))
                m_Areas.Add(area.coord, area);
            else if (!_include && m_Areas.ContainsKey(area.coord))
                m_Areas.Remove(area.coord);
        }
        
        public EAxisVisualize m_AxisVisualize;

        private static class GUIHelper
        {
            public static readonly Color C_AxialColumn = Color.green;
            public static readonly Color C_AxialRow = Color.blue;
            public static readonly Color C_CubeX = Color.red;
            public static readonly Color C_CubeY = Color.green;
            public static readonly Color C_CubeZ = Color.blue;

            public static readonly GUIStyle m_AreaStyle = new GUIStyle
                {alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Normal,normal=new GUIStyleState(){textColor = Color.yellow}};

            public static readonly GUIStyle m_HitStyle = new GUIStyle
                {alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Normal,normal=new GUIStyleState(){textColor = Color.yellow}};
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay( TEditor.EUCommon.GetScreenPoint(sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPoint = ray.GetPoint(UGeometryIntersect.RayPlaneDistance(plane, ray));
            m_HitPointCS = (transform.InverseTransformPoint(hitPoint) / m_CellRadius).ToCoord();
            m_HitAxialCS = m_HitPointCS.ToCube();
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                        ValidateArea(m_HitAxialCS,true);
                        break;
                    case 1:
                        ValidateArea(m_HitAxialCS,false);
                        break;
                }


            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R:
                        Clear();
                        break;
                    case KeyCode.F1:
                        m_Flat = !m_Flat;
                        break;
                    case KeyCode.F2:
                        m_AxisVisualize = m_AxisVisualize.Next();
                        break;
                }
            }

            DrawSceneHandles();
        }

        void DrawSceneHandles()
        {
            Handles.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * m_CellRadius);
            foreach (var hex in m_Areas.Values)
                Handles.Label(hex.centerCS.ToCoord().ToPosition(), $"A:{hex.coord}\nC:{hex.centerCS}",
                    GUIHelper.m_AreaStyle);
            var area = UHexagonArea.GetBelongingArea(m_HitAxialCS);
            Handles.Label(m_HitPointCS.ToPosition(),
                $"Cell:{m_HitAxialCS}\nArea:{area.coord}\nAPos{area.TransformCSToAS(m_HitAxialCS)}",
                GUIHelper.m_HitStyle);
        }

        void DrawAreas()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * m_CellRadius);
            Gizmos.color = Color.grey;
            
            foreach (var area in m_Areas)
            foreach (var coordsCS in UHexagonArea.IterateAllCoordsCS(area.Value))
                coordsCS.DrawHexagon();
            
            Gizmos.color = Color.white;
            foreach (var area in m_Areas)
            foreach (var coordsCS in UHexagonArea.IterateAllCoordsCSRinged(area.Value))
                coordsCS.coord.DrawHexagon();

            Gizmos.color = Color.cyan;
                foreach (var area in m_Areas.Values)
                    area.centerCS.DrawHexagon();
        }

        void DrawAxis()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * m_CellRadius) *
                            Matrix4x4.Translate(m_HitAxialCS.ToCoord().ToPosition());
            switch (m_AxisVisualize)
            {
                default: return;
                case EAxisVisualize.Axial:
                {
                    Gizmos.color = GUIHelper.C_AxialColumn;
                    Gizmos.DrawRay(Vector3.zero, new HexCoord(1, 0).ToCoord().ToPosition());
                    Gizmos.color = GUIHelper.C_AxialRow;
                    Gizmos.DrawRay(Vector3.zero, new HexCoord(0, 1).ToCoord().ToPosition());
                }
                    break;
                case EAxisVisualize.Cube:
                {
                    Gizmos.color = GUIHelper.C_CubeX;
                    Gizmos.DrawRay(Vector3.zero, new HexCoord(1, 0).ToCoord().ToPosition());
                    Gizmos.color = GUIHelper.C_CubeY;
                    Gizmos.DrawRay(Vector3.zero, new HexCoord(1, -1).ToCoord().ToPosition());
                    Gizmos.color = GUIHelper.C_CubeZ;
                    Gizmos.DrawRay(Vector3.zero, new HexCoord(0, 1).ToCoord().ToPosition());
                }
                    break;
            }
        }

        public EGridAxialTest m_Test = EGridAxialTest.AxialAxis;

        [MFoldout(nameof(m_Test), EGridAxialTest.Range, EGridAxialTest.Intersect, EGridAxialTest.Distance,
            EGridAxialTest.Ring)]
        [Range(1, 5)]
        public int m_Radius1;

        [MFoldout(nameof(m_Test), EGridAxialTest.Intersect, EGridAxialTest.Distance)]
        public HexCoord m_TestAxialPoint = new HexCoord(2, 1,-1);

        [MFoldout(nameof(m_Test), EGridAxialTest.Intersect)]
        public int m_Radius2;

        [MFoldout(nameof(m_Test), EGridAxialTest.Reflect)]
        public ECubicAxis m_ReflectAxis = ECubicAxis.X;

        public enum EGridAxialTest
        {
            Hit,
            AxialAxis,
            Range,
            Intersect,
            Distance,
            Nearby,
            Reflect,
            Ring,
        }

        void DrawTestGrids(Coord hitPixel, HexCoord hitAxial)
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * m_CellRadius);
            Gizmos.DrawRay(hitPixel.ToPosition(), Vector3.up);
            switch (m_Test)
            {
                case EGridAxialTest.Hit:
                {
                    Gizmos.color = Color.yellow;
                    hitAxial.DrawHexagon();
                }
                    break;
                case EGridAxialTest.AxialAxis:
                {
                    Gizmos.color = Color.green;

                    var colPixel = hitPixel.SetCol(0);
                    var colAxis = colPixel.ToCube();
                    var rowPixel = hitPixel.SetRow(0);
                    var rowAxis = rowPixel.ToCube();
                    Gizmos.color = GUIHelper.C_AxialColumn;
                    Gizmos.DrawRay(colPixel.ToPosition(), Vector3.up);
                    Gizmos.DrawLine(Vector3.zero, colPixel.ToPosition());
                    Gizmos.DrawLine(colPixel.ToPosition(), hitPixel.ToPosition());
                    colAxis.DrawHexagon();
                    Gizmos.color = GUIHelper.C_AxialRow;
                    Gizmos.DrawRay(rowPixel.ToPosition(), Vector3.up);
                    Gizmos.DrawLine(Vector3.zero, rowPixel.ToPosition());
                    rowAxis.DrawHexagon();
                    Gizmos.DrawLine(rowPixel.ToPosition(), hitPixel.ToPosition());
                }
                    break;
                case EGridAxialTest.Range:
                {
                    Gizmos.color = Color.yellow;
                    foreach (HexCoord axialPoint in hitAxial.GetCoordsInRadius(m_Radius1))
                        axialPoint.DrawHexagon();
                }
                    break;
                case EGridAxialTest.Intersect:
                {
                    foreach (HexCoord axialPoint in hitAxial.GetCoordsInRadius(m_Radius1)
                        .Extend(m_TestAxialPoint.GetCoordsInRadius(m_Radius2)))
                    {
                        var offset1 = m_TestAxialPoint - axialPoint;
                        var offset2 = hitAxial - axialPoint;
                        bool inRange1 = offset1.InRange(m_Radius2);
                        bool inRange2 = offset2.InRange(m_Radius1);
                        if (inRange1 && inRange2)
                            Gizmos.color = Color.cyan;
                        else if (inRange1)
                            Gizmos.color = Color.green;
                        else if (inRange2)
                            Gizmos.color = Color.blue;
                        else
                            continue;

                        axialPoint.DrawHexagon();
                    }
                }
                    break;
                case EGridAxialTest.Distance:
                {
                    foreach (HexCoord axialPoint in m_TestAxialPoint.GetCoordsInRadius(m_Radius1))
                    {
                        int offset = m_TestAxialPoint.Distance(axialPoint);
                        Gizmos.color = Color.Lerp(Color.green, Color.yellow, ((float) offset) / m_Radius1);
                        axialPoint.DrawHexagon();
                    }
                }
                    break;
                case EGridAxialTest.Nearby:
                {
                    foreach (var nearbyAxial in hitAxial.GetCoordsNearby().LoopIndex())
                    {
                        Gizmos.color = Color.Lerp(Color.blue, Color.red, nearbyAxial.index / 6f);
                        nearbyAxial.value.DrawHexagon();
                    }
                }
                    break;
                case EGridAxialTest.Reflect:
                {
                    var reflectCube = hitAxial.Reflect(m_ReflectAxis);
                    Gizmos.color = Color.yellow;
                    hitAxial.DrawHexagon();
                    Gizmos.color = Color.green;
                    reflectCube.DrawHexagon();
                    Gizmos.color = Color.blue;
                    (-hitAxial).DrawHexagon();
                    Gizmos.color = Color.red;
                    (-reflectCube).DrawHexagon();
                }
                    break;
                case EGridAxialTest.Ring:
                {
                    foreach (var cubeCS in m_HitAxialCS.GetCoordsRinged(m_Radius1))
                    {
                        Gizmos.color = Color.Lerp(Color.white, Color.yellow, cubeCS.dir / 5f);
                        cubeCS.coord.DrawHexagon();
                    }
                }
                break;
            }
        }
        #endif
    }
    
    
    public static class GridHelper
    {
        public static Vector3 ToWorld(this HexCoord _hexCube)
        {
            return _hexCube.ToCoord().ToPosition();
        }
#if UNITY_EDITOR
        public static void DrawHexagon(this HexCoord _coord)
        {
            Vector3[] hexagonList = UHexagon.GetHexagonPoints().Select(p=>p.ToPosition() + _coord.ToWorld()).ToArray();
            Gizmos_Extend.DrawLinesConcat(hexagonList);
        }
#endif
    }
    
}