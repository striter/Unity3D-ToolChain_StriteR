using System.Collections.Generic;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TheVoxel
{
    public class ChunkVoxel
    {
        public EVoxelType m_Type { get; private set; }
        public Int3 m_Identity { get; private set; }
        public bool[] m_SideGeometry { get; private set; } = new bool[6];
        public int m_SideCount { get; private set; } = 0;
        public ChunkVoxel(){}

        public ChunkVoxel Init(Int3 _identity,EVoxelType _type)
        {
            m_Identity = _identity;
            m_Type = _type;
            m_SideCount = 0;
            return this;
        }

        public void Refresh(Dictionary<Int3,ChunkVoxel> _voxels)
        {
            m_SideCount = 0;
            for (int i = 0; i < 6; i++)
            {
                bool sideValid =  !_voxels.ContainsKey(m_Identity + UCubeFacing.GetCubeOffset(UCubeFacing.IndexToFacing(i)));
                m_SideGeometry[i] = sideValid;
                m_SideCount += sideValid?1:0;
            }
        }
    }
}