using System;
using System.Collections;
using System.Collections.Generic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace PCG
{
    [Serializable]
    public struct GridVertexData
    {
        public HexCoord identity;
        public Coord coord;
        public bool invalid;
    }
    
    [Serializable]
    public struct GridAreaData
    {
        public HexagonArea identity;
        public GridVertexData[] m_Vertices;
        public HexQuad[] m_Quads;
    }

    public class GridCollection : ScriptableObject
    {
        public GridAreaData[] areaData;
    }
}
