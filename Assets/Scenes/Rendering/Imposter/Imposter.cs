using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Explicit;
using Runtime.Geometry.Extension;
using Runtime.Geometry.Extension.BoundingSphere;
using Unity.Mathematics;
using UnityEngine;

public class Imposter : MonoBehaviour
{
    public GSphere m_BoundingSphere;

    public int width = 4;
    public int height = 2;
    
    [Button]
    void Construct()
    {
        var meshRenderers = GetComponentsInChildren<MeshRenderer>(false);

        List<float3> verticies = new List<float3>();
        foreach (var renderer in meshRenderers)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (!meshFilter || meshFilter.sharedMesh == null)
                continue;
            
            var matrix = renderer.transform.localToWorldMatrix;
            var intersectMesh = meshFilter.sharedMesh;
            verticies.AddRange(intersectMesh.vertices.Select(p=>(float3)matrix.MultiplyPoint(p)));
        }

        if (verticies.Count == 0)
            return;

        m_BoundingSphere = UGeometry.GetBoundingSphere(verticies);

        for (var j = 0 ; j < height ; j++)
        for(var i = 0 ; i < width ; i++)
        {
            var uv = new float2((i+.5f) / width, (j+.5f) / height);
            
            var direction = USphereExplicit.UV.GetPoint(uv);
            var position =  m_BoundingSphere.center + direction * (m_BoundingSphere.radius + 0.1f);
            
            Debug.DrawLine(position ,m_BoundingSphere.center,UColor.IndexToColor(i),5f);
        }
        
    }

    private void OnDrawGizmos()
    {
        m_BoundingSphere.DrawGizmos();
    }
}
