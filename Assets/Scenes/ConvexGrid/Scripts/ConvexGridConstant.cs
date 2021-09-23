using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConvexGrid
{
    public static class KConvexGrid
    {
        public const float tileHeight = 5f;
        public const float tileHeightHalf = tileHeight / 2f;
        public static readonly float tileDiagonalWidth = UMath.SQRT2 * 5;
        
        public static readonly Vector3 tileHeightVector = tileHeight * Vector3.up;
        public static readonly Vector3 tileHeightHalfVector = tileHeightHalf*Vector3.up;
    }
}