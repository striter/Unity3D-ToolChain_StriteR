using UnityEngine;

namespace UHexagon
{
    public static class UHexagonConst
    {
        public static readonly float C_SQRT3 = Mathf.Sqrt(3);
        public static readonly float C_SQRT3Half = C_SQRT3/2f;
        public static readonly Vector2[] C_UnitHexagonPoints = new Vector2[6] {new Vector2(0,1),new Vector2(C_SQRT3Half, .5f),new Vector2(C_SQRT3Half,-.5f),new Vector2(0,-1),new Vector2(-C_SQRT3Half,-.5f),new Vector2(-C_SQRT3Half,.5f) }; 
    }
    
    public static class UHexagonHelper
    {
        public static Vector3 Axis2D(float radius, int xIndex, int yIndex)=>new Vector3(xIndex *UHexagonConst.C_SQRT3Half , 0, yIndex * 3 + (xIndex % 2) * 1.5f) * radius;
        public static Vector3[] GetHexagonPoints(Vector3 origin, float radius) => GetHexagonPoints(origin, radius, Vector3.forward, Vector3.up);
        public static Vector3[] GetHexagonPoints(Vector3 origin, float radius, Vector3 forward, Vector3 normal)
        {
            Vector3[] points = new Vector3[6];
            for (int i = 0; i < 6; i++)
                points[i] = origin + Quaternion.AngleAxis(60 * i, normal) * forward * radius;
            return points;
        }

    }
}