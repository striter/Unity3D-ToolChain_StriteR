using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace ConvexGrid
{
    [Serializable]
    public struct ModuleData:IQube<int>
    {
        public byte identity;
        public int DB;
        public int DL;
        public int DF;
        public int DR;
        public int TB;
        public int TL;
        public int TF;
        public int TR;

        public int vertDB { get=>DB; set=>DB=value; }
        public int vertDL { get=>DL; set=>DL=value; }
        public int vertDF { get=>DF; set=>DF=value; }
        public int vertDR { get=>DR; set=>DR=value; }
        public int vertTB { get=>TB; set=>TB=value; }
        public int vertTL { get=>TL; set=>TL=value; }
        public int vertTF { get=>TF; set=>TF=value; }
        public int vertTR { get=>TR; set=>TR=value; }
        public int this[int _index] => this.GetVertex<ModuleData, int>(_index);
        public int this[EQubeCorner _corner] => this.GetVertex<ModuleData, int>(_corner);
    }

    [Serializable]
    public struct ModuleMesh
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
        GQuad m_OrientedShapeOS { get; }
    }
}