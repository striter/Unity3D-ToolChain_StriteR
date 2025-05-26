using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using UnityEngine;

namespace Runtime
{
    public class DiskRenderer : ARendererBase
    {
        [Min(0.01f)] public float m_Radius = 1f;
        public RangeFloat m_AngleRange = new RangeFloat(0f,180f);
        [MinMaxRange(0f,1f)] public RangeFloat m_DiskTrim = new RangeFloat(0f,1f);
        [Title]public EUVMode m_UVMode = EUVMode.Repeat;
        [Foldout(nameof(m_UVMode), EUVMode.Repeat), Range(0, 180f)] public float m_DegreePerRepeat = 60f;
        [Title]public bool m_EdgeLine = true;
        [Foldout(nameof(m_EdgeLine),true),Range(0.01f,1f)] public float m_EdgeLineWidth = 0.1f;
        [Foldout(nameof(m_EdgeLine),true)] public bool m_LineTrim = false;
        public EResolution m_Resolution = EResolution._64;
        
        public enum EResolution
        {
            _4 = 4,
            _8 = 8,
            _16 = 16,
            _32 = 32,
            _64 = 64,
            _128 = 128,
            // _256 = 256,
            // _512 = 512,
            // _1024 = 1024,
        }

        public enum EUVMode
        {
            Repeat,
            Normalized,
            PerFrame,
            Quad,
        }
        
        private static GCoordinates kCoordinates = GCoordinates.kDefault;
        protected override void PopulateMesh(Mesh _mesh, Transform _viewTransform)
        {
            PoolList<Vector3>.ISpawn(out var vertices);
            PoolList<Vector3>.ISpawn(out var normals);
            PoolList<Vector2>.ISpawn(out var uvs);
            PoolList<int>.ISpawn(out var indexes);
            List<int> extraIndexes = null;

            var upVector = kCoordinates.up;
            var resolution = (float)m_Resolution;
            var forward = kCoordinates.forward.rotateCW(kCoordinates.up, kmath.kPI2 * (m_AngleRange.start / 360f));
            for (var i = 0; i < resolution; i++)
            {
                var curX = i / resolution;
                var nextX = (i+1) / resolution;
                var curDegree = curX * m_AngleRange.length;
                var nextDegree =  nextX * m_AngleRange.length;
                var curLine = new GLine(kCoordinates.origin,kCoordinates.origin + m_Radius * forward.rotateCW(kCoordinates.up,curDegree * kmath.kDeg2Rad)).Trim(m_DiskTrim);
                var nextLine = new GLine(kCoordinates.origin,kCoordinates.origin + m_Radius *forward.rotateCW(kCoordinates.up, nextDegree * kmath.kDeg2Rad)).Trim(m_DiskTrim);

                var indexOffset = vertices.Count;
                vertices.Add(curLine.start);
                vertices.Add(curLine.end);
                vertices.Add(nextLine.end);
                vertices.Add(nextLine.start);

                normals.Add(upVector);
                normals.Add(upVector);
                normals.Add(upVector);
                normals.Add(upVector);

                switch (m_UVMode)
                {
                    case EUVMode.Normalized:
                    {
                        uvs.Add(new Vector2(curX, 0f));
                        uvs.Add(new Vector2(curX, 1f));
                        uvs.Add(new Vector2(nextX, 1f));
                        uvs.Add(new Vector2(nextX, 0f));
                    }
                        break;
                    case EUVMode.Repeat:
                    {
                        var repeat = m_DegreePerRepeat;
                        var curRepeat = curDegree / repeat;
                        var nextRepeat = nextDegree / repeat;
                        uvs.Add(new Vector2(curRepeat, 0f));
                        uvs.Add(new Vector2(curRepeat, 1f));
                        uvs.Add(new Vector2(nextRepeat, 1f));
                        uvs.Add(new Vector2(nextRepeat, 0f));
                    }
                        break;
                    case EUVMode.PerFrame:
                    {
                        uvs.Add(G2Quad.kDefaultUV[0]);
                        uvs.Add(G2Quad.kDefaultUV[1]);
                        uvs.Add(G2Quad.kDefaultUV[2]);
                        uvs.Add(G2Quad.kDefaultUV[3]);
                    }
                        break;
                    case EUVMode.Quad:
                    {
                        var boundingBox = GBox.kOne * m_Radius;
                        uvs.Add(boundingBox.GetUVW(curLine.start).xz);
                        uvs.Add(boundingBox.GetUVW(curLine.end).xz);
                        uvs.Add(boundingBox.GetUVW(nextLine.end).xz);
                        uvs.Add(boundingBox.GetUVW(nextLine.start).xz);
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                indexes.Add(indexOffset + 0);
                indexes.Add(indexOffset + 1);
                indexes.Add(indexOffset + 2);
                indexes.Add(indexOffset + 2);
                indexes.Add(indexOffset + 3);
                indexes.Add(indexOffset + 0);
            }

            if (m_EdgeLine)
            {
                var curLine = new GLine(kCoordinates.origin,kCoordinates.origin + forward * m_Radius);
                var nextLine = new GLine(kCoordinates.origin,kCoordinates.origin + m_Radius *forward.rotateCW(kCoordinates.up, kmath.kPI2 * (m_AngleRange.length / 360f)));
                var lineTrim = m_DiskTrim;
                if (!m_LineTrim)
                    lineTrim = RangeFloat.Minmax(0f,m_DiskTrim.end);
                curLine = curLine.Trim(lineTrim);
                nextLine = nextLine.Trim(lineTrim);
                
                PoolList<int>.ISpawn(out extraIndexes);
                curLine.PopulateVertex(m_EdgeLineWidth,kCoordinates.up,vertices, extraIndexes, uvs, normals);
                nextLine.PopulateVertex(m_EdgeLineWidth,kCoordinates.up,vertices, extraIndexes, uvs, normals);
            }
            
            _mesh.subMeshCount = extraIndexes != null ? 2 : 1;
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
            
            if (extraIndexes != null)
            {
                _mesh.SetIndices(extraIndexes, MeshTopology.Triangles, 1);
                PoolList<int>.IDespawn(extraIndexes);
            }
            
            PoolList<Vector3>.IDespawn(vertices);
            PoolList<Vector3>.IDespawn(normals);
            PoolList<Vector2>.IDespawn(uvs);
            PoolList<int>.IDespawn(indexes);
        }

        public override void DrawGizmos(Transform _viewTransform)
        {
            base.DrawGizmos(_viewTransform);
            Gizmos.matrix = transform.localToWorldMatrix;
            kCoordinates.DrawGizmos();
            Gizmos.color = Color.white.SetA(.3f);
            new GDisk(kCoordinates.origin,kCoordinates.up,m_Radius).DrawGizmos();

            var resolution = (float)m_Resolution;
            var forward = kCoordinates.forward.rotateCW(kCoordinates.up, kmath.kPI2 * (m_AngleRange.start / 360f)) * m_Radius;
            for (var i = 0; i <= resolution; i++)
            {
                var curX = (i / resolution);
                var diskEdge = new GLine(kCoordinates.origin,kCoordinates.origin + (m_Radius * forward.rotateCW(kCoordinates.up, kmath.kPI2 * curX * (m_AngleRange.length / 360f)))).Trim(m_DiskTrim);
                diskEdge.DrawGizmos();
            }
        }

        // [InspectorButton(true)]
        // public void SemiCircle()
        // {
        //     m_AngleRange = new RangeFloat(-90f, 180f);
        //     m_DiskTrim = RangeFloat.k01;
        // }
    }
}