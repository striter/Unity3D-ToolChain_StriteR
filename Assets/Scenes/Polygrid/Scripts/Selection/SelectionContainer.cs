using System;
using System.Collections.Generic;
using PolyGrid;
using Procedural;
using Procedural.Hexagon;
using TPool;
using UnityEngine;

public class SelectionContainer : PoolBehaviour<PolyID>
{
    public PolyID Identity => m_PoolID;
    public readonly List<(Vector3 position,HexCoord _vertex)> m_RelativeCornerDirections=new List<(Vector3 position, HexCoord _vertex)>();
    public MeshCollider m_Collider { get; private set; }
    public override void OnPoolInit(Action<PolyID> _DoRecycle)
    {
        base.OnPoolInit(_DoRecycle);
        m_Collider = GetComponent<MeshCollider>();
    }

    public void Init(ICorner _corner,Mesh _vertexMesh)
    {
        var vertex = _corner.Vertex;
        _vertexMesh.name = $"GridVertex: {vertex.m_Identity}";
        transform.position = _corner.ToCornerPosition();
        m_Collider.sharedMesh = _vertexMesh;
        
        m_RelativeCornerDirections.Clear();
        foreach (var tuple in vertex.m_NearbyQuads.LoopIndex())
        {
            var quad = tuple.value;
            var index = tuple.index;
            var indexes = vertex.GetQuadVertsCW(index);
            var destIndex = indexes[1];
            var srcIndex = indexes[0];
            var center = (( quad.m_CoordQuad[destIndex]-quad.m_CoordQuad[srcIndex]) / 2).ToPosition();
            m_RelativeCornerDirections.Add((center,quad.m_HexQuad[destIndex]));
        }
    }
        
    public PolyID ValidateRaycast(ref RaycastHit _hit)
    {
        if (Vector3.Dot(_hit.normal, Vector3.up) > .95f)
            return m_PoolID.Upward();
        if (Vector3.Dot(_hit.normal, Vector3.down) > .95f)
            return m_PoolID.Downward();
        
        var localPoint = transform.InverseTransformPoint(_hit.point);
        float minSqrDistance = float.MaxValue;
        (Vector3 position, HexCoord vertex) destCorner = default;
        foreach (var tuple in m_RelativeCornerDirections)
        {
            var sqrDistance = (localPoint - tuple.position).sqrMagnitude;
            if (minSqrDistance < sqrDistance)
                continue;
            minSqrDistance = sqrDistance;
            destCorner = tuple;
        }
        return new PolyID(destCorner.vertex,m_PoolID.height);
    }

}
