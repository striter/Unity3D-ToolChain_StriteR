using System;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Optimize.Voxelizer
{
    [Serializable]
    public struct VoxelGrid
    {
        public bool[] voxels;

    }
    
    public class GeometryVoxelData : ScriptableObject
    {
        public GBox m_Bounds;
        public EResolution m_Resolution;
        public VoxelGrid[] m_Grids;

        public int2 Resolution => new int2((int) m_Resolution, (int) m_Resolution);
        private GBox GetBounds(int2 _index)
        {
            return new GBox();
        }
        
        public void DrawGizmos()
        {
            m_Bounds.DrawGizmos();
        }
    }
}