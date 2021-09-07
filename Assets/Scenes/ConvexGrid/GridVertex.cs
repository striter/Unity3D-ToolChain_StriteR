using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using LinqExtentions;
using ObjectPool;
using ObjectPoolStatic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Tile;
using UnityEngine;

namespace ConvexGrid
{
    public class GridVertex : PoolBehaviour<HexCoord>
    {
        public ConvexVertex m_Vertex { get; private set; }
        public Mesh m_CornerMesh { get; private set; }
        public readonly List<(Vector3 position,HexCoord _vertex)> m_RelativeCornerDirections=new List<(Vector3 position, HexCoord _vertex)>();
        public override void OnPoolInit(Action<HexCoord> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_CornerMesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave};
        }

        public void Init(ConvexVertex _vertex)
        {
            m_Vertex = _vertex;
            transform.position = m_Vertex.m_Coord.ToPosition();

            m_CornerMesh.name = m_Vertex.m_Hex.ToString();
            int quadCount = m_Vertex.m_NearbyQuads.Count;
            var vertexCount = quadCount * 4;
            var vertices = TSPoolList<Vector3>.Spawn(vertexCount);
            var normals = TSPoolList<Vector3>.Spawn(vertexCount);
            var uvs = TSPoolList<Vector2>.Spawn(vertexCount);
            var indices = TSPoolList<int>.Spawn(quadCount*24);
            var indexes = TSPoolList<int>.Spawn(8);
            var cornerQuads = TSPoolList<GQuad>.Spawn();
            foreach (var tuple in m_Vertex.m_NearbyQuads.LoopIndex())
            {
                var quad = tuple.value;
                var offset = m_Vertex.m_NearbyQuadsStartIndex[tuple.index];
                var index0 = offset;
                var index1 = (offset + 1) % 4;
                var index2 = (offset + 2) % 4;
                var index3 = (offset + 3) % 4;
                
                var cdOS0 = quad.m_CoordQuad[index0]-m_Vertex.m_Coord;
                var cdOS1 = quad.m_CoordQuad[index1]-m_Vertex.m_Coord;
                //var v2 = quad.m_CoordQuad[index2]-m_Vertex.m_Coord;
                var cdOS0123 = quad.m_CoordCenter - m_Vertex.m_Coord;
                var cdOS3 = quad.m_CoordQuad[index3]-m_Vertex.m_Coord;

                var posOS0 = cdOS0.ToPosition();
                var posOS1 = ((cdOS0 + cdOS1) / 2).ToPosition();
                var posOS2 = cdOS0123.ToPosition();
                var posOS3 = ((cdOS3 + cdOS0) / 2).ToPosition();
                cornerQuads.Add(new GQuad(posOS0,posOS1,posOS2,posOS3));
                m_RelativeCornerDirections.Add((posOS1.normalized,quad.m_HexQuad[index1]));
            }
            
            foreach (var quad in cornerQuads)
            {
                var b0 = quad.Vertex0;
                var b1 = quad.Vertex1;
                var b2 = quad.Vertex2;
                var b3 = quad.Vertex3;
                var t0 = b0 + ConvexGridHelper.m_TileHeight;
                var t1 = b1 + ConvexGridHelper.m_TileHeight;
                var t2 = b2 + ConvexGridHelper.m_TileHeight;
                var t3 = b3 + ConvexGridHelper.m_TileHeight;
                
                int startIndex = vertices.Count;
                indexes.Clear();
                indexes.Add(startIndex);
                indexes.Add(startIndex+1);
                indexes.Add(startIndex+2);
                indexes.Add(startIndex+3);
                
                indexes.Add(startIndex+4);
                indexes.Add(startIndex+5);
                indexes.Add(startIndex+6);
                indexes.Add(startIndex+7);
                
                vertices.Add(t0);
                vertices.Add(t1);
                vertices.Add(t2);
                vertices.Add(t3);
                
                vertices.Add(b0);
                vertices.Add(b1);
                vertices.Add(b2);
                vertices.Add(b3);
                
                var normal = Vector3.Cross(b1, b2);
                for (int i = 0; i < 8; i++)
                {
                    normals.Add(normal);
                    uvs.Add(URender.IndexToQuadUV(i%4));
                }

                //Top 0123
                indices.Add(indexes[0]);
                indices.Add(indexes[1]);
                indices.Add(indexes[2]);
                indices.Add(indexes[3]);
                
                //Bottom 4765
                indices.Add(indexes[4]);
                indices.Add(indexes[7]);
                indices.Add(indexes[6]);
                indices.Add(indexes[5]);
                
                //Forward Left 2156
                indices.Add(indexes[2]);
                indices.Add(indexes[1]);
                indices.Add(indexes[5]);
                indices.Add(indexes[6]);
                
                //Forward Right 3267
                indices.Add(indexes[3]);
                indices.Add(indexes[2]);
                indices.Add(indexes[6]);
                indices.Add(indexes[7]);
            }
            m_CornerMesh.SetVertices(vertices);
            m_CornerMesh.SetUVs(0,uvs);
            m_CornerMesh.SetNormals(normals);
            m_CornerMesh.SetIndices(indices,MeshTopology.Quads,0,false);
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<int>.Recycle(indices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<GQuad>.Recycle(cornerQuads);
        }

        
        public override void OnPoolRecycle()
        {
            m_Vertex = null;
            m_CornerMesh.Clear();
        }
    }
}