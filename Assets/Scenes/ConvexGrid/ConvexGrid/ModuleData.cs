using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ConvexGrid
{
    [Serializable]
    //[CreateAssetMenu(menuName = "Module/Data")]
    public class ModuleData : ScriptableObject
    {
        public ModulePossibilityData[] m_ModulesData;
    }
}