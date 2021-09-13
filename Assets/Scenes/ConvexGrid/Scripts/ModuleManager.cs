using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using TPool;
using UnityEngine;

namespace ConvexGrid
{
    [Serializable]
    public struct ModulePossibilityData
    {
        public byte m_Identity;
        public Vector3[] m_Vertices;
        public Vector2[] m_UVs;
        public int[] m_Indexes;
        public Vector3[] m_Normals;
    }
    
    public class ModuleManager : MonoBehaviour,IConvexGridControl
    {
        public ModuleData m_Data;
        public TObjectPoolMono<int, ModuleContainer> m_Containers;
        public void Init(Transform _transform)
        {
            m_Containers = new TObjectPoolMono<int, ModuleContainer>(_transform.Find("Modules/Container"));
        }

        public void Tick(float _deltaTime)
        {
        }

        public void OnSelectVertex(ConvexVertex _vertex, byte _height, bool _construct)
        {
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
        }

        public void Clear()
        {
            m_Containers.Clear();
            
        }
    }
}
