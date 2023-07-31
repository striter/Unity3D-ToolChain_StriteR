using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INode<T>
{
    public T identity { get; }
}

public interface IGraph<T> 
{
    IEnumerable<T> GetAdjacentNodes(T _src);
}

public interface IGraphDiscrete<T>
{
    T GetNode(Vector3 _srcPosition);
    Vector3 ToNodePosition(T _node);
}

public interface IGraphPathFinding<T>
{
    float Heuristic(T _src, T _dst);
    float Cost(T _src, T _dst);
}