using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry.Pixel
{
    public static class UGeometryPixel
    {
        public static G2Quad ConvertToG2Quad(this GQuad _quad, Func<Vector3,Vector2> _convert)
        {
            return new G2Quad(_convert(_quad.vB),_convert(_quad.vL),_convert(_quad.vF),_convert(_quad.vR));
        }
        public static GQuad ConvertToGQuad(this G2Quad _quad, Func<Vector2,Vector3> _convert)
        {
            return new GQuad(_convert(_quad.vB),_convert(_quad.vL),_convert(_quad.vF),_convert(_quad.vR));
        }
    }
}
