using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Optimize.Voxelizer
{
    public class VoxelData : ScriptableObject
    {
        public GBox m_Bounds;
        public EResolution m_Resolution;
        [HideInInspector] public bool[] m_Voxels;

        public int3 Resolution => (int)m_Resolution;
        
        public void DrawGizmos()
        {
            m_Bounds.DrawGizmos();
            for(var i = 0; i < Resolution.x; i++)
                for (var j = 0; j < Resolution.y; j++)
                    for (var k = 0; k < Resolution.z; k++)
                    {
                        var valid = m_Voxels[i + j * Resolution.x + k * Resolution.x * Resolution.y];
                        if (!valid)
                            continue;
                        Gizmos.color = Color.white;
                        GBox.Minmax(m_Bounds.GetPoint(new float3(i, j, k)/ Resolution), 
                            m_Bounds.GetPoint(new float3(i + 1, j + 1, k + 1) / Resolution))
                            .DrawGizmos();
                    }
        }


        private static BoundingVolumeHierarchy<GBox,GTriangle,BoundaryTreeHelper.GBox_GTriangle> kVoxelizer = new(64,8);
        private static List<float> kIntersectDistances = new List<float>();
        private static List<GTriangle> kTrianglesWS = new List<GTriangle>();
        public static VoxelData Construct(EResolution _resolution,IList<Renderer> _renderers,string _path)
        {
            if (_renderers == null || _renderers.Count == 0)
                return null;
            
            var boundsWS = GBox.kZero;
            foreach (var renderer in _renderers)
            {
                if (boundsWS.size.magnitude() == 0)
                {
                    boundsWS = renderer.bounds;
                    continue;                    
                }
                boundsWS = boundsWS.Encapsulate(renderer.bounds);
            }

            kTrianglesWS.Clear();
            foreach (var renderer in _renderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var intersectMesh = meshFilter.sharedMesh;
                var trianglesOS = intersectMesh.GetPolygonVertices(out var meshVertices,out var meshIndexes);

                var localToWorld = meshFilter.transform.localToWorldMatrix;
                kTrianglesWS.AddRange(trianglesOS.Select(p=>(localToWorld*p)));
            }

            if (kTrianglesWS.Count == 0)
                return null;
            
            kVoxelizer.Construct(kTrianglesWS);

            var asset = CreateInstance<VoxelData>();
            asset.m_Bounds = boundsWS;
            asset.m_Resolution = _resolution;
            asset.m_Voxels = new bool[(int) _resolution * (int) _resolution * (int) _resolution];
            
            var resolution = (int) _resolution;
            
            var step = 1f / (resolution);

            for (var i = 0; i < resolution; i++)
            {
                for (var j = 0; j < resolution; j++)
                {
                    kIntersectDistances.Clear();
                    var ray = new GRay(boundsWS.GetPoint(new float3(0, step * (j + .5f),step * (i + .5f))), kfloat3.right);
                    foreach (var triangle in kVoxelizer.Query(kTrianglesWS,p=>ray.Intersect(p)))
                    {
                        if (ray.Intersect(triangle, out var distance))
                            kIntersectDistances.TryAdd(distance);
                    }

                    if (kIntersectDistances.Count % 2 != 0)
                        kIntersectDistances.RemoveLast();

                    if (kIntersectDistances.Count <= 0)
                        continue;

                    kIntersectDistances.Sort(p=>p);

                    // Debug.DrawLine(ray.GetPoint(kIntersectDistances[0]) + kfloat3.right*step*m_BoundsWS.size.x,ray.GetPoint(kIntersectDistances[0]) + kfloat3.left*step*.1f,Color.red,10f);
                    // Debug.DrawLine(ray.GetPoint(kIntersectDistances[^1]) + kfloat3.right*step*m_BoundsWS.size.x,ray.GetPoint(kIntersectDistances[^1]) + kfloat3.left*step*.1f,Color.blue,10f);
                    
                    var flag = false;
                    var index = 0;
                    for (var k = 0; k < resolution; k++)
                    {
                        var march = step * (k + .5f) * boundsWS.size.x;
                        while (index < kIntersectDistances.Count && march > kIntersectDistances[index])
                        {
                            index++;
                            flag = !flag;
                        }

                        if (!flag) continue;
                        asset.m_Voxels[i * resolution * resolution + j * resolution + k] = true;
                    }
                }
            }

            asset.name = "Voxelizer";
            asset = UnityEditor.Extensions.UEAsset.CreateOrReplaceMainAsset(asset, _path);
            return asset;
        }
        
        public void OutputTexture3D(string _path)
        {
            var texture = new Texture3D(Resolution.x, Resolution.y, Resolution.z, TextureFormat.RGBA32, false,true) {
                    wrapMode = TextureWrapMode.Clamp
            };
            var pixels = texture.GetPixels().Remake(p=>Color.clear);
            var resolution = (int)m_Resolution;
            for (var i = 0; i < resolution; i++)
            {
                for (var j = 0; j < resolution; j++)
                {
                    for (var k = 0; k < resolution; k++)
                    {
                        var index = i * resolution * resolution + j * resolution + k;
                        if (!m_Voxels[index]) 
                            continue;
                        pixels[i * resolution * resolution + j * resolution + k] = Color.white;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = "Voxelizer";
            UnityEditor.Extensions.UEAsset.CreateOrReplaceMainAsset(texture, _path);
        }
    }
}