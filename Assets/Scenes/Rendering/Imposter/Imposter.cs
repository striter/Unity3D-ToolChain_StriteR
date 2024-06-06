using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

public class Imposter : MonoBehaviour
{
    public GSphere m_BoundingSphere;
        
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
    }

    private void OnDrawGizmos()
    {
        m_BoundingSphere.DrawGizmos();
    }
}
