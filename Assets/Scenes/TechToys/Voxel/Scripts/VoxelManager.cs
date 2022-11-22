using System;
using TPool;
using UnityEngine;

namespace TheVoxel
{
    public class VoxelManager : MonoBehaviour
    {
        private ChunkManager m_Chunk;
        private void Awake()
        {
            m_Chunk = GetComponentInChildren<ChunkManager>();
            m_Chunk.Init();
        }

        void Update()
        {
            m_Chunk.ChunkValidate(Vector3.zero);
        }
    }
}