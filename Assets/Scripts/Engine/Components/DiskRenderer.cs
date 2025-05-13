using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Scripting;
using UnityEngine;

namespace Runtime
{
    public class DiskRenderer : ARendererBase
    {
        public GAxis m_Axis = GAxis.kDefault;
        [Min(0.01f)] public float m_Radius = 1f;
        
        [Header("Details")]
        public RangeFloat m_AngleRange = new RangeFloat(0f,180f);
        public EResolution m_Resolution = EResolution._64;
        [MinMaxRange(0f,1f)] public RangeFloat m_DiskTrim = new RangeFloat(0f,1f);
        public EUVMode m_UVMode = EUVMode.Repeat;
        public bool m_EdgeLine = true;
        [Foldout(nameof(m_EdgeLine),true),Range(0.01f,1f)] public float m_EdgeLineWidth = 0.1f;
        
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
        }
        
        protected override void PopulateMesh(Mesh _mesh, Transform _viewTransform)
        {
            ListPool<Vector3>.ISpawn(out var vertices);
            ListPool<Vector3>.ISpawn(out var normals);
            ListPool<Vector2>.ISpawn(out var uvs);
            ListPool<int>.ISpawn(out var indexes);
            List<int> indexes1 = null;

            var upVector = m_Axis.up;
            var resolution = (float)m_Resolution;
            var forward = m_Axis.forward.rotateCW(m_Axis.up, kmath.kPI2 * (m_AngleRange.start / 360f));
            for (var i = 0; i < resolution; i++)
            {
                var curX = (i / resolution);
                var nextX = ((i+1) / resolution);
                var curLine = new GLine(m_Axis.origin,m_Axis.origin + (m_Radius * forward.rotateCW(m_Axis.up, kmath.kPI2 * curX * (m_AngleRange.length / 360f)))).Trim(m_DiskTrim);
                var nextLine = new GLine(m_Axis.origin,m_Axis.origin + (m_Radius *forward.rotateCW(m_Axis.up, kmath.kPI2 * nextX * (m_AngleRange.length / 360f)))).Trim(m_DiskTrim);

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
                        uvs.Add(G2Quad.kDefaultUV[0]);
                        uvs.Add(G2Quad.kDefaultUV[1]);
                        uvs.Add(G2Quad.kDefaultUV[2]);
                        uvs.Add(G2Quad.kDefaultUV[3]);
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
                var curLine = new GLine(m_Axis.origin,m_Axis.origin + forward * m_Radius).Trim(m_DiskTrim);
                var nextLine = new GLine(m_Axis.origin,m_Axis.origin + m_Radius *forward.rotateCW(m_Axis.up, kmath.kPI2 * (m_AngleRange.length / 360f))).Trim(m_DiskTrim);
                
                ListPool<int>.ISpawn(out indexes1);
                curLine.PopulateVertex(m_EdgeLineWidth,m_Axis.up,vertices, indexes1, uvs, normals);
                nextLine.PopulateVertex(m_EdgeLineWidth,m_Axis.up,vertices, indexes1, uvs, normals);
            }
            
            _mesh.subMeshCount = indexes1 != null ? 2 : 1;
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetUVs(0, uvs);
            _mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
            
            if (indexes1 != null)
            {
                _mesh.SetIndices(indexes1, MeshTopology.Triangles, 1);
                ListPool<int>.IDespawn(indexes1);
            }
            
            ListPool<Vector3>.IDespawn(vertices);
            ListPool<Vector3>.IDespawn(normals);
            ListPool<Vector2>.IDespawn(uvs);
            ListPool<int>.IDespawn(indexes);
        }

        public override void DrawGizmos(Transform _viewTransform)
        {
            base.DrawGizmos(_viewTransform);
            Gizmos.matrix = transform.localToWorldMatrix;
            m_Axis.DrawGizmos();
            Gizmos.color = Color.white.SetA(.3f);
            new GDisk(m_Axis.origin,m_Axis.up,m_Radius).DrawGizmos();

            var resolution = (float)m_Resolution;
            var forward = m_Axis.forward.rotateCW(m_Axis.up, kmath.kPI2 * (m_AngleRange.start / 360f)) * m_Radius;
            for (var i = 0; i <= resolution; i++)
            {
                var curX = (i / resolution);
                var diskEdge = new GLine(m_Axis.origin,m_Axis.origin + (m_Radius * forward.rotateCW(m_Axis.up, kmath.kPI2 * curX * (m_AngleRange.length / 360f)))).Trim(m_DiskTrim);
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