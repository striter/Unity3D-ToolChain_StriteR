
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

        public void Tick(float _deltaTime)
        {
            foreach (var chunk in m_Chunks)
                chunk.Tick(_deltaTime,m_Chunks.m_Dic);
        }
        
        public void ChunkValidate(Vector3 _position)
        {
            Int2 centerChunk = DVoxel.GetChunkID(_position);
            TSPoolHashset<Int2>.Spawn(out var currentChunkList);
            TSPoolHashset<Int2>.Spawn(out var removeChunkList);
            UTile.GetAxisRange(centerChunk, DVoxel.kVisualizeRange).FillHashset(currentChunkList);
            foreach (var chunk in currentChunkList)
            {
                if(m_Chunks.Contains(chunk))
                    continue;

                m_Chunks.Spawn(chunk);
            }

            foreach (var chunk in m_Chunks.m_Dic.Keys)
            {
                if (currentChunkList.Contains(chunk))
                    continue;
                removeChunkList.Add(chunk);
            }

            foreach (var chunk in removeChunkList)
                m_Chunks.Recycle(chunk);

            TSPoolHashset<Int2>.Recycle(currentChunkList);
            TSPoolHashset<Int2>.Recycle(removeChunkList);
        }
    }
    
}