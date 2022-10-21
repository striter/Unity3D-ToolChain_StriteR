using System;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using TTouchTracker;
using UnityEngine;
using TPoolStatic;

namespace PCG
{
    using static PCGDefines<int>;
    public class GridManager : MonoBehaviour,IPolyGridControl
    {
        public int m_ChunkSize = 10;
        public Dictionary<SurfaceID, PolyArea> m_Areas { get; } = new Dictionary<SurfaceID, PolyArea>();
        public Dictionary<SurfaceID, PolyVertex> m_Vertices { get; } = new Dictionary<SurfaceID, PolyVertex>();
        public Dictionary<SurfaceID, PolyQuad> m_Quads { get; } = new Dictionary<SurfaceID, PolyQuad>();

        public Dictionary<SurfaceID, PolyChunk> m_Chunks { get; } = new Dictionary<SurfaceID, PolyChunk>();     //For future usage

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
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            m_Chunks.Clear();
            UHexagonArea.Init(m_ChunkSize * 4); //Quad made of 4 points
            TSPoolList<HexCoord>.Spawn(out var vertexIndex);
            TSPoolList<HexCoord>.Spawn(out var quadIndex);
            TSPoolList<HexCoord>.Spawn(out var chunkIndex);
            Dictionary<HexCoord, int> vertexIDQuery = new Dictionary<HexCoord, int>();

            foreach (var tuple in _data.areaData.LoopIndex())
            {
                var areaData = tuple.value;
                var areaIdentity = tuple.index;
                var area = new PolyArea(areaData.identity);
                m_Areas.Add(areaIdentity, area);
                //Insert Vertices&Quads
                foreach (var data in areaData.m_Vertices)
                {
                    vertexIndex.Add(data.identity);
                    var identity = vertexIndex.Count - 1;
                    vertexIDQuery.Add(data.identity, identity);

                    var coord = data.coord * KPCG.kPolySize;

                    m_Vertices.TryAdd(identity, () => new PolyVertex() { m_Identity = identity, m_Coord = coord, m_Invalid = data.invalid });
                    area.m_Vertices.Add(m_Vertices[identity]);
                }

                foreach (var quad in areaData.m_Quads)
                {
                    quadIndex.Add(quad.identity);
                    var quadID = quadIndex.Count - 1;
                    var quadVertices = quad.quad.Convert(p => new SurfaceID(vertexIDQuery[p]));
                    var polyQuad = new PolyQuad(quadID, quadVertices, m_Vertices);
                    m_Quads.Add(polyQuad.m_Identity, polyQuad);
                    area.m_Quads.Add(polyQuad);

                    var chunkCoord = UHexagonArea.GetBelongAreaCoord(quad.identity);
                    chunkIndex.TryAdd(chunkCoord);
                    var chunkID = chunkIndex.IndexOf(chunkCoord);
                    if (!m_Chunks.ContainsKey(chunkID))
                        m_Chunks.Add(chunkID, new PolyChunk() { m_Center = polyQuad.m_CenterWS });     //We can't get the chunk's center without doing sum/average,Simply fill it with one quad's center
                    m_Chunks[chunkID].m_QuadIDs.Add(quadID);
                }

                foreach (var quad in m_Quads.Values)
                {
                    foreach (var vertexID in quad.m_Hex)
                        m_Vertices[vertexID].AddNearbyQuads(quad);
                }
            }

            foreach (var vertex in m_Vertices.Values)
                vertex.Initialize(m_Vertices);

            TSPoolList<HexCoord>.Recycle(vertexIndex);
            TSPoolList<HexCoord>.Recycle(quadIndex);
            TSPoolList<HexCoord>.Recycle(chunkIndex);
            vertexIDQuery = null;
            return this;
        }


        public bool ValidatePlaneSelection(Ray _ray, GPlane _plane, out SurfaceID _vertexID)
        {
            var hitCoord = _ray.GetPoint(UGeometryIntersect.RayPlaneDistance(_plane, _ray)).ToCoord();
            _vertexID = default;
            if (m_Quads.Values.TryFind(p => p.m_CoordWS.IsPointInside(hitCoord), out var quad))
            {
                var quadVertexIndex = quad.m_CoordWS.NearestPointIndex(hitCoord);
                _vertexID = quad.m_Hex[quadVertexIndex];
                return true;
            }
            return false;
        }

        public bool ValidateSideVertex(SurfaceID _vertID, Vector3 _hitPoint, out SurfaceID _sideVertex)
        {
            _sideVertex = default;
            if (!m_Vertices.ContainsKey(_vertID))
                return false;

            float minSQRDistance = float.MaxValue;
            foreach (var tVertID in m_Vertices[_vertID].m_NearbyVertIds)
            {
                var vert = m_Vertices[tVertID];
                var sqrDistance = Vector3.SqrMagnitude(vert.m_Coord.ToPosition() - _hitPoint);
                if (minSQRDistance < sqrDistance)
                    continue;
                minSQRDistance = sqrDistance;
                _sideVertex = tVertID;
            }
            return true;
        }

        public (bool, SurfaceID) ValidatePos(Coord pos)
        {
            SurfaceID vertexID = default;
            if (m_Quads.Values.TryFind(p => p.m_CoordWS.IsPointInsideOrOnSegment(pos), out var quad))
            {
                var quadVertexIndex = quad.m_CoordWS.NearestPointIndex(pos);
                vertexID = quad.m_Hex[quadVertexIndex];
                return (true, vertexID);
            }
            return (false, vertexID);
        }

