using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObjectPoolStatic;
using UnityEngine;
using UnityEditor;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using TEditor;

namespace GridTest
{
    [ExecuteInEditMode]
    public class GridTest_HexagonGrid : MonoBehaviour
    {
        public bool m_Flat = false;
        public float m_CellRadius = 1;
        public int m_AreaRadius = 8;
        public int m_MaxAreaRadius = 4;
#if UNITY_EDITOR
        [NonSerialized] private readonly List<PHexCube> m_HexAxis=new List<PHexCube>();
        [NonSerialized] private readonly Dictionary<PHexCube,HexagonArea> m_Areas = new Dictionary<PHexCube, HexagonArea>();
        [NonSerialized] private readonly List<HexTriangle> m_Triangles = new List<HexTriangle>();
        [NonSerialized] private readonly List<HexQuad> m_Quads = new List<HexQuad>();
        [NonSerialized] private readonly Dictionary<PHexCube, Vertex> m_Vertices=new Dictionary<PHexCube, Vertex>();

        private readonly Queue<IEnumerator> m_AreaIterate=new Queue<IEnumerator>();
        private readonly Timer m_IterateTimer = new Timer(1f/60f);
        private Coord m_HitPointCS;
        private PHexAxial m_HitAxialCS;

        private void OnValidate() => Clear();

        private void OnEnable()
        {
            EditorApplication.update += Tick;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        void Clear()
        {
            m_AreaIterate.Clear();
            m_Vertices.Clear();
            m_HexAxis.Clear();
            m_Areas.Clear();
            m_Triangles.Clear();
            m_Quads.Clear();
        }

        private void Tick()
        {
            m_IterateTimer.Tick(EditorTime.deltaTime);
            if (m_IterateTimer.m_Timing)
                return;
            m_IterateTimer.Replay();
            if (m_AreaIterate.Count==0)
                return;

            if (!m_AreaIterate.First().MoveNext())
                m_AreaIterate.Dequeue();
        }

        private void OnDrawGizmos()
        {
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6);
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one*m_CellRadius);
            DrawProcedural();

            DrawAdditionalGizmos();
        }

        void ValidateArea(PHexCube _positionCS)
        {
            var area = UHexagonArea.GetBelongingArea(_positionCS);
            
            if (!area.position.InRange(m_MaxAreaRadius))
                return;
            if (m_Areas.ContainsKey(area.position))
                return;
                
            m_Areas.Add(area.position,area);
            m_HexAxis.AddRange(area.GetAllCoordsCS());
            m_AreaIterate.Enqueue(Generate(area));
        }

