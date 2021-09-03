using System;
using UnityEngine;
using Procedural.Hexagon;
using System.Linq;
using Procedural;
namespace GridTest
{
    public static class GridHelper
    {
        public static Vector3 ToWorld(this Coord _pixel)
        {
            return new Vector3(_pixel.x,0,_pixel.y);
        }

        public static Coord ToCoord(this Vector3 _world)
        {
            return new Coord(_world.x,  _world.z);
        }
        
        public static Vector3 ToWorld(this HexCoord _hexCube)
        {
            return _hexCube.ToPixel().ToWorld();
        }
#if UNITY_EDITOR
        public static void DrawHexagon(this HexCoord _coord)
        {
            Vector3[] hexagonList = UHexagon.GetHexagonPoints().Select(p=>p.ToWorld() + _coord.ToWorld()).ToArray();
            Gizmos_Extend.DrawLines(hexagonList);
        }
#endif
    }
}