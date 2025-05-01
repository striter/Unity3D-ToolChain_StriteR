using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace Runtime.Optimize.Voxelizer
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

#if UNITY_EDITOR
        [EditorPath] public string m_Path;
        private BoundingVolumeHierarchy<GBox,GTriangle,BoundaryTreeHelper.GBox_GTriangle> m_Voxelizer = new(64,8);

        private List<float> kIntersectDistances = new List<float>();
        private List<GTriangle> m_Triangles = new List<GTriangle>();
        [InspectorButton]
        void Construct()
        {
            var meshRenderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Collect(p => p.bounds.Intersects(m_Box));

            m_Triangles.Clear();
            foreach (var renderer in meshRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var intersectMesh = meshFilter.sharedMesh;
                var trianglesOS = intersectMesh.GetPolygonVertices(out var meshVertices,out var meshIndexes);

                var localToObject =  transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
                m_Triangles.AddRange(trianglesOS.Select(p=>(localToObject*p)));
            }

            if (m_Triangles.Count == 0)
                return;
            
            m_Voxelizer.Construct(m_Triangles);

            
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
                    var ray = new GRay(m_Box.GetPoint(new float3(0, step * (j + .5f),step * (i + .5f)) - .5f), kfloat3.right);
                    foreach (var triangle in m_Voxelizer.Query(m_Triangles,p=>ray.Intersect(p)))
                    {
                        if (ray.Intersect(triangle, out var distance))
                            kIntersectDistances.TryAdd(distance);
                    }

                    if (kIntersectDistances.Count % 2 != 0)
                        kIntersectDistances.RemoveLast();

                    if (kIntersectDistances.Count <= 0)
                        continue;

                    kIntersectDistances.Sort((a, b) => a > b ? 1 : -1);

                    // Debug.DrawLine(ray.GetPoint(kIntersectDistances[0]) + kfloat3.right*step*.1f,ray.GetPoint(kIntersectDistances[0]) + kfloat3.left*step*.1f,Color.red,10f);
                    // Debug.DrawLine(ray.GetPoint(kIntersectDistances[^1]) + kfloat3.right*step*.1f,ray.GetPoint(kIntersectDistances[^1]) + kfloat3.left*step*.1f,Color.blue,10f);
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
            UnityEditor.Extensions.UEAsset.CreateOrReplaceMainAsset(texture, UEPath.PathRegex(m_Path));
        }

        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            m_Box.DrawGizmos();
            
            if(m_DrawGizmos)
                m_Voxelizer.DrawGizmos(m_Triangles);
        }
#endif
    }

}