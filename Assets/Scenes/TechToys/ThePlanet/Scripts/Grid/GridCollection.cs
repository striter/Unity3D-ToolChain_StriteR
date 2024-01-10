using System;
using Runtime.Geometry;
using UnityEngine;

namespace TechToys.ThePlanet
{
    [Serializable]
    public struct GridVertexData
    {
        public Vector3 position;
        public Vector3 normal;
        public bool invalid;
    }

    [Serializable]
    public struct GridQuadData
    {
        public PQuad vertices;
    }
    
    [Serializable]
    public struct GridChunkData
    {
        public GridQuadData[] quads;
    }

    public class GridCollection : ScriptableObject
    {
        public GridVertexData[] vertices;
        public GridChunkData[] chunks;
    }
}
