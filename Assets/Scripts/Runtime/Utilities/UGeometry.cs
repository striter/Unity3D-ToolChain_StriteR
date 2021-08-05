using UnityEngine;
public static class UGeometry
{
    #region Line&Ray
    public static Vector3 RayRayProjection(GRay _ray1,GRay _ray2)
    {
        Vector3 diff = _ray1.origin - _ray2.origin;
        float a01 = -Vector3.Dot(_ray1.direction, _ray2.direction);
        float b0 = Vector3.Dot(diff, _ray1.direction);
        float b1 = -Vector3.Dot(diff, _ray2.direction);
        float det = 1f - a01 * a01;
        return new Vector2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
    }
    public static Vector2 LineRayProjection(GLine _line, GRay _ray)
    {
        Vector2 projections = RayRayProjection(_line, _ray);
        projections.x = Mathf.Clamp(projections.x, 0, _line.length);
        projections.y = PointRayProjection(_line.GetPoint(projections.x), _ray);
        return projections;
    }
    #endregion
    #region Point
    public static float PointRayProjection(Vector3 _point,GRay _ray)
    {
        return Vector3.Dot(_point- _ray.origin, _ray.direction);
    }
    public static float PointPlaneDistance(Vector3 _point, GPlane _plane)
    {
        float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z + _plane.distance;
        return nr / _plane.normal.magnitude;
    }
    #endregion
    #region Ray
    public static bool RayTriangleIntersect(GTriangle _triangle, GRay _ray, bool _rayDirectionCheck) => RayTriangleIntersect(_triangle, _ray, _rayDirectionCheck, out float distance);
    
    public static bool RayTriangleIntersect(GTriangle _triangle,GRay _ray,bool _rayDirectionCheck,out float distance)
    {
        if (!RayTriangleCalculate(_triangle[0], _triangle[1], _triangle[2], _ray.origin, _ray.direction, out distance, out float u, out float v))
            return false;
        return !_rayDirectionCheck || distance > 0;
    }
    
    public static bool RayDirectedTriangleIntersect(GDirectedTriangle _triangle, GRay _ray, bool _rayDirectionCheck, bool _triangleDirectionCheck) => RayDirectedTriangleIntersect(_triangle,_ray,_rayDirectionCheck,_triangleDirectionCheck,out float distance);
    
