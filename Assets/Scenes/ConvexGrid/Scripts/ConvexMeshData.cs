using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ConvexGrid
{
    [Serializable]
    //[CreateAssetMenu(menuName = "Module/Data")]
    public class ConvexMeshData : ScriptableObject
    {
        public ModuleData[] m_ModuleData;
        public ModuleMesh[] m_ModuleMeshes;
    }
}