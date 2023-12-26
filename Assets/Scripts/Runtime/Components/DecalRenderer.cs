using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Runtime
{
    [Serializable]
    public class FDecalRenderer: ARuntimeRendererBase
    {
        [CullingMask] public int m_Layer = int.MaxValue;
        public float m_Width = 1;
        public float m_Height = 1;
        public float m_Distance = 1;
        [Range(0,0.9999f)]public float m_Falloff = 0.5f;

        private static int kInstanceID = 0;
        protected override string GetInstanceName() => $"Decal - ({kInstanceID++})";
        
        protected override void PopulateMesh(Mesh _mesh,Transform _transform,Transform _viewTransform)
        {
            var curBounds = new Bounds(_transform.position,new Vector3(m_Width,m_Height,m_Distance));
            var intersectBox = (GBox) curBounds;
            
            //Filter available meshes
            var meshRenderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Collect(p => p.bounds.Intersects(curBounds));

            List<GTriangle> triangles = new List<GTriangle>();
            foreach (var renderer in meshRenderers)
            {
                if (( m_Layer & (1 << renderer.gameObject.layer)) == 0)
                    continue;
                
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var intersectMesh = meshFilter.sharedMesh;
                var trianglesOS = intersectMesh.GetPolygonVertices(out var meshVertices,out var meshIndexes);

                var localToObject =  _transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
                triangles.AddRange(trianglesOS.Select(p=>(localToObject*p)));
            }
            
            
            //Clip Planes
            foreach (var plane in new GBox(float3.zero, intersectBox.extent).GetPlanes(true, true, true))
            {
                for (int i = triangles.Count - 1; i >= 0; i--)
                {
                    var triangle = triangles[i];
                    if (triangle.Clip(plane, out var clippedShape, false))
                    {
                        if (clippedShape is GTriangle clippedTriangle)
                        {
                            triangles[i] = clippedTriangle;
                        }
                        else if(clippedShape is GQuad clippedQuad)
                        {
                            clippedQuad.GetTriangles(out var  triangle1,out var triangle2);
                            triangles[i] = triangle1;
                            triangles.Insert(i,triangle2);
                        }
                        continue;
                    }
                    
                    triangles.RemoveAt(i);
                }

            }
            
            //Create decal with clipped planes
            List<int> curIndexes = new List<int>();
            List<Vector3> curVertices = new List<Vector3>();
            List<Vector3> curNormals = new List<Vector3>();
            for (int i = 0; i < triangles.Count; i++)
            {
                var shape = triangles[i];
                for (int j = 0; j < 3; j++)
                {
                    var appendVertex = (Vector3)shape.triangle[j];
                    var vertexIndex= curVertices.FindIndex(p=>p== appendVertex);
                    if (vertexIndex >= 0)
                    {
                        curNormals[vertexIndex] += (Vector3)shape.normal;
                        curIndexes.Add(vertexIndex);
                    }
                    else
                    {
                        curIndexes.Add(curVertices.Count);
                        curNormals.Add(shape.normal);
                        curVertices.Add(appendVertex);
                    }
                }
            }

            List<Color> curColors = new List<Color>();
            List<Vector2> curUVs = new List<Vector2>();
            var up = kfloat3.back;      // train
            var tangent = kfloat3.right;
            var biTangent = kfloat3.up;
            for (int i = 0; i < curNormals.Count; i++)
            {
                var vertex = curVertices[i];
                var normal = curNormals[i].normalized;
                curNormals[i] = curNormals[i].normalized;
                var alpha =math.clamp( (math.dot(up, normal) / up.magnitude() - m_Falloff)/(1-m_Falloff),0,1);
                curColors.Add(Color.white.SetA(alpha));
                curUVs.Add(new Vector2(
                    math.dot(tangent,vertex)/m_Width + .5f,
                    math.dot(biTangent,vertex)/m_Height + .5f
                    ));
            }

            _mesh.Clear();
            _mesh.SetVertices(curVertices);
            _mesh.SetNormals(curNormals);
            _mesh.SetColors(curColors);
            _mesh.SetUVs(0,curUVs);
            _mesh.SetIndices(curIndexes,MeshTopology.Triangles,0,true);
        }

        public override void DrawGizmos(Transform _transform)
        {
            base.DrawGizmos(_transform);
            Gizmos.matrix = _transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero,new Vector3(m_Width,m_Height,m_Distance));
        }
    }
    
    public class DecalRenderer : ARuntimeRendererMonoBehaviour<FDecalRenderer>
    {
        private ValueChecker<Matrix4x4> m_MatrixValidator = new ValueChecker<Matrix4x4>();
        private void OnValidate() => m_MatrixValidator.Set(Matrix4x4.identity);      //how mysterious

        protected override void Awake()
        {
            base.Awake();
            m_MatrixValidator.Set(transform.localToWorldMatrix);
        }

        private new void Update()
        {
            if(m_MatrixValidator.Check(transform.localToWorldMatrix))
                PopulateMesh();
        }
    }
}
