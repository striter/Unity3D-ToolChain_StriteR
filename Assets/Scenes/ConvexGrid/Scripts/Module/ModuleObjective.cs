using System;
using Geometry.Extend;
using Geometry.Pixel;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    [Serializable]
    public struct ModuleData
    {
        public byte identity;
        public IntQube modules;
        public IntQube orientations;
    }
    
    [Serializable]
    public struct OrientedModuleMeshData
    {
        public Vector3[] m_Vertices;
        public Vector2[] m_UVs;
        public int[] m_Indexes;
        public Vector3[] m_Normals;
    }
    public interface IModuleCollector
    { 
        Transform m_ModuleTransform { get; }
        PileID m_Identity { get; }
        byte m_ModuleByte { get; }
        BCubeFacing m_SideRelation { get; set; }
        G2Quad[] m_ModuleShapeLS { get; }
    }
}