using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using LinqExtentions;
using Procedural;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    public static class UConvexGrid
    {
        
        public static Vector3 ToPosition(this HexCoord _hexCube)
        {
            return _hexCube.ToCoord().ToPosition();
        }

        public static GQuad ConstructLocalGeometry(this ConvexQuad _quad,Coord _center, int[] indexes,ETileQuadGeometry _geometry)
        {
            var cdOS0 = _quad.m_CoordQuad[indexes[0]] - _center;
            var cdOS1 = _quad.m_CoordQuad[indexes[1]] - _center;
            var cdOS2 = _quad.m_CoordQuad[indexes[2]] - _center;
            var cdOS3 = _quad.m_CoordQuad[indexes[3]] - _center;

            if (_geometry == ETileQuadGeometry.Half)
            {
                var cdOS0123 = _quad.m_CoordCenter - _center;
                var posOS0 = cdOS0.ToPosition();
                var posOS1 = ((cdOS0 + cdOS1) / 2).ToPosition();
                var posOS2 = cdOS0123.ToPosition();
                var posOS3 = ((cdOS3 + cdOS0) / 2).ToPosition();
                return new GQuad(posOS0, posOS1, posOS2, posOS3);
            }
            return new GQuad(cdOS0.ToPosition(), cdOS1.ToPosition(), cdOS2.ToPosition(), cdOS3.ToPosition());
        }

        public static void ConstructLocalMesh(this ConvexVertex _vertex, Mesh _mesh, ETileQuadGeometry _geometry,
            ETileVoxelGeometry _volumeGeometry, out Vector3 _centerWS,bool generateUV,bool generateNormals)
        {
            var cornerQuads = TSPoolList<GQuad>.Spawn();
            Coord center = _vertex.m_Coord;
            _centerWS = center.ToPosition();
            foreach (var tuple in _vertex.m_NearbyQuads.LoopIndex())
                cornerQuads.Add(ConstructLocalGeometry(tuple.value,center,_vertex.GetQuadVertsCW(tuple.index),_geometry));
            
            _mesh.Clear();
            int totalQuad = cornerQuads.Count;
            var totalVertex =0;
            var totalIndex = 0;
            switch (_volumeGeometry)
            {
                case ETileVoxelGeometry.Plane:
                {
                    totalVertex = totalQuad * 4;
                    totalIndex=totalQuad * 4;
                }
                break;
                case ETileVoxelGeometry.VoxelTight:
                {
                    totalVertex = totalQuad * 8;
                    totalIndex = totalQuad * 16;
                }
                break;
                case ETileVoxelGeometry.VoxelTopBottom:
                {
                    totalVertex = totalQuad * 16;
                    totalIndex = totalQuad * 16;
                }
                break;
            }


            List<Vector3> vertices = TSPoolList<Vector3>.Spawn();
            List<int> indices = TSPoolList<int>.Spawn();
            List<Vector3> normals = TSPoolList<Vector3>.Spawn();
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn();

            switch (_volumeGeometry)
            {
                case ETileVoxelGeometry.Plane:
                {
                    foreach (var quad in cornerQuads)
                    {
                        int indexOffset = vertices.Count;
                        for (int i = 0; i < 4; i++)
                        {
                            vertices.Add(quad[i]);
                            if (generateUV) uvs.Add(URender.IndexToQuadUV(i));
                            if (generateNormals) uvs.Add(quad.normal);
                        }
                        
                        indices.Add(indexOffset);
                        indices.Add(indexOffset + 1);
                        indices.Add(indexOffset + 2);
                        indices.Add(indexOffset + 3);
                    }
                }
                break;
                case ETileVoxelGeometry.VoxelTight:
                {
                    foreach (var quad in cornerQuads)
                    {                
                        int indexOffset = vertices.Count;
                        var qube = quad.ExpandToQube(KConvexGrid.tileHeightVector,.5f);
                        for (int i = 0; i < 8; i++)
                        {
                            vertices.Add(qube[i]);
                            if(generateUV) uvs.Add(URender.IndexToQuadUV(i%4));
                            if (generateNormals) normals.Add(quad.normal);
                        };
                        
                        //Bottom
                        indices.Add(indexOffset+0);
                        indices.Add(indexOffset+3);
                        indices.Add(indexOffset+2);
                        indices.Add(indexOffset+1);

                        //Top
                        indices.Add(indexOffset+4);
                        indices.Add(indexOffset+5);
                        indices.Add(indexOffset+6);
                        indices.Add(indexOffset+7);

                        //Forward Left
                        indices.Add(indexOffset+1);
                        indices.Add(indexOffset+2);
                        indices.Add(indexOffset+6);
                        indices.Add(indexOffset+5);

                        //Forward Right
                        indices.Add(indexOffset+2);
                        indices.Add(indexOffset+3);
                        indices.Add(indexOffset+7);
                        indices.Add(indexOffset+6);
                    }
                }
                break;
                case ETileVoxelGeometry.VoxelTopBottom:
                {
                    foreach (var quad in cornerQuads)
                    {
                        var qube = quad.ExpandToQube(KConvexGrid.tileHeightVector,.5f);
                        qube.FillFacingQuad(ECubeFacing.T,vertices,indices,generateUV?uvs:null,generateNormals?normals:null);
                        qube.FillFacingQuad(ECubeFacing.D,vertices,indices,generateUV?uvs:null,generateNormals?normals:null);
                    }
                }
                break;
            }
            TSPoolList<GQuad>.Recycle(cornerQuads);
            
            _mesh.SetVertices(vertices);
            TSPoolList<Vector3>.Recycle(vertices);
            
            _mesh.SetIndices(indices, MeshTopology.Quads, 0, false);
            TSPoolList<int>.Recycle(indices);
            
            if (generateUV)
            {
                _mesh.SetUVs(0, uvs);
                TSPoolList<Vector2>.Recycle(uvs);
            }

            if (generateNormals)
            {
                _mesh.SetNormals(normals);
                TSPoolList<Vector3>.Recycle(normals);
            }
        }
    }
}