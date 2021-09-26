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
    public class ModuleRuntimeData : ScriptableObject
    {
        public EModuleType m_Type;
        public bool m_Top;
        public bool m_Bottom;
        public OrientedModuleMeshData[] m_OrientedMeshes;
    }
}