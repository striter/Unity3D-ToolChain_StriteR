using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode<T> where T:struct
{
    public T identity { get; }
}

public interface IGraph<T> where T:struct
{
    IEnumerable<T> GetAdjacentNodes(T _src);
}

public interface IGraphDiscrete<T> where T : struct
{
    T GetNode(Vector3 _srcPosition);
    Vector3 ToNodePosition(T _node);
}

public interface IGraphPathFinding<T> where T:struct
{
    float Heuristic(T _src, T _dst);
    float Cost(T _src, T _dst);
}