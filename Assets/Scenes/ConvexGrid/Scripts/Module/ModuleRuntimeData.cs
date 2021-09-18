using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ConvexGrid
{
    [Serializable]
    //[CreateAssetMenu(menuName = "Module/Data")]
    public class ModuleRuntimeData : ScriptableObject
    {
        public ModuleData[] m_ModuleData;
        public OrientedModuleMeshData[] m_OrientedMeshes;
    }
}