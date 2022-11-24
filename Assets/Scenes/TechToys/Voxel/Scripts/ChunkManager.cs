
using System;
using Procedural.Tile;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace TheVoxel
{
    public class ChunkManager : MonoBehaviour
    {
        private TObjectPoolMono<Int2, ChunkElement> m_Chunks;

        public void Init()
        {
            m_Chunks = new TObjectPoolMono<Int2, ChunkElement>(transform.Find("Element"));
        }

        public void Dispose()
        {
            m_Chunks.Dispose();
        }
        
        public void ChunkValidate(Vector3 _position)
        {
            Int2 centerChunk = DVoxel.GetChunkID(_position);
            TSPoolHashset<Int2>.Spawn(out var nearbyChunkList);
            TSPoolHashset<Int2>.Spawn(out var removeChunkList);
            UTile.GetAxisRange(centerChunk, DVoxel.kVisualizeRange).FillHashset(nearbyChunkList);
            foreach (var chunk in nearbyChunkList)
            {
                if(m_Chunks.Contains(chunk))
                    continue;

                m_Chunks.Spawn(chunk);
            }

            foreach (var chunk in m_Chunks.m_Dic.Keys)
            {
                if (nearbyChunkList.Contains(chunk))
                    continue;
                removeChunkList.Add(chunk);
            }

            foreach (var chunk in removeChunkList)
                m_Chunks.Recycle(chunk);

            TSPoolHashset<Int2>.Recycle(nearbyChunkList);
            TSPoolHashset<Int2>.Recycle(removeChunkList);
        }

        public void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            foreach (var chunk in m_Chunks)
                chunk.Tick(deltaTime,m_Chunks.m_Dic);
        }

    }
    
}