using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public interface IGraph<Node> 
{
    IEnumerable<Node> GetAdjacentNodes(Node _src);
}

public interface IGraphFinite<Node> : IGraph<Node> , IEnumerable<Node>
{
    int Count { get; }
}

public interface IGraphMapping<Node>
{
    bool PositionToNode(float3 _position, out Node _node);
    bool NodeToPosition(Node _node,out float3 _position);
}

public interface IGraphPathFinding<Node> : IGraphFinite<Node> , IGraphMapping<Node>
{
    float Heuristic(Node _src, Node _dst);
    float Cost(Node _src, Node _dst);
}