        IEnumerator Generate(HexagonArea _curArea)
        {
            void AddCoord(PHexCube p) => m_Vertices.TryAdd(p, new Vertex(){position = p.ToPixel()});
            //Generate Vertices
            foreach (HexagonArea area in _curArea.IterateNearbyAreas().Extend(_curArea))
            {
                area.centerCS -= (area.position-_curArea.position);
                foreach (var tuple in area.IterateCoordsCS(false))
                {
                    var coord =  tuple.coord;
                    AddCoord(tuple.coord);
                    yield return null;
                }

                foreach (var tuple in area.IterateCoordsCS(true))
                {
                    var radius = tuple.radius;
                    var direction = tuple.dir;
                    bool first = tuple.first;

                    if (radius == UHexagonArea.radius)
                        break;
                
                    var startCoordCS = tuple.coord;
                    AddCoord(startCoordCS);
                    if (radius == 0)
                    {
                        var nearbyCoords = UHexagonArea.GetCoordsNearbyCS(startCoordCS);
                    
                        for (int i = 0; i < 6; i++)
                        {
                            var coord1 = nearbyCoords[i];
                            var coord2 = nearbyCoords[(i + 1) % 6];
                            AddCoord(coord1);
                            AddCoord(coord2);
                            m_Triangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                            yield return null;
                        }
                    }
                    else
                    {
                        direction += 3;
                        int begin = first ? 0:1;
                        for (int i = begin; i < 3; i++)
                        {
                            var coord1=UHexagonArea.GetCoordsNearby(startCoordCS,direction+i);
                            var coord2=UHexagonArea.GetCoordsNearby(startCoordCS,direction+i+1);
                            AddCoord(coord1);
                            AddCoord(coord2);
                            m_Triangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                            yield return null;
                        }
                    }
                }
            }
            
            //Generate Triangles
            System.Random random = new System.Random("Test".GetHashCode());
            List<HexTriangle> availableTriangles = m_Triangles.DeepCopy();
            int maxValidateCount = 1024;
            while (availableTriangles.Count > 0)
            {
                if (maxValidateCount-- <= 0)
                    throw new Exception("Validate Failed!");
                
                int validateIndex = availableTriangles.RandomIndex(random);
                var curTriangle = availableTriangles[validateIndex];
                availableTriangles.RemoveAt(validateIndex);
            
                var relativeTriangleIndex = availableTriangles.FindIndex(p => p.MatchVertexCount(curTriangle)==2);
                if(relativeTriangleIndex==-1)
                    continue;
            
                var nextTriangle = availableTriangles[relativeTriangleIndex];
                availableTriangles.RemoveAt(relativeTriangleIndex);
                
                m_Quads.Add(UHexagonGeometry.CombineTriangle(curTriangle,nextTriangle));
                m_Triangles.Remove(curTriangle);
                m_Triangles.Remove(nextTriangle);
                yield return null;
            }
            
            //Split Quads
            int quadCount = m_Quads.Count;
            while (quadCount-->0)
            {
                var splitQuad = m_Quads[0];
                m_Quads.RemoveAt(0);
            
                var index0 = splitQuad.vertex0;
                var index1 = splitQuad.vertex1;
                var index2 = splitQuad.vertex2;
                var index3 = splitQuad.vertex3;
                var midTuple = splitQuad.GetQuadMidVertices();
                
                var index01 = midTuple.m01;
                var index12 = midTuple.m12;
                var index23 = midTuple.m23;
                var index30 = midTuple.m30;
                var index0123 = midTuple.m0123;
                
                AddCoord(index01);
                AddCoord(index12);
                AddCoord(index23);
                AddCoord(index30);
                AddCoord(index0123);
                
                m_Quads.Add(new HexQuad(index0,index01,index0123,index30));
                yield return null;
                m_Quads.Add(new HexQuad(index01,index1,index12,index0123));
                yield return null;
                m_Quads.Add(new HexQuad(index0123,index12,index2,index23));
                yield return null;
                m_Quads.Add(new HexQuad(index30,index0123,index23,index3));
                yield return null;
            }
            
            //Split Triangles
            while (m_Triangles.Count > 0)
            {
                var splitTriangle = m_Triangles[0];
                m_Triangles.RemoveAt(0);
                var index0 = splitTriangle.vertex0;
                var index1 = splitTriangle.vertex1;
                var index2 = splitTriangle.vertex2;
            
                var midTuple = splitTriangle.GetTriangleMidVertices();
                var index01 = midTuple.m01;
                var index12 = midTuple.m12;
                var index20 = midTuple.m20;
                var index012 = midTuple.m012;
                
                AddCoord(index01);
                AddCoord(index12);
                AddCoord(index20);
                AddCoord(index012);
                
                m_Quads.Add(new HexQuad(index0,index01,index012,index20));
                yield return null;
                m_Quads.Add(new HexQuad(index1,index12,index012,index01));
                yield return null;
                m_Quads.Add(new HexQuad(index2,index20,index012,index12));
                yield return null;
            }

            //Relaxing
            Dictionary<PHexCube, Coord> directions = new Dictionary<PHexCube, Coord>();
            for (int i = 0; i < 256; i++)
            {
                directions.Clear();
                foreach (var quad in m_Quads)
                {
                    var origins = quad.GetVertices(p=>m_Vertices[p].position);
                    var center = origins.Average((a,b)=>a+b,(a,divide)=>a/divide);
                    var offsets = origins.Select(p => p - center).ToArray();
            
                    var rad01=UMath.GetRadin(offsets[0],offsets[1]);
                    var rad02=UMath.GetRadin(offsets[0],offsets[2]);
                    var rad03=UMath.GetRadin(offsets[0],offsets[3]);
                    
                    
                    Coord direction0 = offsets[0];
                    Coord direction1 = UMath.GetRotateMatrix(-rad01).Multiply( offsets[1]);
                    Coord direction2 = UMath.GetRotateMatrix(-rad02).Multiply( offsets[2]);
                    Coord direction3 = UMath.GetRotateMatrix(-rad03).Multiply( offsets[3]);
                    
                    var average = (direction0 + direction1 + direction2 + direction3) / 4;
                    direction0 = average;
                    direction1 = UMath.GetRotateMatrix(rad01).Multiply( average);
                    direction2 = UMath.GetRotateMatrix(rad02).Multiply( average);
                    direction3 = UMath.GetRotateMatrix(rad03).Multiply( average);
            
                    direction0 -= offsets[0];
                    direction1 -= offsets[1];
                    direction2 -= offsets[2];
                    direction3 -= offsets[3];
                    
                    var vertices = quad.vertices;
                    
                    directions.TryAdd(vertices[0], Coord.zero);
                    directions.TryAdd(vertices[1], Coord.zero);
                    directions.TryAdd(vertices[2], Coord.zero);
                    directions.TryAdd(vertices[3], Coord.zero);
                    
                    directions[vertices[0]] += direction0;
                    directions[vertices[1]] += direction1;
                    directions[vertices[2]] += direction2;
                    directions[vertices[3]] += direction3;
                }
            
                foreach (var pair in directions)
                    m_Vertices[pair.Key].position += pair.Value * .005f;
                yield return null;
            }

            int passIndex = m_Quads.Count;
            int curIndex = 0;
            while (passIndex-- > 0)
            {
                var quad = m_Quads[curIndex];
                if (quad.vertices.Any(p => _curArea.InRange(p)))
                {
                    curIndex++;
                    continue;
                }

                m_Quads.RemoveAt(curIndex);
                yield return null;
            }
        }
        
