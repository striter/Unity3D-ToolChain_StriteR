//Ray
struct GRay
{
    float3 origin;
    float3 direction;
    float3 GetPoint(float _distance)  {  return origin + direction * _distance;  }
};
GRay GRay_Ctor(float3 _origin, float3 _direction)
{
    GRay ray;
    ray.origin = _origin;
    ray.direction = _direction;
    return ray;
}
struct GLine
{
    float3 origin;
    float3 direction;
    float length;
    float3 end;
    float3 GetPoint(float _distance)  { return origin + direction * _distance;  }
    GRay ToRay()
    {
        GRay ray;
        ray.origin = origin;
        ray.direction = direction;
        return ray;
    }
};
GLine GLine_Ctor(float3 _origin, float3 _direction, float _length)
{
    GLine gline;
    gline.origin = _origin;
    gline.direction = _direction;
    gline.length = _length;
    gline.end = _origin + _direction * _length;
    return gline;
}
//Plane
struct GPlane
{
    float3 normal;
    float distance;
    float3 position;
};
GPlane GPlane_Ctor(float3 _normal, float _distance)
{
    GPlane plane;
    plane.normal = _normal;
    plane.distance = _distance;
    plane.position=plane.normal*plane.distance;
    return plane;
}
struct GPlanePos
{
    float3 normal;
    float3 position;
};
GPlanePos GPlanePos_Ctor(float3 _normal, float3 _position)
{
    GPlanePos plane;
    plane.normal = _normal;
    plane.position = _position;
    return plane;
}
//Sphere
struct GSphere
{
    float3 center;
    float radius;
};
GSphere GSphere_Ctor(float3 _center, float _radius)
{
    GSphere sphere;
    sphere.center = _center;
    sphere.radius = _radius;
    return sphere;
}
//Box
struct GBox
{
    float3 center;
    float3 size;
    float3 extend;
    float3 boxMin;
    float3 boxMax;
};
GBox GBox_Ctor(float3 _center, float3 _size)
{
    GBox box;
    box.center=_center;
    box.size=_size;
    box.extend=_size*.5;
    box.boxMin = _center-box.extend;
    box.boxMax = _center+box.extend;
    return box;
}
struct GBoxRound
{
    GBox box;
    float roundness;
};
GBoxRound GRoundBox_Ctor(float3 _center,float3 _size,float _roundness)
{
    GBoxRound roundBox;
    roundBox.box=GBox_Ctor(_center,_size-_roundness*2);
    roundBox.roundness=_roundness;
    return roundBox;
}
struct GBoxFrame
{
    GBox box;
    float frameExtend;
};
GBoxFrame GFrameBox_Ctor(float3 _center,float3 _size,float _frameExtend)
{
    GBoxFrame frameBox;
    frameBox.box=GBox_Ctor(_center,_size);
    frameBox.frameExtend=_frameExtend;
    return frameBox;
}
//Torus
struct GTorus
{
    float3 center;
    float majorRadius;
    float minorRadius;
};
GTorus GTorus_Ctor(float3 _center,float _majorRadius,float _minorRadius)
{
    GTorus torus;
    torus.center=_center;
    torus.majorRadius=_majorRadius;
    torus.minorRadius=_minorRadius;
    return torus;
}
struct GTorusLink
{
    GTorus torus;
    float extend;
};
GTorusLink GTorusLink_Ctor(float3 _center,float _majorRadius,float _minorRadius,float _extend)
{
    GTorusLink torusLink;
    torusLink.torus=GTorus_Ctor(_center,_majorRadius,_minorRadius);
    torusLink.extend=_extend;
    return torusLink;
}
struct GTorusCapped
{
    GTorus torus;
    float2 capRadianSinCos;
};
GTorusCapped GTorusCapped_Ctor(float3 _center,float _majorRadius,float _minorRadius,float2 _capRadianSinCos)
{
    GTorusCapped torusCapped;
    torusCapped.torus=GTorus_Ctor(_center,_majorRadius,_minorRadius);
    torusCapped.capRadianSinCos=_capRadianSinCos;
    return torusCapped;
}

struct GHeightCone
{
    float3 origin;
    float3 normal;
    float sqrCosA;
    float tanA;
    float height;
    float3 bottom;
    float bottomRadius;
    GPlanePos bottomPlane;
    float GetRadius(float _height)
    {
        return _height * tanA;
    }
};
GHeightCone GHeightCone_Ctor(float3 _origin, float3 _normal, float _angle, float _height)
{
    GHeightCone cone;
    cone.origin = _origin;
    cone.normal = _normal;
    cone.height = _height;
    float radianA = _angle / 360. * PI;
    float cosA = cos(radianA);
    cone.sqrCosA = cosA * cosA;
    cone.tanA = tan(radianA);
    cone.bottom = _origin + _normal * _height;
    cone.bottomRadius = _height *cone. tanA;
    cone.bottomPlane = GPlanePos_Ctor(_normal, cone.bottom);
    return cone;
}
