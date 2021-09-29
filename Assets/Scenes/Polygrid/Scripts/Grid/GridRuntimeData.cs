using System;
using System.Collections;
using System.Collections.Generic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace PolyGrid
{
    [Serializable]
    public struct GridVertexData
    {
        public HexCoord identity;
        public Coord coord;
    }
    
    [Serializable]
    public struct GridAreaData
    {
        public HexagonArea identity;
        public GridVertexData[] m_Vertices;
        public HexQuad[] m_Quads;
    }

    public class GridRuntimeData : ScriptableObject
    {
        public GridAreaData[] areaData;
    }
}
