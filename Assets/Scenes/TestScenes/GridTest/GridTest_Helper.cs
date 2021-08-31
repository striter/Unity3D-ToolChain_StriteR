using System;
using UnityEngine;
using Procedural.Hexagon;
using System.Linq;
using Procedural;
namespace GridTest
{
    public static class GridHelper
    {
        public static Vector3 ToWorld(this PHexAxial _axial)
        {
            return _axial.ToPixel().ToWorld();
        }

        public static Vector3 ToWorld(this PHexCube _hexCube)
        {
            return _hexCube.ToAxial().ToWorld();
        }
        public static Vector3 ToWorld(this Coord _pixel)
        {
            return new Vector3(_pixel.x,0,_pixel.y);
        }

        public static Coord ToPixel(this Vector3 _world)
        {
            return new Coord(_world.x,  _world.z);
        }
        
#if UNITY_EDITOR
        public static void DrawHexagon(this PHexAxial _axial)
        {
            Vector3[] hexagonList = UHexagon.GetHexagonPoints().Select(p=>p.ToWorld() + _axial.ToPixel().ToWorld()).ToArray();
            Gizmos_Extend.DrawLines(hexagonList);
        }

        public static void DrawHexagon(this PHexCube _axial) => _axial.ToAxial().DrawHexagon();
#endif
    }
}