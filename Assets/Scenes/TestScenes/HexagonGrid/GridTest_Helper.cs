using System;
using UnityEngine;
using Procedural.Hexagon;
using System.Linq;
using Procedural;
namespace GridTest
{
    public static class GridHelper
    {
        public static Vector3 ToWorld(this HexCoord _hexCube)
        {
            return _hexCube.ToCoord().ToPosition();
        }
#if UNITY_EDITOR
        public static void DrawHexagon(this HexCoord _coord)
        {
            Vector3[] hexagonList = UHexagon.GetHexagonPoints().Select(p=>p.ToPosition() + _coord.ToWorld()).ToArray();
            Gizmos_Extend.DrawLinesConcat(hexagonList);
        }
#endif
    }
}