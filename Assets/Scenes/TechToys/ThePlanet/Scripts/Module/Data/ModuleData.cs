using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using MeshFragment;
using TechToys.ThePlanet.Module.Cluster;
using TechToys.ThePlanet.Module.Prop;
using UnityEngine;

namespace  TechToys.ThePlanet.Module
{
    [Serializable]
    public class ModuleData : ScriptableObject
    {
        public EClusterType m_ClusterType;
        public ModuleClusterData[] m_ClusterData;
        public ModulePathData m_Paths;
        public ModuleDecorationCollection m_Decorations;
    }

    [Serializable]
    public struct ModuleClusterData
    {
        public ModuleClusterUnitData[] m_Units;
    }

    [Serializable]
    public struct ModuleClusterUnitData
    {
        public ModuleClusterUnitPossibilityData[] m_Possibilities;
    }

    [Serializable]
    public struct ModuleClusterUnitPossibilityData
    {
        public byte m_MixableReadMask;
        public FMeshFragmentCluster m_Mesh;
    }
    
    [Serializable]
    public struct ModulePathData
    {
        public FMeshFragmentCluster[] m_Units;
        public bool Available => m_Units != null && m_Units.Length > 0;
    }
    
}