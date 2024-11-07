
using System;
using System.Linq;
using System.Linq.Extensions;
using Procedural.Tile;
using TPool;
using TObjectPool;
using UnityEngine;

namespace TheVoxel
{
    [Serializable]
    public struct TerrainData
    {
        [Header("Execution")]
        public int visualRange;
        public int iterationPerTick;

        [Header("Terrain")]
        public int baseHeight;
        public float formScale;
        public float mountainValidation;

        [Header("Plane")] 
        public float planeScale;
        public RangeInt planeHeight;
        public RangeInt dirtRandom;
        
        [Header("Mountain")] 
        public float mountainScale;
        public RangeInt mountainHeight;
        public int mountainForm;
        
        [Header("Cave")] 
        public float caveScale;
        public float caveValidation;
    }
    
    public class ChunkManager : MonoBehaviour
    {
        public static TerrainData Instance;
        public TerrainData m_TerrainData;
        
        private ObjectPoolBehaviour<Int2, ChunkElement> m_Chunks;

        public void Init()
        {
            Instance = m_TerrainData;
            m_Chunks = new ObjectPoolBehaviour<Int2, ChunkElement>(transform.Find("Element"));
        }

        private void OnValidate()
        {
            if (m_Chunks == null)
                return;
            Instance = m_TerrainData;
            m_Chunks.Clear();
        }

        public void Dispose()
        {
            m_Chunks.Dispose();
        }

        public void Tick(float _deltaTime)
        {
            var validIteration = m_TerrainData.iterationPerTick;
            while (validIteration >= 0)
            {
                if (m_Chunks.Any(chunk => chunk.Tick(_deltaTime, m_Chunks.m_Dic)))
                    validIteration--;
                else
                    break;
            }
        }
        
        public void ChunkValidate(Vector3 _position)
        {
            Int2 centerChunk = DVoxel.GetChunkID(_position);
            TSPoolHashset<Int2>.Spawn(out var currentChunkList);
            TSPoolHashset<Int2>.Spawn(out var removeChunkList);
            UTile.GetAxisRange(centerChunk, m_TerrainData.visualRange).FillHashset(currentChunkList);

            foreach (var chunk in m_Chunks.m_Dic.Keys)
            {
                if (currentChunkList.Contains(chunk))
                    continue;
                removeChunkList.Add(chunk);
            }

            foreach (var chunk in removeChunkList)
                m_Chunks.Recycle(chunk);
            
            foreach (var chunk in currentChunkList)
            {
                if(m_Chunks.Contains(chunk))
                    continue;

                m_Chunks.Spawn(chunk);
            }

            TSPoolHashset<Int2>.Recycle(currentChunkList);
            TSPoolHashset<Int2>.Recycle(removeChunkList);
        }
    }
    
}