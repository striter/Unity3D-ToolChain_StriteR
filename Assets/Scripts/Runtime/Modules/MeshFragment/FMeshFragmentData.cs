using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using UnityEngine;

namespace MeshFragment
{

    public interface IMeshFragment
    {
        public int embedMaterial { get; }
        public IList<Vector3> vertices { get; }
        public IList<Vector2> uvs { get; }
        public IList<Vector3> normals { get; }
        public IList<Vector4> tangents { get; }
        public IList<Color> colors { get; }
        public IList<int> indexes { get; }
    }
    
    [Serializable]
    public struct FMeshFragmentData:IMeshFragment
    {
        public int embedMaterial;
        public Vector3[] vertices;
        public Vector2[] uvs;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Color[] colors;
        public int[] indexes;

        int IMeshFragment.embedMaterial => embedMaterial;
        IList<Vector3> IMeshFragment.vertices => vertices;
        IList<Vector2> IMeshFragment.uvs => uvs;
        IList<Vector3> IMeshFragment.normals => normals;
        IList<Vector4> IMeshFragment.tangents => tangents;
        IList<Color> IMeshFragment.colors => colors;
        IList<int> IMeshFragment.indexes => indexes;
    }

    public class FMeshFragmentObject:IMeshFragment,IPoolClass     //Non Serializable Version, For object pool
    {
        public int m_EmbedMaterial;
        private readonly  List<Vector3> m_Vertices = new List<Vector3>();
        private readonly  List<Vector2> m_UVs=new List<Vector2>();
        private readonly  List<Vector3> m_Normals=new List<Vector3>();
        private readonly  List<Vector4> m_Tangents=new List<Vector4>();
        private readonly  List<Color> m_Colors=new List<Color>();
        private readonly List<int> m_Indexes=new List<int>();
        
        int IMeshFragment.embedMaterial => m_EmbedMaterial;
        public IList<Vector3> vertices => m_Vertices;
        public IList<Vector2> uvs => m_UVs;
        public IList<Vector3> normals => m_Normals;
        public IList<Vector4> tangents => m_Tangents;
        public IList<Color> colors => m_Colors;
        public IList<int> indexes => m_Indexes;
        public void OnPoolCreate()
        {
        }

        public void OnPoolInitialize()
        {
        }

        public FMeshFragmentObject Initialize(int _embedMaterial)
        {
            m_EmbedMaterial = _embedMaterial;
            return this;
        }
        
        public void OnPoolRecycle()
        {
            m_Vertices.Clear();
            m_UVs.Clear();
            m_Normals.Clear();
            m_Tangents.Clear();
            m_Colors.Clear();
            m_Indexes.Clear();
        }
    }
}