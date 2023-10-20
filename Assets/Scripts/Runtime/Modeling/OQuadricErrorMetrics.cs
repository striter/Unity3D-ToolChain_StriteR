using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace QuadricErrorsMetric
{
    public enum EContractMode
    {
        VertexCount,
        Percentage,
        DecreaseAmount,
    }

    [Serializable]
    public struct ContractConfigure
    {
        public EContractMode mode;
        [MFoldout(nameof(mode),EContractMode.Percentage)][Range(1f,100f)] public float percent;
        [MFoldout(nameof(mode),EContractMode.VertexCount , EContractMode.DecreaseAmount)]public int count;
    }


}

namespace QuadricErrorsMetric2
{
    
    internal class FatVertex
    {
        public float3 m_Position;
        public int m_FinalIndex;
        public bool m_Collapsed;
        public bool m_Checked;

        public List<int> vertexNeighbors = new List<int>();
        public List<int> triangleNeighbors = new List<int>();
    }

    internal class FatTriangle
    {
        public PTriangle m_Indexes;
        public GPlane m_Plane;
        public int m_FinalIndex;
        public bool m_Collapsed;

        public FatTriangle(PTriangle _triangle,FatVertex[] _vertices)
        {
            m_Indexes = _triangle;
            m_Plane = GPlane.FromPositions(_vertices[m_Indexes[0]].m_Position,
                _vertices[m_Indexes[1]].m_Position,
                _vertices[m_Indexes[2]].m_Position);
        }
        
        public bool ReplaceVertex(int _src,int _end,FatVertex[] _vertices)
        {
            if (!m_Indexes.IterateFindIndex(_src, out var index))
                return false;
            m_Indexes[index] = _end;
            m_Plane = GPlane.FromPositions(_vertices[m_Indexes[0]].m_Position,
                _vertices[m_Indexes[1]].m_Position,
                _vertices[m_Indexes[2]].m_Position);
            
            return true;
        }
    }


    internal class EdgeCollapse
    {
        public int m_Index1;
        public int m_Index2;
        public float3 m_Target;
        public float m_Cost;
        public int m_QueueIndex;

        public static bool operator ==(EdgeCollapse _src, EdgeCollapse _dst) => _src.m_Index1 == _dst.m_Index1 && _src.m_Index2 == _dst.m_Index2;
        public static bool operator !=(EdgeCollapse _src, EdgeCollapse _dst) => !(_src == _dst);
        public static bool operator >(EdgeCollapse _src, EdgeCollapse _dst) => _src.m_Cost > _dst.m_Cost;
        public static bool operator <(EdgeCollapse _src, EdgeCollapse _dst) => _src.m_Cost < _dst.m_Cost;
        public static bool operator >=(EdgeCollapse _src, EdgeCollapse _dst) => _src.m_Cost >= _dst.m_Cost;
        public static bool operator <=(EdgeCollapse _src, EdgeCollapse _dst) => _src.m_Cost <= _dst.m_Cost;
        protected bool Equals(EdgeCollapse other)
        {
            return m_Index1 == other.m_Index1 && m_Index2 == other.m_Index2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EdgeCollapse)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_Index1, m_Index2);
        }

    }

    internal class QEMVertex
    {
        public float4x4 m_Q;
        public List<EdgeCollapse> m_Collapses = new List<EdgeCollapse>();
    }
}