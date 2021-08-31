using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Two;
using GridTest;
using ObjectPoolStatic;
using UnityEngine;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using Random = System.Random;

namespace Procedural.Hexagon.ConvexGrid
{
    enum EAreaState
    {
        Empty=0,
        Tesselation=1,
        Relaxed=2,
        Meshed=3,
    }
    class Vertex
    {
        public Coord m_Coord;
        public readonly List<Quad> m_RelativeQuads=new List<Quad>(6);
    }
    
    class AreaContainer
    {
        public EAreaState m_State { get; private set; }
        public HexagonArea m_Area { get; private set; }
        public Random m_Random { get; private set; }
        public Mesh m_Mesh { get; private set; }
        public readonly List<Quad> m_Quads = new List<Quad>();
        public readonly List<PHexCube> m_ProceduralVertices = new List<PHexCube>(); 
        public readonly List<HexTriangle> m_ProceduralTriangles = new List<HexTriangle>(); 
        public readonly List<HexQuad> m_ProceduralQuads = new List<HexQuad>();
        public AreaContainer(HexagonArea _area)
        {
            m_State = EAreaState.Empty;
            m_Area = _area;
            m_Mesh=new Mesh{name=_area.ToString(),hideFlags = HideFlags.HideAndDontSave};
            m_Random= new Random(("Test"+m_Area.coord).GetHashCode());
        }
        public IEnumerator SplitQuads()
        {
            if (m_State >= EAreaState.Tesselation)
                yield break;
            m_State = EAreaState.Tesselation;
            
            var random =  m_Random;
                
            //Generate Vertices
            foreach (var tuple in m_Area.IterateAllCoordsCSRinged(false))
            {
                m_ProceduralVertices.TryAdd(tuple.coord);
                yield return null;
            }

            //Generate Triangles
            foreach (var tuple in m_Area.IterateAllCoordsCSRinged(true))
            {
                var radius = tuple.radius;
                var direction = tuple.dir;
                bool first = tuple.first;

                if (radius == UHexagonArea.radius)
                    break;
    
                var startCoordCS = tuple.coord;
                m_ProceduralVertices.TryAdd(startCoordCS);
                if (radius == 0)
                {
                    var nearbyCoords = UHexagonArea.GetNearbyAreaCoordsCS(startCoordCS);
        
                    for (int i = 0; i < 6; i++)
                    {
                        var coord1 = nearbyCoords[i];
                        var coord2 = nearbyCoords[(i + 1) % 6];
                        m_ProceduralVertices.TryAdd(coord1);
                        m_ProceduralVertices.TryAdd(coord2);
                        m_ProceduralTriangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                        yield return null;
                    }
                }
                else
                {
                    int begin = first ? 0:1;
                    for (int i = begin; i < 3; i++)
                    {
                        var coord1=UHexagonArea.GetNearbyAreaCoordCS(startCoordCS,direction+3+i);
                        var coord2=UHexagonArea.GetNearbyAreaCoordCS(startCoordCS,direction+4+i);
                        m_ProceduralVertices.TryAdd(coord1);
                        m_ProceduralVertices.TryAdd(coord2);
                        m_ProceduralTriangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                        yield return null;
                    }
                }
            }
            
            List<HexTriangle> availableTriangles = m_ProceduralTriangles.DeepCopy();
            //Remove Edge Triangles
            foreach (var tuple in m_Area.IterateAreaCoordsCSRinged(m_Area.RadiusAS))
            {
                 if(!tuple.first)
                     continue;
            
                 var triangles = availableTriangles.Collect(p => p.All(k=>m_Area.InRange(k))&&p.Contains(tuple.coord)).ToArray();
                 availableTriangles.RemoveRange(triangles);
            }

            //Combine Center Triangles
            while (availableTriangles.Count > 0)
            {
                int validateIndex = availableTriangles.RandomIndex(random);
                var curTriangle = availableTriangles[validateIndex];
                availableTriangles.RemoveAt(validateIndex);
        
                var relativeTriangleIndex = availableTriangles.FindIndex(p => p.MatchVertexCount( curTriangle)==2&&p.All(p=>m_Area.InRange(p)));
                if(relativeTriangleIndex==-1)
                    continue;
        
                var nextTriangle = availableTriangles[relativeTriangleIndex];
                availableTriangles.RemoveAt(relativeTriangleIndex);

                m_ProceduralQuads.Add(UHexagonGeometry.CombineTriangle(curTriangle, nextTriangle));
                m_ProceduralTriangles.Remove(curTriangle);
                m_ProceduralTriangles.Remove(nextTriangle);
                yield return null;
            }

            //Split Quads
            int quadCount = m_ProceduralQuads.Count;
            while (quadCount-->0)
            {
                var splitQuad = m_ProceduralQuads[0];
                m_ProceduralQuads.RemoveAt(0);
            
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
                
                m_ProceduralVertices.TryAdd(index01);
                m_ProceduralVertices.TryAdd(index12);
                m_ProceduralVertices.TryAdd(index23);
                m_ProceduralVertices.TryAdd(index30);
                m_ProceduralVertices.TryAdd(index0123);
                
                m_ProceduralQuads.Add(new HexQuad(index0,index01,index0123,index30));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index01,index1,index12,index0123));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index30,index0123,index23,index3));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index0123,index12,index2,index23));
                yield return null;
            }
            
            //Split Triangles
            while (m_ProceduralTriangles.Count > 0)
            {
                var splitTriangle = m_ProceduralTriangles[0];
                m_ProceduralTriangles.RemoveAt(0);

                var index0 = splitTriangle.vertex0;
                var index1 = splitTriangle.vertex1;
                var index2 = splitTriangle.vertex2;
            
                var midTuple = splitTriangle.GetTriangleMidVertices();
                var index01 = midTuple.m01;
                var index12 = midTuple.m12;
                var index20 = midTuple.m20;
                var index012 = midTuple.m012;
                
                m_ProceduralVertices.TryAdd(index01);
                m_ProceduralVertices.TryAdd(index12);
                m_ProceduralVertices.TryAdd(index20);
                m_ProceduralVertices.TryAdd(index012);
                
                m_ProceduralQuads.Add(new HexQuad(index0,index01,index012,index20));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index01,index1,index12,index012));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index20,index012,index12,index2));
                yield return null;
            }
        }
        public IEnumerator Relaxed()
        {
            if (m_State >= EAreaState.Relaxed)
                yield break;
            m_State = EAreaState.Relaxed;
            
            m_ProceduralVertices.Clear();
            m_ProceduralTriangles.Clear();
            m_ProceduralQuads.Clear();
        }
        
        public void SetupMesh(List<Quad> _areaQuads,Dictionary<PHexCube,Vertex> _vertices,Matrix4x4 _matrix)
        {
            if (m_State >= EAreaState.Meshed)
                return;
            m_State = EAreaState.Meshed;

            List<Vector3> vertices = TSPoolList<Vector3>.Spawn(); 
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn(); 
            List<int> indices = TSPoolList<int>.Spawn();

            foreach (Quad quad in _areaQuads)
            {
                var hexagonQuad = quad.m_HexQuad;
                int index0 = vertices.Count;
                int index1 = index0 + 1;
                int index2 = index0 + 2;
                int index3 = index0 + 3;
                foreach (var tuple in hexagonQuad.LoopIndex())
                {
                    var index = tuple.index;
                    var coord = tuple.value;
                    var positionOS = _matrix*_vertices[coord].m_Coord.ToWorld();
                    vertices.Add(positionOS);
                    uvs.Add(URender.IndexToQuadUV(index));
                }
                indices.Add(index0);
                indices.Add(index1);
                indices.Add(index3);
                indices.Add(index1);
                indices.Add(index2);
                indices.Add(index3);
            }
            
            m_Mesh.Clear();
            m_Mesh.SetVertices(vertices);
            m_Mesh.SetUVs(0,uvs);
            m_Mesh.SetIndices(indices,MeshTopology.Triangles,0,true);
        }
    }
    class Quad
    {
        public HexQuad m_HexQuad { get; private set; }
        public G2Quad m_GeometryQuad { get; private set; }
        public Quad(HexQuad _hexQuad,Dictionary<PHexCube,Vertex> _vertices)
        {
            m_HexQuad = _hexQuad;
            m_GeometryQuad=new G2Quad(
                _vertices[m_HexQuad.vertex0].m_Coord,
                _vertices[m_HexQuad.vertex1].m_Coord,
                _vertices[m_HexQuad.vertex2].m_Coord,
                _vertices[m_HexQuad.vertex3].m_Coord);
        }
    }
}
