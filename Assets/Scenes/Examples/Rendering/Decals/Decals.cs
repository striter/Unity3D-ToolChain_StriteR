
using System.Collections.Generic;
using System.Linq;
using Geometry;
using MeshFragment;
using TPoolStatic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Examples.Rendering.Decals

{
    [ExecuteInEditMode, RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class Decals : MonoBehaviour
    {
        [CullingMask] public int m_Layer = int.MaxValue;
        public float m_Width = 1;
        public float m_Height = 1;
        public float m_Distance = 1;

        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;
        private Mesh m_DecalMesh;
        private void Awake() => Generate();

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero,new Vector3(m_Width,m_Height,m_Distance));
        }

        private static int kInstanceID = 0;
        [Button]
        public void Generate()
        {
            if(m_DecalMesh)
                DestroyImmediate(m_DecalMesh);

            m_Filter = GetComponent<MeshFilter>();
            m_Renderer = GetComponent<MeshRenderer>();
            m_DecalMesh = new Mesh(){name = $"Decal Mesh {kInstanceID++}"};


            var curBounds = new Bounds(transform.position,new Vector3(m_Width,m_Height,m_Distance));
            var intersectBox = (GBox) curBounds;
            
            //Filter available meshes
            var meshRenderers = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Collect(p => p.bounds.Intersects(curBounds));

            List<GTriangle> triangles = new List<GTriangle>();
            foreach (var renderer in meshRenderers)
            {
                if (( m_Layer & 1 >> renderer.gameObject.layer) == 0)
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
            for (int i = 0; i < triangles.Count; i++)
            {
                var shape = triangles[i];
                int startIndex = curVertices.Count;
                if (shape is GTriangle triangle)
                {
                    curVertices.AddRange(triangle.triangle.Select(_p=>(Vector3)_p));
                    curIndexes.Add(startIndex);
                    curIndexes.Add(startIndex + 1);
                    curIndexes.Add(startIndex + 2);
                }
            }

            m_DecalMesh.Clear();
            m_DecalMesh.SetVertices(curVertices);
            m_DecalMesh.SetIndices(curIndexes,MeshTopology.Triangles,0,false);
            m_DecalMesh.bounds = curBounds;
            
            m_Filter.sharedMesh = m_DecalMesh;
        }
        
    }
    
    
}