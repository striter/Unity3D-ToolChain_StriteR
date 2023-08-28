using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public interface IGraph<T> 
{
    IEnumerable<T> GetAdjacentNodes(T _src);
}

public interface IGraphDiscrete<T>
{
    T GetNode(float3 _srcPosition);
    Vector3 ToNodePosition(T _node);
}

public interface IGraphPathFinding<T>
{
    float Heuristic(T _src, T _dst);
    float Cost(T _src, T _dst);
}