using System;
using MeshFragment;
using UnityEngine;

namespace TechToys.ThePlanet.Simplex
{
    public class SimplexCollection : ScriptableObject
    {
        public SimplexData[] m_SimplexData;
        public Material[] m_MaterialLibrary;
    }

    [Serializable]
    public class SimplexData
    {
        public string m_Name;
        public FMeshFragmentCluster[] m_ModuleData;
    }
}