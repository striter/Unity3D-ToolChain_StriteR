using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using LinqExtension;
using Procedural;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace PolyGrid
{
    public static class UPolyGrid
    {
        public static Vector3 ToPosition(this HexCoord _hexCube)
        {
            return _hexCube.ToCoord().ToPosition();
        }

        public static GQuad ConstructLocalGeometry(this PolyQuad _quad,Coord _center, int[] indexes,EQuadGeometry _geometry)
        {
            var cdOS0 = _quad.m_CoordQuad[indexes[0]] - _center;
            var cdOS1 = _quad.m_CoordQuad[indexes[1]] - _center;
            var cdOS2 = _quad.m_CoordQuad[indexes[2]] - _center;
            var cdOS3 = _quad.m_CoordQuad[indexes[3]] - _center;

            if (_geometry == EQuadGeometry.Half)
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

        public static void ConstructLocalMesh(this PolyVertex _vertex, Mesh _mesh, EQuadGeometry _geometry,
            EVoxelGeometry _volumeGeometry,bool generateUV,bool generateNormals,bool generateColors=false,Color _color=default)
        {
            var cornerQuads = TSPoolList<GQuad>.Spawn();
            Coord center = _vertex.m_Coord;
            foreach (var tuple in _vertex.m_NearbyQuads.LoopIndex())
                cornerQuads.Add(ConstructLocalGeometry(tuple.value,center,_vertex.GetQuadVertsCW(tuple.index),_geometry));
            
            _mesh.Clear();
            List<Vector3> vertices = TSPoolList<Vector3>.Spawn();
            List<int> indices = TSPoolList<int>.Spawn();
            List<Vector3> normals =generateNormals? TSPoolList<Vector3>.Spawn():null;
            List<Vector2> uvs = generateUV? TSPoolList<Vector2>.Spawn():null;
            List<Color> colors = generateColors? TSPoolList<Color>.Spawn():null;

            switch (_volumeGeometry)
            {
                case EVoxelGeometry.Plane:
                {
                    foreach (var quad in cornerQuads)
                    {
                        int indexOffset = vertices.Count;
                        for (int i = 0; i < 4; i++)
                        {
                            vertices.Add(quad[i]);
                            uvs?.Add(URender.IndexToQuadUV(i));
                            normals?.Add(quad.normal);
                            colors?.Add(_color);
                        }
                        
                        UPolygon.QuadToTriangleIndices(indices,indexOffset+0,indexOffset+1,indexOffset+2,indexOffset+3);
                    }
                }
                break;
                case EVoxelGeometry.VoxelTight:
                {
                    foreach (var quad in cornerQuads)
                    {                
                        int indexOffset = vertices.Count;
                        var qube = quad.ExpandToQube(KPolyGrid.tileHeightVector,.5f);
                        for (int i = 0; i < 8; i++)
                        {
                            vertices.Add(qube[i]);
                            uvs?.Add(URender.IndexToQuadUV(i%4));
                            normals?.Add(quad.normal);
                            colors?.Add(_color);
                        };
                        
                        //Bottom
                        UPolygon.QuadToTriangleIndices(indices,indexOffset+0,indexOffset+3,indexOffset+2,indexOffset+1);
                        //Top
                        UPolygon.QuadToTriangleIndices(indices,indexOffset+4,indexOffset+5,indexOffset+6,indexOffset+7);
                        //Forward Left
                        UPolygon.QuadToTriangleIndices(indices,indexOffset+1,indexOffset+2,indexOffset+6,indexOffset+5);
                        //Forward Right
                        UPolygon.QuadToTriangleIndices(indices,indexOffset+2,indexOffset+3,indexOffset+7,indexOffset+6);
                    }
                }
                break;
                case EVoxelGeometry.VoxelFull:
                {
                    foreach (var quad in cornerQuads)
                    {
                        var qube = quad.ExpandToQube(KPolyGrid.tileHeightVector, .5f);
                        qube.FillFacingQuadTriangle(ECubeFacing.T,vertices,indices,uvs,normals,colors,_color);
                        qube.FillFacingSplitQuadTriangle(ECubeFacing.LF,vertices,indices,uvs,normals,colors,_color);
                        qube.FillFacingSplitQuadTriangle(ECubeFacing.FR,vertices,indices,uvs,normals,colors,_color);
                        qube.FillFacingQuadTriangle(ECubeFacing.D,vertices,indices,uvs,normals,colors,_color);
                    }

                }
                break;
            }
            TSPoolList<GQuad>.Recycle(cornerQuads);
            
            _mesh.SetVertices(vertices);
            TSPoolList<Vector3>.Recycle(vertices);
            
            _mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            TSPoolList<int>.Recycle(indices);
            
            if (uvs!=null)
            {
                _mesh.SetUVs(0, uvs);
                TSPoolList<Vector2>.Recycle(uvs);
            }

            if (normals!=null)
            {
                _mesh.SetNormals(normals);
                TSPoolList<Vector3>.Recycle(normals);
            }

            if (colors != null)
            {
                _mesh.SetColors(colors);
                TSPoolList<Color>.Recycle(colors);
            }
        }
    }
}