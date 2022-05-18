using UnityEngine;

namespace Geometry.Polygon
{
    public static class UGeometryPolygon 
    {
        public static GTrianglePolygon[] GetPolygons(int[] _indices)
        {
            GTrianglePolygon[] polygons = new GTrianglePolygon[_indices.Length / 3];
            for (int i = 0; i < polygons.Length; i++)
            {
                int startIndex = i * 3;
                int triangle0 = _indices[startIndex];
                int triangle1 = _indices[startIndex + 1];
                int triangle2 = _indices[startIndex + 2];
                polygons[i] = new GTrianglePolygon(triangle0, triangle1, triangle2);
            }
            return polygons;
        }
    
        public static GTrianglePolygon[] GetPolygons(this Mesh _srcMesh, out int[] _indices)
        {
            _indices = _srcMesh.triangles;
            return GetPolygons(_indices);
        }
    }

}