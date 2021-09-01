using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Procedural.Hexagon.Area
{
    public class HexagonArea
    {
        public HexagonCoordC centerCS;
        public HexagonCoordC m_Coord;
        public int RadiusAS => UHexagonArea.radius;
        public int RadiusCS => UHexagonArea.radius*UHexagonArea.tilling;
    }
}