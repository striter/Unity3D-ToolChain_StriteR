using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Two
{
    public static class UGeometry2D
    {
        public static bool IsPointInside(this G2Quad _quad,Vector2 _point)
        {
            ref var A = ref _quad.vertices[0];
            ref var B = ref _quad.vertices[1];
            ref var C = ref _quad.vertices[2];
            ref var D = ref _quad.vertices[3];
            ref var x = ref _point.x;
            ref var y = ref _point.y;
            int a = (int)Mathf.Sign((B.x - A.x) * (y - A.y) - (B.y - A.y) * (x - A.x));
            int b = (int)Mathf.Sign((C.x - B.x) * (y - B.y) - (C.y - B.y) * (x - B.x));
            int c = (int)Mathf.Sign((D.x - C.x) * (y - C.y) - (D.y - C.y) * (x - C.x));
            int d = (int)Mathf.Sign((A.x - D.x) * (y - D.y) - (A.y - D.y) * (x - D.x));
            return Mathf.Abs( a + b + c + d) == 4;
        }

        public static int NearestPointIndex(this G2Quad _quad, Vector2 _point)
        {
            float minDistance = float.MaxValue;
            int minIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                var sqrDistance = Vector2.SqrMagnitude(_point - _quad[i]);
                if(minDistance<sqrDistance)
                    continue;
                minIndex = i;
                minDistance = sqrDistance;
            }
            return minIndex;
        }
    }
}