        private readonly PolyGridOutput kOutpput = new PolyGridOutput();
        public bool GetSelectionCorners(PCGID _origin, Int3 _cornerSize, out PolyGridOutput _output)
        {
            _output = null;
            kOutpput.rValidateCorners.Clear();
            kOutpput.rQuadsAvailable = true;
            kOutpput.rValidateQuads.Clear();
            TSPoolList<SurfaceID>.Spawn(out var locationCheck);

            var rightShiftOrigin = _origin.location;
            for (int i = 0; i < _cornerSize.x; i++)
            {
                if (i != 0)
                    rightShiftOrigin = m_Vertices[rightShiftOrigin].m_RightVertex;

                var forwardShiftOrigin = rightShiftOrigin;
                for (int j = 0; j < _cornerSize.z; j++)
                {
                    if (j != 0)
                        forwardShiftOrigin = m_Vertices[forwardShiftOrigin].m_ForwardVertex;
                    if (!locationCheck.TryAdd(forwardShiftOrigin))
                        continue;
                    var upperShiftOrigin = new PCGID(forwardShiftOrigin, _origin.height);
                    for (int k = 0; k < _cornerSize.y; k++)
                    {
                        var finalCorner = upperShiftOrigin;
                        if (k != 0 && !upperShiftOrigin.TryUpward(out finalCorner, k))
                            continue;
                        kOutpput.rValidateCorners.Add(finalCorner);
                    }

                    var relativeQuads = m_Vertices[forwardShiftOrigin].m_NearbyQuads;
                    if (relativeQuads.Count < 4)
                    {
                        kOutpput.rQuadsAvailable = false;
                        continue;
                    }
                    kOutpput.rValidateQuads.TryAdd(new Int2(i + 1, j + 1), relativeQuads[0].m_Identity);
                    kOutpput.rValidateQuads.TryAdd(new Int2(i + 1, j), relativeQuads[1].m_Identity);
                    kOutpput.rValidateQuads.TryAdd(new Int2(i, j), relativeQuads[2].m_Identity);
                    kOutpput.rValidateQuads.TryAdd(new Int2(i, j + 1), relativeQuads[3].m_Identity);
                }
            }

            _output = kOutpput;
            TSPoolList<SurfaceID>.Recycle(locationCheck);
            return _output != null;
        }

        public SurfaceID FindNearestVert(Vector2 _pos)
        {
            SurfaceID minVId = default;
            float minDis = float.MaxValue;
            foreach (var pair in m_Vertices)
            {
                var vId = pair.Key;
                var vert = pair.Value;
                var dx = _pos.x - vert.m_Coord.x;
                var dy = _pos.y - vert.m_Coord.y;
                var dis = dx * dx + dy * dy;
                if (dis < minDis)
                {
                    minVId = vId;
                    minDis = dis;
                }
            }
            return minVId;
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

        public bool m_ChunkGizmos;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (m_VertexGizmos)
            {
                foreach (var vertex in m_Vertices.Values)
                {
                    Gizmos.color = vertex.m_Invalid ? Color.red : Color.green;
                    Gizmos.DrawSphere(vertex.m_Coord.ToPosition(), .2f);
                    if (!m_VertexDirection || vertex.m_Invalid)
                        continue;

                    if (m_VertexAdjacentRelation)
                    {
                        foreach (var (index, adjacentVertex) in vertex.IterateNearbyVertices().LoopIndex())
                        {
                            Gizmos.color = UColor.IndexToColor(index);
                            Gizmos_Extend.DrawLine(vertex.m_Coord.ToPosition(), adjacentVertex.m_Coord.ToPosition(), .4f);
                        }
                    }
                    else if (m_VertexIntervalRelation)
                    {
                        foreach (var (index, intervalVertex) in vertex.IterateIntervalVertices().LoopIndex())
                        {
                            Gizmos.color = UColor.IndexToColor(index);
                            Gizmos_Extend.DrawLine(vertex.m_Coord.ToPosition(), intervalVertex.m_Coord.ToPosition(), .4f);
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos_Extend.DrawLine(vertex.m_Coord.ToPosition(), m_Vertices[vertex.m_RightVertex].m_Coord.ToPosition(), .4f);
                        Gizmos.color = Color.blue;
                        Gizmos_Extend.DrawLine(vertex.m_Coord.ToPosition(), m_Vertices[vertex.m_ForwardVertex].m_Coord.ToPosition(), .4f);
                    }
                }
            }

            if (m_QuadGizmos)
            {
                foreach (var quad in m_Quads.Values)
                {
                    Gizmos.color = Color.white.SetAlpha(.3f);
                    Gizmos_Extend.DrawLinesConcat(quad.m_Hex.Iterate(p => m_Vertices[p].m_Coord.ToPosition()));

                    if (m_QuadVertexRelation)
                        for (int i = 0; i < quad.m_Hex.Length; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            Gizmos_Extend.DrawLine(quad.m_CenterWS.ToPosition(), m_Vertices[quad.m_Hex[i]].m_Coord.ToPosition(), .8f);
                        }
                }
            }

            if (m_ChunkGizmos)
            {
                foreach (var (index, chunk) in m_Chunks.Values.LoopIndex())
                {
                    Gizmos.color = UColor.IndexToColor(index).SetAlpha(.5f);
                    foreach (var quadID in chunk.m_QuadIDs)
                        Gizmos_Extend.DrawLinesConcat(m_Quads[quadID].m_Hex.Iterate(_p => m_Vertices[_p].m_Coord.ToPosition()));
                }
            }
        }
        #endregion
#endif
    }

}
