using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Runtime
{
    public class DecalRenderer : ARendererBase
    {
        [CullingMask] public int m_Layer = int.MaxValue;
        public GBox m_Bounding = GBox.kDefault;
        [Range(0,0.9999f)]public float m_Falloff = 0.5f;
        [InspectorButton]
        protected override void OnInitialize() => SetDirty();
        protected override void PopulateMesh(Mesh _mesh,Transform _viewTransform)
        {
            var intersectBox = new GBox((float3)transform.position + m_Bounding.center    ,m_Bounding.extent);
            
            //Filter available meshes
            var meshRenderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Collect(p => p.bounds.Intersects(intersectBox));

            var triangles = PoolList<GTriangle>.Empty(nameof(DecalRenderer) + "_Triangles");
            foreach (var renderer in meshRenderers)
            {
                if (!CullingMask.HasLayer(m_Layer,renderer.gameObject.layer))
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
            var curIndexes = PoolList<int>.Empty(nameof(DecalRenderer) + "_Indexes");
            var curVertices = PoolList<Vector3>.Empty(nameof(DecalRenderer) + "_Vertices");
            var curNormals = PoolList<Vector3>.Empty(nameof(DecalRenderer) + "_Normals");
            for (var i = 0; i < triangles.Count; i++)
            {
                var shape = triangles[i];
                for (var j = 0; j < 3; j++)
                {
                    curIndexes.Add(curVertices.Count);
                    curNormals.Add(shape.normal);
                    curVertices.Add(shape.triangle[j]);
                }
            }

            var curColors = PoolList<Color>.Empty(nameof(DecalRenderer) + "_Colors");
            var curUVs = PoolList<Vector2>.Empty(nameof(DecalRenderer) + "_UVs");
            var up = kfloat3.back;      // train
            var tangent = kfloat3.right;
            var biTangent = kfloat3.up;
            for (var i = 0; i < curNormals.Count; i++)
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
            _mesh.SetIndices(curIndexes,MeshTopology.Triangles,0,false);
            _mesh.bounds = m_Bounding;
        }

        public override void DrawGizmos(Transform _viewTransform)
        {
            base.DrawGizmos(_viewTransform);
            Gizmos.matrix = transform.localToWorldMatrix;
            m_Bounding.DrawGizmos();
        }
    }
}
