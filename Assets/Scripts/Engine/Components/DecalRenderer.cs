using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Runtime
{
    public class DecalRenderer : ARendererBase
    {
        private ValueChecker<Matrix4x4> m_MatrixValidator = new ValueChecker<Matrix4x4>();
        [CullingMask] public int m_Layer = int.MaxValue;
        public GBox m_Bounding = GBox.kDefault;
        [Range(0,0.9999f)]public float m_Falloff = 0.5f;
        protected override void OnInitialize() {
            m_MatrixValidator.Set(transform.localToWorldMatrix);
        }

        protected override void Tick(float _deltaTime)
        {
            base.Tick(_deltaTime);
            if(m_MatrixValidator.Check(transform.localToWorldMatrix))
                SetDirty();
        }

        [InspectorButton]
        protected override void Validate()
        {
           m_MatrixValidator.Set(Matrix4x4.identity);      //how mysterious
           Tick(0f);
        }

        protected override void PopulateMesh(Mesh _mesh,Transform _viewTransform)
        {
            var intersectBox = new GBox((float3)transform.position + m_Bounding.center    ,m_Bounding.extent);
            
            //Filter available meshes
            var meshRenderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Collect(p => p.bounds.Intersects(intersectBox));

            var triangles = new List<GTriangle>();
            foreach (var renderer in meshRenderers)
            {
                if (( m_Layer & (1 << renderer.gameObject.layer)) == 0)
                    continue;
                
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var intersectMesh = meshFilter.sharedMesh;
                var trianglesOS = intersectMesh.GetPolygonVertices(out var meshVertices,out var meshIndexes);

                var localToObject =  transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
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
                    math.dot(tangent,vertex)/m_Bounding.size.x + .5f,
                    math.dot(biTangent,vertex)/m_Bounding.size.y + .5f
                    ));
            }

            _mesh.Clear();
            _mesh.SetVertices(curVertices);
            _mesh.SetNormals(curNormals);
            _mesh.SetColors(curColors);
            _mesh.SetUVs(0,curUVs);
            _mesh.SetIndices(curIndexes,MeshTopology.Triangles,0,true);
        }

        public override void DrawGizmos(Transform _viewTransform)
        {
            base.DrawGizmos(_viewTransform);
            Gizmos.matrix = transform.localToWorldMatrix;
            m_Bounding.DrawGizmos();
        }
    }
}
