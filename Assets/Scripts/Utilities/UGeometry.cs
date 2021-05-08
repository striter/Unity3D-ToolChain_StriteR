using UnityEngine;

public static class UBoundsChecker
{
    static Vector3 m_BoundsMin;
    static Vector3 m_BoundsMax;
    public static void Begin()
    {
        m_BoundsMin = Vector3.zero;
        m_BoundsMax = Vector3.zero;
    }
    public static void CheckBounds(Vector3 vertex)
    {
        m_BoundsMin = Vector3.Min(m_BoundsMin, vertex);
        m_BoundsMax = Vector3.Max(m_BoundsMax, vertex);
    }
    public static Bounds CalculateBounds() => new Bounds((m_BoundsMin + m_BoundsMax) / 2, m_BoundsMax - m_BoundsMin);

    public static Bounds GetBounds(Vector3[] _verticies)
    {
        Begin();
        foreach (var vertex in _verticies)
            CheckBounds(vertex);
        return CalculateBounds();
    }
}

public static class UGeometry
{
    public static bool RayTriangleIntersect(Triangle _triangle, Ray _ray, bool _rayDirectionCheck) => RayTriangleIntersect(_triangle, _ray, _rayDirectionCheck, out float distance);
    public static bool RayTriangleIntersect(Triangle _triangle,Ray _ray,bool _rayDirectionCheck,out float distance)
    {
        if (!RayTriangleCalculate(_triangle[0], _triangle[1], _triangle[2], _ray.origin, _ray.direction, out distance, out float u, out float v))
            return false;
        return !_rayDirectionCheck || distance > 0;
    }
    public static bool RayDirectedTriangleIntersect(DirectedTriangle _triangle, Ray _ray, bool _rayDirectionCheck, bool _triangleDirectionCheck) => RayDirectedTriangleIntersect(_triangle,_ray,_rayDirectionCheck,_triangleDirectionCheck,out float distance);
    public static bool RayDirectedTriangleIntersect(DirectedTriangle _triangle, Ray _ray, bool _rayDirectionCheck, bool _triangleDirectionCheck,out float distance)
    {
        if (!RayTriangleCalculate(_triangle[0], _triangle[1], _triangle[2], _ray.origin, _ray.direction, out distance, out float u, out float v))
            return false;
        bool intersect = true;
        intersect &= !_rayDirectionCheck || distance > 0;
        intersect &= !_triangleDirectionCheck || Vector3.Dot(_triangle.m_Normal, _ray.direction) < 0;
        return intersect;
    }
    static bool RayTriangleCalculate(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2, Vector3 _rayOrigin, Vector3 _rayDir,out float t,out float u,out float v)  //Möller-Trumbore
    {
        t = 0;
        u = 0;
        v = 0;
        Vector3 E1 = _vertex1 - _vertex0;
        Vector3 E2 = _vertex2 - _vertex0;
        Vector3 P = Vector3.Cross(_rayDir, E2);
        float determination = Vector3.Dot(E1, P);
        Vector3 T;
        if (determination > 0)
        {
            T = _rayOrigin - _vertex0;
        }
        else
        {
            T = _vertex0 - _rayOrigin;
            determination = -determination;
        }

        if (determination < float.Epsilon)
            return false;

        u = Vector3.Dot(T, P);
        if (u < 0f || u > determination)
            return false;
        Vector3 Q = Vector3.Cross(T, E1);
        v = Vector3.Dot(_rayDir, Q);
        if (v < 0f || (u + v) > determination)
            return false;

        t = Vector3.Dot(E2, Q);
        float invDetermination = 1/ determination;
        t *= invDetermination;
        u *= invDetermination;
        v *= invDetermination;
        return true;
    }
    public static float RayPlaneDistance(Vector3 _pNormal, float _pDistance, Vector3 _rayOrigin, Vector3 _rayDirection) => RayPlaneDistance(new DistancePlane(_pNormal,_pDistance),new Ray(_rayOrigin,_rayDirection));
    public static float RayPlaneDistance(DistancePlane _plane,Ray _ray)
    {
        float nrO = Vector3.Dot(_plane.m_Normal, _ray.origin);
        float nrD = Vector3.Dot(_plane.m_Normal, _ray.direction);
        return (_plane.m_Distance - nrO) / nrD;
    }
    public static float PointPlaneDistance(Vector3 _point, Vector3 _normal, float _distance) => PointPlaneDistance(_point, new DistancePlane(_normal, _distance));
    public static float PointPlaneDistance(Vector3 _point,DistancePlane _plane)
    {
        float nr= _point.x*_plane.m_Normal.x + _point.y * _plane.m_Normal.y + _point.z * _plane.m_Normal.z+_plane.m_Distance;
        return nr / _plane.m_Normal.magnitude;
    }
    static void RayBSCalculate(Vector3 _bsCenter, float _bsRadius, Vector3 _rayOrigin, Vector3 _rayDirection, out float dotOffsetDirection, out float discriminant)
    {
        Vector3 offset = _rayOrigin - _bsCenter;
        dotOffsetDirection = Vector3.Dot(_rayDirection, offset);
        float sqrRadius = _bsRadius * _bsRadius;
        float radiusDelta = Vector3.Dot(offset, offset) - sqrRadius;
        discriminant = -1;
        if (dotOffsetDirection > 0 && radiusDelta > 0)
            return;

        float dotOffset = Vector3.Dot(offset, offset);
        discriminant = dotOffsetDirection * dotOffsetDirection - dotOffset + sqrRadius;
    }
    public static bool RayBSIntersect(Vector3 _bsCenter, float _bsRadius, Ray _ray) => RayBSIntersect(_bsCenter,_bsRadius,_ray.origin,_ray.direction);
    public static bool RayBSIntersect(Vector3 _bsCenter, float _bsRadius, Vector3 _rayOrigin, Vector3 _rayDirection)
    {
        RayBSCalculate(_bsCenter, _bsRadius, _rayOrigin, _rayDirection, out float dotOffsetDirection, out float discriminant);
        return discriminant >= 0;
    }
    public static Vector2 RayBSDistance(Vector3 _bsCenter, float _bsRadius, Vector3 _rayOrigin, Vector3 _rayDirection)
    {
        RayBSCalculate(_bsCenter, _bsRadius, _rayOrigin, _rayDirection, out float dotOffsetDirection, out float discriminant);
        if (discriminant < 0)
            return Vector2.one * -1;

        discriminant = Mathf.Sqrt(discriminant);
        float t0 = -dotOffsetDirection - discriminant;
        float t1 = -dotOffsetDirection + discriminant;
        if (t0 < 0)
            t0 = t1;
        return new Vector2(t0, t1 - t0);
    }
    static void RayAABBCalculate(Vector3 _boundsMin, Vector3 _boundsMax, Vector3 _rayOrigin, Vector3 _rayDir, out Vector3 _tmin, out Vector3 _tmax)
    {
        Vector3 invRayDir = Vector3.one.Divide(_rayDir);
        Vector3 t0 = (_boundsMin - _rayOrigin).Multiply(invRayDir);
        Vector3 t1 = (_boundsMax - _rayOrigin).Multiply(invRayDir);
        _tmin = Vector3.Min(t0, t1);
        _tmax = Vector3.Max(t0, t1);
    }
    public static bool RayAABBIntersect(Vector3 _boundsMin, Vector3 _boundsMax, Vector3 _rayOrigin, Vector3 _rayDir)
    {
        RayAABBCalculate(_boundsMin, _boundsMax, _rayOrigin, _rayDir, out Vector3 tmin, out Vector3 tmax);
        return tmin.Max() <= tmax.Min();
    }
    public static Vector2 RayAABBDistance(Vector3 _boundsMin, Vector3 _boundsMax, Vector3 _rayOrigin, Vector3 _rayDir)
    {
        RayAABBCalculate(_boundsMin, _boundsMax, _rayOrigin, _rayDir, out Vector3 tmin, out Vector3 tmax);
        float dstA = Mathf.Max(Mathf.Max(tmin.x, tmin.y), tmin.z);
        float dstB = Mathf.Min(tmax.x, Mathf.Min(tmax.y, tmax.z));
        float dstToBox = Mathf.Max(0, dstA);
        float dstInsideBox = Mathf.Max(0, dstB - dstToBox);
        return new Vector2(dstToBox, dstInsideBox);
    }

}
