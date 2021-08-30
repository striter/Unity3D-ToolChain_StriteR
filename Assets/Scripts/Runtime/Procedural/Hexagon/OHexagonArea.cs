using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Procedural.Hexagon.Area
{
    public class HexagonArea
    {
        public PHexCube centerCS;
        public PHexCube coord;
        public int RadiusAS => UHexagonArea.radius;
        public int RadiusCS => UHexagonArea.radius*UHexagonArea.tilling;
    }
}