using System;
using MeshFragment;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    public class ModuleCollection : ScriptableObject
    {
        public ModuleData[] m_ModuleLibrary;
        public Mesh[] m_MeshLibrary;
        public Material[] m_MaterialLibrary;
        public ModuleData this[int _type]
        {
            get
            {
                if (_type >= m_ModuleLibrary.Length)
                    throw new ArgumentException();
                return m_ModuleLibrary[_type];
            }
        }
    }

}