using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.Rendering.Voxelizer
{
    public enum EResolution
    {
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
    }
    public class Voxelizer : MonoBehaviour
    {
        public GBox m_Box = GBox.kDefault;
        public EResolution m_Resolution = EResolution._64;

        private QuadTree_triangle3 m_Voxelizer = new QuadTree_triangle3(2);

        private List<float> kIntersectDistances = new List<float>();

        [Button]
        void Construct()
        {
            var meshRenderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Collect(p => p.bounds.Intersects(m_Box));

            List<GTriangle> triangles = new List<GTriangle>();
            foreach (var renderer in meshRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var intersectMesh = meshFilter.sharedMesh;
                var trianglesOS = intersectMesh.GetPolygonVertices(out var meshVertices,out var meshIndexes);

                var localToObject =  transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
                triangles.AddRange(trianglesOS.Select(p=>(localToObject*p)));
            }

            if (triangles.Count == 0)
                return;
            
            m_Voxelizer.Construct(triangles,64,16,true,true);

            
            var resolution = (int) m_Resolution;
            Texture3D texture = new Texture3D(resolution, resolution, resolution, TextureFormat.RGBA32, false,true);
            texture.wrapMode = TextureWrapMode.Clamp;
            
            var pixels = texture.GetPixels().Remake(p=>Color.clear);
            var step = 1f / (resolution);

            for (var i = 0; i < resolution; i++)
            {
                for (var j = 0; j < resolution; j++)
                {
                    kIntersectDistances.Clear();
                    var ray = new GRay(m_Box.GetPoint(new float3(0, step * (j + .5f),step * (i + .5f)) - .5f),
                        kfloat3.right);
                    foreach (var node in m_Voxelizer.Collect(p => ray.Intersect(p.boundary)))
                    {
                        foreach (var triangle in node.elements)
                        {
                            if (ray.Intersect(triangle, out var distance))
                                kIntersectDistances.TryAdd(distance);
                        }
                    }

                    if (kIntersectDistances.Count <= 0)
                        continue;

                    if (kIntersectDistances.Count % 2 != 0)
                        kIntersectDistances.RemoveLast();

                    kIntersectDistances.Sort((a, b) => a > b ? 1 : -1);

                    // Debug.DrawLine(ray.GetPoint(intersectDistances[0]),ray.GetPoint(intersectDistances[0]) + kfloat3.up*step*.1f,Color.red,10f);
                    // Debug.DrawLine(ray.GetPoint(intersectDistances[^1]),ray.GetPoint(intersectDistances[^1]) + kfloat3.up*step*.1f,Color.blue,10f);
                    var flag = false;
                    var index = 0;
                    for (var k = 0; k < resolution; k++)
                    {
                        var march = step * (k + .5f);
                        while (index < kIntersectDistances.Count && march > kIntersectDistances[index])
                        {
                            index++;
                            flag = !flag;
                        }

                        if (!flag) continue;
                        pixels[i * resolution * resolution + j * resolution + k] = Color.white;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = "Voxelizer";
            UEAsset.CreateOrReplaceMainAsset(texture, "Assets/Scenes/Examples/Rendering/Voxelizer/Voxelizer.asset");
        }
        


        public bool m_DrawBounds;
        public bool m_DrawElements;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            m_Box.DrawGizmos();
            m_Voxelizer.DrawGizmos(m_DrawBounds,m_DrawElements);
        }
    }

}