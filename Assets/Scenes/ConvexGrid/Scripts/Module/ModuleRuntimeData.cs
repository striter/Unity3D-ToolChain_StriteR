using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ConvexGrid
{
    [Serializable]
    public struct OrientedModuleMeshData
    {
        public Vector3[] m_Vertices;
        public Vector2[] m_UVs;
        public int[] m_Indexes;
        public Vector3[] m_Normals;
    }
    
    [Serializable]
    //[CreateAssetMenu(menuName = "Module/Data")]
    public class ModuleRuntimeData : ScriptableObject
    {
        public OrientedModuleMeshData[] m_OrientedMeshes;
    }
}