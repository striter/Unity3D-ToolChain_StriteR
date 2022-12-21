using System.Collections.Generic;
using Geometry.PointSet;
using UnityEngine;

public class TileGraph:IGraph<Int2>,IGraphDiscrete<Int2>
{
    public float m_Size;

    public TileGraph(float _size)
    {
        m_Size = _size;
    }

    public Int2 GetNode(Vector3 _srcPosition) => new Int2(Mathf.FloorToInt(_srcPosition.x/m_Size), Mathf.FloorToInt(_srcPosition.z/m_Size));
    public IEnumerable<Int2> GetAdjacentNodes(Int2 _src)
    {
        yield return new Int2(_src.x - 1, _src.y);
        yield return new Int2(_src.x, _src.y - 1);
        yield return new Int2(_src.x + 1, _src.y);
        yield return new Int2(_src.x, _src.y + 1);
        yield return new Int2(_src.x + 1, _src.y - 1);
        yield return new Int2(_src.x + 1, _src.y + 1);
        yield return new Int2(_src.x - 1, _src.y + 1);
        yield return new Int2(_src.x - 1, _src.y - 1);
    }
        
    public void DrawGizmos(Int2 _node)
    {
        var bounds = UBoundsIncrement.MinMax(new Vector3(_node.x,0f,_node.y)*m_Size,new Vector3(_node.x+1,0f,_node.y+1)*m_Size);
        Gizmos.DrawWireCube(bounds.center,bounds.size);
    }
}