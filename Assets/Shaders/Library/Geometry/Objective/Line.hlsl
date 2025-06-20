
struct GLine
{
    float3 start;
    float3 end;
    float3 direction;
    float length;
    float3 GetPoint(float _distance)  { return start + direction * _distance;  }
    GRay ToRay()
    {
        GRay ray;
        ray.origin = start;
        ray.direction = direction;
        return ray;
    }
};
GLine GLine_Ctor(float3 _origin, float3 _direction, float _length)
{
    GLine gline;
    gline.start = _origin;
    gline.direction = _direction;
    gline.length = _length;
    gline.end = _origin + _direction * _length;
    return gline;
}
GLine GLine_Ctor(float3 _origin, float3 _end)
{
    GLine gline;
    gline.start = _origin;
    gline.end = _end;
    float3 delta = _end - _origin;
    gline.length = length(delta);
    gline.direction = normalize(delta);
    return gline;
}