    public static bool RayDirectedTriangleIntersect(GDirectedTriangle _triangle, GRay _ray, bool _rayDirectionCheck, bool _triangleDirectionCheck,out float distance)
    {
        if (!RayTriangleCalculate(_triangle[0], _triangle[1], _triangle[2], _ray.origin, _ray.direction, out distance, out float u, out float v))
            return false;
        bool intersect = true;
        intersect &= !_rayDirectionCheck || distance > 0;
        intersect &= !_triangleDirectionCheck || Vector3.Dot(_triangle.normal, _ray.direction) < 0;
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

    public static bool RayPlaneDistance(GPlane _plane, Ray _ray,out float distance)
    {
        distance = RayPlaneDistance(_plane, _ray);
        return distance!=0;
    }
    
    public static bool RayPlaneDistance(GPlane _plane, Ray _ray,out Vector3 _hitPoint)
    {
        float distance = RayPlaneDistance(_plane, _ray);
        _hitPoint = _ray.GetPoint(distance);
        return distance!=0;
    }
    
    public static float RayPlaneDistance(GPlane _plane,Ray _ray)
    {
        float nrO = Vector3.Dot(_plane.normal, _ray.origin);
        float nrD = Vector3.Dot(_plane.normal, _ray.direction);
        return (_plane.distance - nrO) / nrD;
    }
    
    public static bool RayBSIntersect(GSphere _sphere, GRay _ray)
    {
        RayBSCalculate(_sphere,_ray, out float dotOffsetDirection, out float discriminant);
        return discriminant >= 0;
    }
    
    public static Vector2 RayBSDistance(GSphere _sphere, GRay _ray)
    {
        RayBSCalculate(_sphere,_ray, out float dotOffsetDirection, out float discriminant);
        if (discriminant < 0)
            return Vector2.one * -1;

        discriminant = Mathf.Sqrt(discriminant);
        float t0 = -dotOffsetDirection - discriminant;
        float t1 = -dotOffsetDirection + discriminant;
        if (t0 < 0)
            t0 = t1;
        return new Vector2(t0, t1);
    }
    static void RayBSCalculate(GSphere _sphere, GRay _ray, out float dotOffsetDirection, out float discriminant)
    {
        Vector3 offset = _ray.origin - _sphere.center;
        dotOffsetDirection = Vector3.Dot(_ray.direction, offset);
        float sqrRadius = _sphere.radius * _sphere.radius;
        float radiusDelta = Vector3.Dot(offset, offset) - sqrRadius;
        discriminant = -1;
        if (dotOffsetDirection > 0 && radiusDelta > 0)
            return;

        float dotOffset = Vector3.Dot(offset, offset);
        discriminant = dotOffsetDirection * dotOffsetDirection - dotOffset + sqrRadius;
    }

    public static bool RayAABBIntersect(GBox _box,GRay _ray)
    {
        RayAABBCalculate(_box,_ray, out Vector3 tmin, out Vector3 tmax);
        return tmin.Max() <= tmax.Min();
    }
    public static Vector2 RayAABBDistance(GBox _box, GRay _ray)
    {
        RayAABBCalculate(_box,_ray, out Vector3 tmin, out Vector3 tmax);
        float dstA = Mathf.Max(Mathf.Max(tmin.x, tmin.y), tmin.z);
        float dstB = Mathf.Min(tmax.x, Mathf.Min(tmax.y, tmax.z));
        float dstToBox = Mathf.Max(0, dstA);
        float dstInsideBox = Mathf.Max(0, dstB - dstToBox);
        return new Vector2(dstToBox, dstInsideBox);
    }
    static void RayAABBCalculate(GBox _box, GRay _ray, out Vector3 _tmin, out Vector3 _tmax)
    {
        Vector3 invRayDir = Vector3.one.Divide(_ray.direction);
        Vector3 t0 = (_box.Min - _ray.origin).Multiply(invRayDir);
        Vector3 t1 = (_box.Max - _ray.origin).Multiply(invRayDir);
        _tmin = Vector3.Min(t0, t1);
        _tmax = Vector3.Max(t0, t1);
    }
    public static Vector2 RayConeDistance(GCone _cone, GRay _ray)
    {
        Vector2 distances = RayConeCalculate(_cone, _ray);
        if (Vector3.Dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin) < 0)
            distances.x = -1;
        if (Vector3.Dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin) < 0)
            distances.y = -1;
        return distances;
    }
    public static Vector2 RayConeDistance(GHeightCone _cone, GRay _ray)
    {
        Vector2 distances = RayConeCalculate(_cone, _ray);
        GPlane bottomPlane = new GPlane(_cone.normal, _cone.origin + _cone.normal * _cone.height);
        float rayPlaneDistance = RayPlaneDistance(bottomPlane, _ray);
        float sqrRadius = _cone.Radius;
        sqrRadius *= sqrRadius;
        if ((_cone.Bottom - _ray.GetPoint(rayPlaneDistance)).sqrMagnitude > sqrRadius)
            rayPlaneDistance = -1;

        float surfaceDst = Vector3.Dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin);
        if (surfaceDst<0|| surfaceDst > _cone.height)
            distances.x = rayPlaneDistance;

        surfaceDst = Vector3.Dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin) ;
        if (surfaceDst<0||surfaceDst > _cone.height)
            distances.y = rayPlaneDistance;
        return distances;
    }

    static Vector2 RayConeCalculate(GCone _cone, GRay _ray)
    {
        Vector2 distances = Vector2.one * -1;
        Vector3 offset = _ray.origin - _cone.origin;

        float RDV = Vector3.Dot(_ray.direction, _cone.normal);
        float ODN = Vector3.Dot(offset, _cone.normal);
        float cosA = Mathf.Cos(UMath.AngleToRadin(_cone.angle));
        float sqrCosA = cosA * cosA;

        float a = RDV * RDV - sqrCosA;
        float b = 2f * (RDV * ODN - Vector3.Dot(_ray.direction, offset) * sqrCosA);
        float c = ODN * ODN - Vector3.Dot(offset, offset) * sqrCosA;
        float determination = b * b - 4f * a * c;
        if (determination < 0)
            return distances;
        determination = Mathf.Sqrt(determination);
        distances.x = (-b + determination) / (2f * a);
        distances.y = (-b - determination) / (2f * a);
        return distances;
    }
    #endregion
    public static Matrix4x4 GetMirrorMatrix(this GPlane _plane)
    {
        Matrix4x4 mirrorMatrix = Matrix4x4.identity;
        mirrorMatrix.m00 = 1 - 2 * _plane.normal.x * _plane.normal.x;
        mirrorMatrix.m01 = -2 * _plane.normal.x * _plane.normal.y;
        mirrorMatrix.m02 = -2 * _plane.normal.x * _plane.normal.z;
        mirrorMatrix.m03 = 2 * _plane.normal.x * _plane.distance;
        mirrorMatrix.m10 = -2 * _plane.normal.x * _plane.normal.y;
        mirrorMatrix.m11 = 1 - 2 * _plane.normal.y * _plane.normal.y;
        mirrorMatrix.m12 = -2 * _plane.normal.y * _plane.normal.z;
        mirrorMatrix.m13 = 2 * _plane.normal.y * _plane.distance;
        mirrorMatrix.m20 = -2 * _plane.normal.x * _plane.normal.z;
        mirrorMatrix.m21 = -2 * _plane.normal.y * _plane.normal.z;
        mirrorMatrix.m22 = 1 - 2 * _plane.normal.z * _plane.normal.z;
        mirrorMatrix.m23 = 2 * _plane.normal.z * _plane.distance;
        mirrorMatrix.m30 = 0;
        mirrorMatrix.m31 = 0;
        mirrorMatrix.m32 = 0;
        mirrorMatrix.m33 = 1;
        return mirrorMatrix;
    }
}