        void DrawProcedural()
        {
            Gizmos.color = Color.yellow;
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.position.ToWorld(),.2f);
            Gizmos.color = Color.cyan;
            foreach (var area in m_Areas)
                foreach (var coord in UHexagonArea.GetAllCoordsCS(area.Value))
                    if(m_Vertices.ContainsKey(coord))
                        Gizmos.DrawSphere(m_Vertices[coord].position.ToWorld(),.4f);
            Gizmos.color = Color.white.SetAlpha(.2f);
            foreach (var triangle in m_Triangles)
                Gizmos_Extend.DrawLines(triangle.GetVertices(p=>m_Vertices[p].position.ToWorld()));
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLines(quad.GetVertices(p=>m_Vertices[p].position.ToWorld()));
        }
        
        #region DrawGUI
        #region Enums
        public enum EAxisVisualize
        {
            Invalid,
            Axial,
            Cube,
        }

        #endregion
        [Header("Visualize")]
        public EAxisVisualize m_AxisVisualize;

        public bool m_AreaBackground;
        public bool m_AreaCenter;
        public bool m_AreaCoords;
        private static class GUIHelper
        {
            public static readonly Color C_AxialColumn = Color.green;
            public static readonly Color C_AxialRow = Color.blue;
            public static readonly Color C_CubeX = Color.red;
            public static readonly Color C_CubeY= Color.green;
            public static readonly Color C_CubeZ = Color.blue;

            public static readonly GUIStyle m_AreaStyle = new GUIStyle {alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Normal};

            public static readonly GUIStyle m_HitStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Normal };
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay( TEditor.UECommon.GetScreenPoint(sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPoint = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            m_HitPointCS = (transform.InverseTransformPoint(hitPoint)/m_CellRadius) .ToPixel();
            m_HitAxialCS = m_HitPointCS.ToAxial();
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:ValidateArea(m_HitAxialCS); break;
                    case 1: break;
                }
                
                    
            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R:Clear();
                        break;
                    case KeyCode.F1:
                        m_Flat = !m_Flat;
                        break;
                    case KeyCode.F2:
                        m_AxisVisualize = m_AxisVisualize.Next();
                        break;
                    case KeyCode.F5:
                        m_AreaBackground = !m_AreaBackground;
                        break;
                    case KeyCode.F6:
                        m_AreaCenter = !m_AreaCenter;
                        break;
                }
            }

            
            DrawSceneHandles();
        }

        void DrawSceneHandles()
        {
            if (!m_AreaCenter)
                return;
            
            Handles.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one*m_CellRadius);
            foreach (var hex in m_Areas.Values)
                Handles.Label(hex.centerCS.ToAxial().ToPixel().ToWorld(),$"A:{hex.position}\nC:{hex.centerCS}",GUIHelper.m_AreaStyle);
            var area = UHexagonArea.GetBelongingArea(m_HitAxialCS);
            Handles.Label(m_HitPointCS.ToWorld(),$"Cell:{m_HitAxialCS}\nArea:{area.position}\nAPos{area.TransformCSToAS(m_HitAxialCS)}",GUIHelper.m_HitStyle);
        }
        void DrawAdditionalGizmos()
        {
            DrawAxis();
            DrawAreas();
            DrawTestGrids(m_HitPointCS,m_HitAxialCS);
        }
        void DrawAreas()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one*m_CellRadius);
            if (m_AreaCenter)
                foreach (var area in m_Areas.Values)
                {
                    Gizmos.color = Color.cyan;
                    area.centerCS.DrawHexagon( );
                }

            Gizmos.color = Color.yellow;
            if(m_AreaCoords)
                foreach (var area in m_Areas)
                    foreach (var coords in UHexagonArea.IterateCoordsCS(area.Value))
                        Gizmos.DrawSphere(coords.coord.ToWorld(),.3f);
            
            if (m_AreaBackground)
            {
                Gizmos.color = Color.grey;
                foreach (var area in m_Areas.Values)
                {
                    Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * m_CellRadius);
                    Gizmos.matrix *= Matrix4x4.Translate(area.centerCS.ToWorld());
                    Gizmos.matrix *= Matrix4x4.Scale(Vector3.one * UHexagonArea.radius *2* UHexagonArea.tilling);
                    PHexCube.zero.DrawHexagon();
                }
            }
        }

        void DrawAxis()
        {
            Gizmos.matrix = transform.localToWorldMatrix* Matrix4x4.Scale(Vector3.one*m_CellRadius)* Matrix4x4.Translate(m_HitAxialCS.ToPixel().ToWorld());
            switch (m_AxisVisualize)
            {
                default: return;
                case EAxisVisualize.Axial:
                    {
                        Gizmos.color = GUIHelper.C_AxialColumn;
                        Gizmos.DrawRay(Vector3.zero,new PHexAxial(1,0).ToPixel().ToWorld());
                        Gizmos.color = GUIHelper.C_AxialRow;
                        Gizmos.DrawRay(Vector3.zero,new PHexAxial(0,1).ToPixel().ToWorld());
                    }
                    break;
                case EAxisVisualize.Cube:
                    {
                        Gizmos.color = GUIHelper.C_CubeX;
                        Gizmos.DrawRay(Vector3.zero,new PHexAxial(1,0).ToPixel().ToWorld());
                        Gizmos.color = GUIHelper.C_CubeY;
                        Gizmos.DrawRay(Vector3.zero,new PHexAxial(1,-1).ToPixel().ToWorld());
                        Gizmos.color =GUIHelper. C_CubeZ;
                        Gizmos.DrawRay(Vector3.zero,new PHexAxial(0,1).ToPixel().ToWorld());
                    }
                    break;
            }
        }
        
        public EGridAxialTest m_Test =  EGridAxialTest.AxialAxis;
        [MFoldout(nameof(m_Test),EGridAxialTest.Range,EGridAxialTest.Intersect,EGridAxialTest.Distance,EGridAxialTest.Ring)]
        [Range(1,5)]public int m_Radius1;
        [MFoldout(nameof(m_Test),EGridAxialTest.Intersect,EGridAxialTest.Distance)]
        public PHexAxial m_TestAxialPoint=new PHexAxial(2,1);
        [MFoldout(nameof(m_Test),EGridAxialTest.Intersect)]
        public int m_Radius2;
        [MFoldout(nameof(m_Test),EGridAxialTest.Reflect)]
        public ECubeAxis m_ReflectAxis = ECubeAxis.X;
        
        public enum EGridAxialTest
        {
            Hit,
            AxialAxis,
            Range,
            Intersect,
            Distance,
            Nearby,
            Mirror,
            Reflect,
            Ring,
        }
        void DrawTestGrids(Coord hitPixel, PHexAxial hitAxial)
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one*m_CellRadius);
            Gizmos.DrawRay(hitPixel.ToWorld(), Vector3.up);
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
                        var colAxis = colPixel.ToAxial();
                        var rowPixel = hitPixel.SetRow(0);
                        var rowAxis = rowPixel.ToAxial();
                        Gizmos.color = GUIHelper.C_AxialColumn;
                        Gizmos.DrawRay(colPixel.ToWorld(), Vector3.up);
                        Gizmos.DrawLine(Vector3.zero, colPixel.ToWorld());
                        Gizmos.DrawLine(colPixel.ToWorld(), hitPixel.ToWorld());
                        colAxis.DrawHexagon();
                        Gizmos.color =GUIHelper. C_AxialRow;
                        Gizmos.DrawRay(rowPixel.ToWorld(), Vector3.up);
                        Gizmos.DrawLine(Vector3.zero, rowPixel.ToWorld());
                        rowAxis.DrawHexagon();
                        Gizmos.DrawLine(rowPixel.ToWorld(), hitPixel.ToWorld());
                    }
                    break;
                case EGridAxialTest.Range:
                {
                    Gizmos.color = Color.yellow;
                    foreach (PHexAxial axialPoint in hitAxial.GetCoordsInRadius(m_Radius1))
                        axialPoint.DrawHexagon();
                }
                break;
                case EGridAxialTest.Intersect:
                {
                    foreach (PHexAxial axialPoint in hitAxial.GetCoordsInRadius(m_Radius1).Extend(m_TestAxialPoint.GetCoordsInRadius(m_Radius2)))
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
                    foreach (PHexAxial axialPoint in m_TestAxialPoint.GetCoordsInRadius(m_Radius1))
                    {
                        int offset = m_TestAxialPoint.Distance(axialPoint);
                        Gizmos.color = Color.Lerp(Color.green, Color.yellow, ((float) offset) / m_Radius1);
                        axialPoint.DrawHexagon();
                    }
                }
                break;
                case EGridAxialTest.Nearby:
                {
                    foreach (var nearbyAxial in  hitAxial.GetCoordsNearby().LoopIndex())
                    {
                        Gizmos.color = Color.Lerp(Color.blue, Color.red, nearbyAxial.index / 6f);
                        nearbyAxial.value.DrawHexagon();
                    }
                }
                break;
                case EGridAxialTest.Mirror:
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Gizmos.color = Color.Lerp(Color.green, Color.red, ((float) i) / 6);
                        var axialOffset = UHexagon.RotateMirror(m_AreaRadius-1, i).ToAxial();
                        var coords = hitAxial.GetCoordsInRadius(m_AreaRadius);
                        foreach (PHexAxial axialPoint in coords)
                            (axialPoint+axialOffset).DrawHexagon();
                    }
                }
                break;
                case EGridAxialTest.Reflect:
                {
                    var axialHitCube = hitAxial.ToCube();
                    var reflectCube = axialHitCube.Reflect(m_ReflectAxis);
                    Gizmos.color = Color.yellow;
                    hitAxial.DrawHexagon();
                    Gizmos.color = Color.green;
                    reflectCube.ToAxial().DrawHexagon();
                    Gizmos.color = Color.blue;
                    (-axialHitCube).ToAxial().DrawHexagon();
                    Gizmos.color = Color.red;
                    (-reflectCube).ToAxial().DrawHexagon();
                }
                break;
                case EGridAxialTest.Ring:
                {
                    foreach (var cubeCS in m_HitAxialCS.ToCube().GetCoordsRinged(m_Radius1))
                    {
                        Gizmos.color = Color.Lerp(Color.white, Color.yellow, cubeCS.dir / 5f);
                        cubeCS.coord.DrawHexagon();
                    }
                }
                break;
            }
        }
        #endregion
        #endif
    }
    
}