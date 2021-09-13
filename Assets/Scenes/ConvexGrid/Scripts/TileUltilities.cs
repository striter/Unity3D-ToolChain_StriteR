using System.Collections.Generic;
using Geometry.Voxel;
using LinqExtentions;
using Procedural;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    public enum EGridQuadGeometry
    {
        Full,
        Half,
    }

    public enum EGridVoxelGeometry
    {
        Plane,
        VoxelTight,
        VoxelTopBottom,
    }

    public static class ConvexGridHelper
    {
        public static readonly Vector3 m_TileHeight = 4f * Vector3.up;
        public static readonly Vector3 m_TileHeightHalf = m_TileHeight / 2f;

        public static int m_SmoothTimes = 256;
        public static float m_SmoothFactor = 0.4f;
        static Matrix4x4 TransformMatrix = Matrix4x4.identity;
        static Matrix4x4 InvTransformMatrix = Matrix4x4.identity;

        public static void InitMatrix(Transform _transform, float _scale)
        {
            TransformMatrix = _transform.localToWorldMatrix * Matrix4x4.Scale(_scale * Vector3.one);
            InvTransformMatrix = _transform.worldToLocalMatrix * Matrix4x4.Scale(Vector3.one / _scale);
        }

        public static void InitRelax(int _smoothTimes, float _smoothFactor)
        {
            m_SmoothTimes = _smoothTimes;
            m_SmoothFactor = _smoothFactor;
        }

        public static Vector3 ToPosition(this Coord _pixel)
        {
            return TransformMatrix * new Vector3(_pixel.x, 0, _pixel.y);
        }

        public static Coord ToCoord(this Vector3 _world)
        {
            var coord = InvTransformMatrix * _world;
            return new Coord(coord.x, coord.z);
        }

        public static Vector3 ToPosition(this HexCoord _hexCube)
        {
            return _hexCube.ToPixel().ToPosition();
        }

        public static Vector3 GetVoxelHeight(PileID _id)
        {
            return  m_TileHeight * _id.height;
        }

        public static Vector3 GetCornerHeight(PileID _id)
        {
            return GetCornerHeight( _id.height);
        }

        public static Vector3 GetCornerHeight(byte _height)
        {
            return m_TileHeightHalf+m_TileHeight * _height;
        }

        public static GQuad ConstructLocalGeometry(this ConvexQuad _quad,Coord _center, int[] indexes,EGridQuadGeometry _geometry)
        {
            var cdOS0 = _quad.m_CoordQuad[indexes[0]] - _center;
            var cdOS1 = _quad.m_CoordQuad[indexes[1]] - _center;
            var cdOS2 = _quad.m_CoordQuad[indexes[2]] - _center;
            var cdOS3 = _quad.m_CoordQuad[indexes[3]] - _center;

            if (_geometry == EGridQuadGeometry.Half)
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

        public static void ConstructLocalMesh(this ConvexVertex _vertex, Mesh _mesh, EGridQuadGeometry _geometry,
            EGridVoxelGeometry _volumeGeometry, out Vector3 _centerWS,bool generateUV,bool generateNormals)
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
                case EGridVoxelGeometry.Plane:
                {
                    totalVertex = totalQuad * 4;
                    totalIndex=totalQuad * 4;
                }
                break;
                case EGridVoxelGeometry.VoxelTight:
                {
                    totalVertex = totalQuad * 8;
                    totalIndex = totalQuad * 16;
                }
                break;
                case EGridVoxelGeometry.VoxelTopBottom:
                {
                    totalVertex = totalQuad * 16;
                    totalIndex = totalQuad * 16;
                }
                break;
            }


            List<Vector3> vertices = TSPoolList<Vector3>.Spawn(totalVertex);
            List<int> indices = TSPoolList<int>.Spawn(totalIndex);
            List<Vector3> normals = TSPoolList<Vector3>.Spawn(totalVertex);
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn(totalVertex);

            switch (_volumeGeometry)
            {
                case EGridVoxelGeometry.Plane:
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
                case EGridVoxelGeometry.VoxelTight:
                {
                    foreach (var quad in cornerQuads)
                    {                
                        int indexOffset = vertices.Count;
                        var qube = quad.ConvertToQube(m_TileHeight,.5f);
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
                case EGridVoxelGeometry.VoxelTopBottom:
                {
                    foreach (var quad in cornerQuads)
                    {
                        var qube = quad.ConvertToQube(m_TileHeight,.5f);
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