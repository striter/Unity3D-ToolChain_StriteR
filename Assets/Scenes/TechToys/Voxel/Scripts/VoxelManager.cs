using System;
using TPool;
using UnityEngine;

namespace TheVoxel
{
    public class VoxelManager : MonoBehaviour
    {
        private ChunkManager m_Chunk;
        private void OnEnable()
        {
            m_Chunk = GetComponentInChildren<ChunkManager>();
            m_Chunk.Init();
        }

        private void OnDisable()
        {
            m_Chunk.Dispose();
        }

        void Update()
        {
            m_Chunk.Tick(Time.unscaledDeltaTime);
            #if UNITY_EDITOR
                m_Chunk.ChunkValidate((UnityEditor.SceneView.sceneViews[0] as UnityEditor.SceneView).camera.transform.position);
            #endif
        }
    }
}