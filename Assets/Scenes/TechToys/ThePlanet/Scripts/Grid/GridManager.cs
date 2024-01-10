using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using TTouchTracker;
using UnityEngine;
using TObjectPool;
using Unity.Mathematics;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.ThePlanet
{
    public class GridManager : MonoBehaviour,IPCGControl
    {
        public Dictionary<GridID, PCGChunk> m_Chunks { get; } = new Dictionary<GridID, PCGChunk>();
        public Dictionary<GridID, PCGVertex> m_Vertices { get; } = new Dictionary<GridID, PCGVertex>();
        public Dictionary<GridID, PCGQuad> m_Quads { get; } = new Dictionary<GridID, PCGQuad>();
        public GridCollection m_Data;

        public void Init()
        {
        }
        public void Tick(float _deltaTime)
        {
        }

        public void Clear()
        {

        }

        public void Dispose()
        {
        }

        public GridManager Setup(GridCollection _data)
        {
            m_Data = _data;
            m_Chunks.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            int chunkIndex = 0;
            int vertexIndex = 0;
            int quadIndex = 0;
            foreach (var vertex in _data.vertices)
            {
                m_Vertices.Add(vertexIndex, new PCGVertex() {
                    m_Identity = vertexIndex,
                    m_Position = vertex.position * KPCG.kGridSize,
                    m_Normal = vertex.normal,
                    m_Invalid = vertex.invalid
                });
                vertexIndex++;
            }
            
            foreach (var chunk in _data.chunks)
            {
                m_Chunks.Add(chunkIndex, new PCGChunk(chunkIndex));
                chunkIndex++;

                foreach (var gridQuad in chunk.quads)
                {
                    var quad = new PCGQuad(quadIndex, gridQuad.vertices, m_Vertices);
                    m_Quads.Add(quad.m_Identity, quad);
                    
                    foreach (var vertexID in gridQuad.vertices)
                        m_Vertices[vertexID].PreInitialize(quad);

                    quadIndex++;
                }

            }

            foreach (var vertex in m_Vertices.Values)
                vertex.Initialize(m_Vertices);
            return this;
        }

        public bool ValidateGridSelection(GRay _ray, out GridID _vertexID)
        {
            _vertexID = default;
            var distance = UGeometry.Distance(_ray,new GSphere(0, KPCG.kGridSize)).x;
            if (distance < 0)
                return false;
            var hitPos = _ray.GetPoint(distance);
            var nearestVertex = m_Vertices.Values.MinElement(p => (hitPos - p.m_Position).sqrmagnitude());
            _vertexID = nearestVertex?.m_Identity ?? -1;
            return nearestVertex!=null;
        }

        public bool ValidateSideVertex(GridID _vertID, float3 _hitPoint, out GridID _sideVertex)
        {
            _sideVertex = default;
            if (!m_Vertices.ContainsKey(_vertID))
                return false;

            float minSQRDistance = float.MaxValue;
            foreach (var tVertID in m_Vertices[_vertID].m_NearbyVertIds)
            {
                var vert = m_Vertices[tVertID];
                var sqrDistance = Vector3.SqrMagnitude(vert.m_Position - _hitPoint);
                if (minSQRDistance < sqrDistance)
                    continue;
                minSQRDistance = sqrDistance;
                _sideVertex = tVertID;
            }
            return true;
        }

#if UNITY_EDITOR

        #region Gizmos
        [Header("Gizmos")]
        public bool m_VertexGizmos;
        [MFoldout(nameof(m_VertexGizmos), true)] public bool m_VertexDirection;
        [MFoldout(nameof(m_VertexGizmos), true, nameof(m_VertexDirection), true)] public bool m_VertexAdjacentRelation;
        [MFoldout(nameof(m_VertexGizmos), true, nameof(m_VertexDirection), true)] public bool m_VertexIntervalRelation;

        public bool m_QuadGizmos;
        [MFoldout(nameof(m_QuadGizmos), true)] public bool m_QuadVertexRelation;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (m_VertexGizmos)
            {
                foreach (var vertex in m_Vertices.Values)
                {
                    Gizmos.color = vertex.m_Invalid ? Color.red : Color.green;
                    Gizmos.DrawWireSphere(vertex.m_Position, .1f);
                    Gizmos.DrawLine(vertex.m_Position,vertex.m_Position+vertex.m_Normal);
                    
                    if (!m_VertexDirection || vertex.m_Invalid)
                        continue;

                    if (m_VertexAdjacentRelation)
                    {
                        foreach (var (index, adjacentVertex) in vertex.IterateNearbyVertices().LoopIndex())
                        {
                            Gizmos.color = UColor.IndexToColor(index);
                            UGizmos.DrawLine(vertex.m_Position, adjacentVertex.m_Position, .4f);
                        }
                    }
                    else if (m_VertexIntervalRelation)
                    {
                        foreach (var (index, intervalVertex) in vertex.IterateIntervalVertices().LoopIndex())
                        {
                            Gizmos.color = UColor.IndexToColor(index);
                            UGizmos.DrawLine(vertex.m_Position, intervalVertex.m_Position, .4f);
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        UGizmos.DrawLine(vertex.m_Position, m_Vertices[vertex.m_RightVertex].m_Position, .4f);
                        Gizmos.color = Color.blue;
                        UGizmos.DrawLine(vertex.m_Position, m_Vertices[vertex.m_ForwardVertex].m_Position, .4f);
                    }
                }
            }

            if (m_QuadGizmos)
            {
                foreach (var quad in m_Quads.Values)
                {
                    Gizmos.color = Color.white.SetA(.3f);
                    UGizmos.DrawLinesConcat(quad.m_Indexes.Iterate(p => (float3)m_Vertices[p].m_Position));
                    quad.m_ShapeWS.DrawGizmos();
                    
                    
                    if (m_QuadVertexRelation)
                        for (int i = 0; i < quad.m_Indexes.Length; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            UGizmos.DrawLine(quad.position, m_Vertices[quad.m_Indexes[i]].m_Position, .8f);
                        }
                }
            }
        }
        #endregion
#endif
    }

}
