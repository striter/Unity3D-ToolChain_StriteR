using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Procedural.Hexagon.Area
{
    [Serializable]
    public class HexagonArea
    {
        public HexCoord coord;
        public HexCoord centerCS;
        public int RadiusAS => UHexagonArea.radius;
        public int RadiusCS => UHexagonArea.radius*UHexagonArea.tilling;
    }
}