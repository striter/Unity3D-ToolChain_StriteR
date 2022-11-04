using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using Rendering.Pipeline;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PCG
{
    using static PCGDefines<int>;
    public class GridMeshConstructor : MonoBehaviour
    {
        public Material m_AreaMaterial;
        private TObjectPoolClass<int, AreaRenderer> m_AreaMeshes;

        private AnimationCurve m_Curve;
        private readonly Counter m_SelectionFadeTimer = new Counter();

        private Transform m_Selection;
        private Mesh m_SelectionMesh;
        private MeshRenderer m_SelectionRenderer;
        private MaterialPropertyBlock m_SelectionRendererBlock;

        public GridMeshConstructor Init(Transform _transform)
        {
            var meshTransform = _transform.Find("Mesh");
            m_AreaMeshes = new TObjectPoolClass<int, AreaRenderer>(meshTransform.Find("Area/Item"));

            m_Selection = meshTransform.Find("Markup");
            m_SelectionMesh = new Mesh { name = "Select", hideFlags = HideFlags.HideAndDontSave };
            m_SelectionMesh.MarkDynamic();

            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
            m_SelectionRenderer = m_Selection.GetComponent<MeshRenderer>();
            m_SelectionRendererBlock = new MaterialPropertyBlock();
            return this;
        }

        public void ConstructArea(PolyArea _area)
        {
            // Debug.LogWarning($"Area Mesh Populate {_area.m_Identity.coord}");
            m_AreaMeshes.Spawn().GenerateMesh(_area, m_AreaMaterial);
        }

        public void Clear()
        {
            m_AreaMeshes.Clear();
            m_Selection.SetActive(false);
            m_SelectionMesh.Clear();
        }

        public void Tick(float _deltaTime)
        {
            if (!m_SelectionFadeTimer.m_Playing)
                return;

            m_SelectionFadeTimer.Tick(_deltaTime);
            m_SelectionRendererBlock.SetFloat(KShaderProperties.kAlpha, m_Curve.Evaluate(m_SelectionFadeTimer.m_TimeElapsed));
            m_SelectionRenderer.SetPropertyBlock(m_SelectionRendererBlock);

            if (m_SelectionFadeTimer.m_Playing)
                return;
            m_Selection.SetActive(false);
        }

        public void Fadeout(AnimationCurve _curve)
        {
            m_Curve = _curve;
            m_SelectionFadeTimer.Set(m_Curve.length);
            m_SelectionFadeTimer.Replay();
        }

        private static readonly int kMainTex = Shader.PropertyToID("_MainTex");
        public void SetAreaTexture(Texture _texture, Color _color)
        {
            m_AreaMaterial.SetTexture(kMainTex, _texture);
            m_AreaMaterial.color = _color;
        }

        public void SetAreaActive(bool _active)
        {
            m_AreaMeshes.transform.SetActive(_active);
        }

        public void ConstructMarkup(PCGID _origin, IEnumerable<PCGID> _corners, Dictionary<SurfaceID, PolyVertex> _vertices, Func<PCGID, PolyVertex, Color> _getColor, bool _plane = true)
        {
            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<int>.Spawn(out var indices);
            TSPoolList<Vector2>.Spawn(out var uvs);
            TSPoolList<Color>.Spawn(out var colors);
            TSPoolList<GQuad>.Spawn(out var cornerQuads);

            var originVertex = _vertices[_origin.location];
            var center = originVertex.m_Coord;
            foreach (var cornerID in _corners)
            {
                var height = KPCG.GetPlaneHeight(cornerID.height);
                var vertex = _vertices[cornerID.location];
                cornerQuads.Clear();
                foreach (var (index, quad) in vertex.m_NearbyQuads.LoopIndex())
                    cornerQuads.Add(quad.ConstructGeometry(center, vertex.GetQuadVertsArrayCW(index), EQuadGeometry.Half) + height);

                var color = _getColor(cornerID, vertex);
                // if (_plane)
                // {
                //     foreach (var quad in cornerQuads)
                //     {
                //         int indexOffset = vertices.Count;
                //         for (int i = 0; i < 4; i++)
                //         {
                //             vertices.Add(quad[i]);
                //             uvs.Add(URender.IndexToQuadUV(i));
                //             colors.Add(color);
                //         }
                //     
                //         UPolygon.QuadToTriangleIndices(indices, indexOffset + 0, indexOffset + 1, indexOffset + 2, indexOffset + 3);
                //     }
                // }
                // else
                // {
                foreach (var quad in cornerQuads)
                {
                    var qube = quad.ExpandToQube(KPCG.kCornerHeightVector, 0f);
                    qube.FillTopDownQuadTriangle(ECubeFacing.D, vertices, indices, uvs, null, colors, color);
                    //if (_plane)
                    //{
                    //    qube.FillTopDownQuadTriangle(ECubeFacing.D, vertices, indices, uvs, null, colors, color);
                    //}
                    //else
                    //{
                    //    var v = Vector3.down * 2;
                    //    qube = new Qube<Vector3>(qube.vDB, qube.vDL, qube.vDF, qube.vDR,
                    //        qube.vTB + v, qube.vTL + v, qube.vTF + v, qube.vTR + v);
                    //    qube.FillFacingQuadTriangle(ECubeFacing.T, vertices, indices, uvs, null, colors, color);
                    //    qube.FillTopDownQuadTriangle(ECubeFacing.LF, vertices, indices, uvs, null, colors, color);
                    //    qube.FillTopDownQuadTriangle(ECubeFacing.FR, vertices, indices, uvs, null, colors, color);
                    //    qube.FillFacingQuadTriangle(ECubeFacing.D, vertices, indices, uvs, null, colors, color);
                    //}
                }
                // }
            }

            m_SelectionMesh.Clear();
            m_SelectionMesh.SetVertices(vertices);
            m_SelectionMesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            m_SelectionMesh.SetUVs(0, uvs);
            m_SelectionMesh.SetColors(colors);

            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indices);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Color>.Recycle(colors);
            TSPoolList<GQuad>.Recycle(cornerQuads);

            m_Selection.SetActive(true);
            m_SelectionRendererBlock.SetFloat(KShaderProperties.kAlpha, 0.8f);
            m_SelectionRenderer.SetPropertyBlock(m_SelectionRendererBlock);

            m_Selection.transform.position = originVertex.m_Coord.ToPosition();
        }

        public (Vector3 centerOfMass, Bounds bounds) ConstructGridCollider(PCGID _origin, IList<PCGID> _corners, Dictionary<SurfaceID, PolyVertex> _vertices, Mesh _mesh)
        {
            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<int>.Spawn(out var indices);
            TSPoolList<GQuad>.Spawn(out var cornerQuads);
            UBounds.Begin();
            var center = _vertices[_origin.location].m_Coord;
            var srcHeight = _origin.height;
            foreach (var corner in _corners)
            {
                var height = KPCG.GetCornerHeight((byte)(corner.height - srcHeight));
                var vertex = _vertices[corner.location];
                cornerQuads.Clear();
                foreach (var (index, quad) in vertex.m_NearbyQuads.LoopIndex())
                    cornerQuads.Add(quad.ConstructGeometry(center, vertex.GetQuadVertsArrayCW(index), EQuadGeometry.Half) + height);

                foreach (var cornerQuad in cornerQuads)
                {
                    int indexOffset = vertices.Count;
                    vertices.AddRange(cornerQuad.ExpandToQube(KPCG.kCornerHeightVector, .5f));
                    UPolygon.QuadToTriangleIndices(indices, indexOffset + 0, indexOffset + 3, indexOffset + 2, indexOffset + 1); //Bottom
                    UPolygon.QuadToTriangleIndices(indices, indexOffset + 4, indexOffset + 5, indexOffset + 6, indexOffset + 7); //Top
                    UPolygon.QuadToTriangleIndices(indices, indexOffset + 1, indexOffset + 2, indexOffset + 6, indexOffset + 5); //Forward Left
                    UPolygon.QuadToTriangleIndices(indices, indexOffset + 2, indexOffset + 3, indexOffset + 7, indexOffset + 6); //Forward Right
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            var output = (center.ToPosition() + KPCG.kCornerHeightVector * srcHeight, UBounds.GetBounds(vertices));
            TSPoolList<GQuad>.Recycle(cornerQuads);
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indices);
            return output;
        }

        public (IList<PolyVertex> vertices, Vector3 pos) GenerateMistMesh(IList<PCGID> _corners, Dictionary<SurfaceID, PolyVertex> _vertices, MeshFilter _meshFilter, MeshCollider _collider)
        {
            TSPoolList<PolyVertex>.Spawn(out var result);
            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<int>.Spawn(out var indices);
            TSPoolList<GQuad>.Spawn(out var cornerQuads);

            Coord center = Coord.zero;
            foreach (var corner in _corners)
                center += _vertices[corner.location].m_Coord;
            center /= _corners.Count;

            foreach (var corner in _corners)
            {
                var vertex = _vertices[corner.location];
                result.Add(vertex);
                cornerQuads.Clear();
                foreach (var (index, quad) in vertex.m_NearbyQuads.LoopIndex())
                    cornerQuads.Add(quad.ConstructGeometry(center, vertex.GetQuadVertsArrayCW(index), EQuadGeometry.Half));

                foreach (var quad in cornerQuads)
                {
                    int indexOffset = vertices.Count;
                    for (int i = 0; i < 4; i++)
                    {
                        vertices.Add(quad[i]);
                    }

                    UPolygon.QuadToTriangleIndices(indices, indexOffset + 0, indexOffset + 1, indexOffset + 2, indexOffset + 3);
                }

                //foreach (var cornerQuad in cornerQuads)
                //{
                //    int indexOffset = vertices.Count;
                //    vertices.AddRange(cornerQuad.ExpandToQube(KPolyGrid.kCornerHeightVector));
                //    UPolygon.QuadToTriangleIndices(indices, indexOffset + 0, indexOffset + 3, indexOffset + 2, indexOffset + 1); //Bottom
                //    UPolygon.QuadToTriangleIndices(indices, indexOffset + 4, indexOffset + 5, indexOffset + 6, indexOffset + 7); //Top
                //    UPolygon.QuadToTriangleIndices(indices, indexOffset + 1, indexOffset + 2, indexOffset + 6, indexOffset + 5); //Forward Left
                //    UPolygon.QuadToTriangleIndices(indices, indexOffset + 2, indexOffset + 3, indexOffset + 7, indexOffset + 6); //Forward Right
                //}
            }

            _meshFilter.mesh.Clear();
            _meshFilter.mesh.SetVertices(vertices);
            _meshFilter.mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            _meshFilter.mesh.Optimize();
            _collider.sharedMesh = _meshFilter.mesh;
            var output = (result, center.ToPosition());

            TSPoolList<GQuad>.Recycle(cornerQuads);
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indices);
            return output;
        }
        private class AreaRenderer : ITransform, IPoolCallback<int>
        {
            public Transform Transform { get; }
            public MeshFilter m_MeshFilter { get; }
            public MeshRenderer m_MeshRenderer { get; }

            public AreaRenderer(Transform _transform)
            {
                Transform = _transform;
                m_MeshFilter = _transform.GetComponent<MeshFilter>();
                m_MeshRenderer = _transform.GetComponent<MeshRenderer>();
            }
            public void OnPoolCreate(Action<int> _doRecycle) { }
            public void GenerateMesh(PolyArea _area, Material _areaMaterial)
            {
                m_MeshRenderer.sharedMaterial = _areaMaterial;
                TSPoolList<Vector3>.Spawn(out var vertices);
                TSPoolList<Vector3>.Spawn(out var normals);
                TSPoolList<Vector2>.Spawn(out var uvs);
                TSPoolList<int>.Spawn(out var indices);
                var center = _area.m_Identity.centerCS.ToCoord();
                foreach (var quad in _area.m_Quads)
                {
                    if (quad.m_Vertices.Any(p => p.m_Invalid))
                        continue;

                    int startIndex = vertices.Count;
                    foreach (var (index, coord) in quad.m_CoordWS.LoopIndex())
                    {
                        var positionOS = (coord - center).ToPosition();
                        vertices.Add(positionOS);
                        normals.Add(Vector3.up);
                        uvs.Add(URender.IndexToQuadUV(index));
                    }

                    indices.Add(startIndex);
                    indices.Add(startIndex + 1);
                    indices.Add(startIndex + 2);
                    indices.Add(startIndex + 3);
                    indices.Add(startIndex);
                    indices.Add(startIndex + 2);
                }

                var areaMesh = new Mesh { hideFlags = HideFlags.HideAndDontSave, name = _area.m_Identity.coord.ToString() };
                areaMesh.SetVertices(vertices);
                areaMesh.SetNormals(normals);
                areaMesh.SetIndices(indices, MeshTopology.Triangles, 0);
                areaMesh.SetUVs(0, uvs);
                areaMesh.Optimize();
                areaMesh.UploadMeshData(true);
                m_MeshFilter.sharedMesh = areaMesh;
                Transform.localPosition = _area.m_Identity.centerCS.ToPosition();
                Transform.localScale = Vector3.one;

                TSPoolList<Vector3>.Recycle(vertices);
                TSPoolList<Vector3>.Recycle(normals);
                TSPoolList<Vector2>.Recycle(uvs);
                TSPoolList<int>.Recycle(indices);
            }
            public void OnPoolSpawn(int identity)
            {

            }

            public void OnPoolRecycle()
            {
                m_MeshFilter.sharedMesh.Clear();
            }

            public void OnPoolDispose()
            {
                if (m_MeshFilter.sharedMesh == null)
                    return;
                DestroyImmediate(m_MeshFilter.sharedMesh);
                m_MeshFilter.sharedMesh = null;
            }
        }
    }
}
