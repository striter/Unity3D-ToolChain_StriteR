using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshFragment
{
    [Serializable]
    public struct MeshFragmentData
    {
        public int embedMaterial;
        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] indexes;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Color[] colors;
    }
}