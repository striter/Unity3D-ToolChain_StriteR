struct GAxis
{
    float3 origin;
    float3 right;
    float3 forward;
    float3 up;
    float distance;
    
    float2 GetUV(float3 _point)
    {
        float3 v0 = right;
        float3 v1 = forward;
        float3 v2 = _point - origin;
        float dot00 = dot(v0, v0);
        float dot01 = dot(v0, v1);
        float dot02 = dot(v0, v2);
        float dot11 = dot(v1, v1);
        float dot12 = dot(v1, v2);

        float denominator = (dot00 * dot11 - dot01 * dot01);
    
        float u = (dot11 * dot02 - dot01 * dot12) / denominator;
        float v = (dot00 * dot12 - dot01 * dot02) / denominator;

        return float2(u,v);
    }
};

GAxis GAxis_Ctor(float3 _origin, float3 _right, float3 _forward)
{
    GAxis axis;
    axis.origin = _origin;
    axis.right = _right;
    axis.forward = _forward;
    axis.up = cross(axis.right, axis.forward);
    axis.distance = dot(_origin, axis.up);
    return axis;
}

GAxis GAxis_Ctor(float3 _origin, float3 _right, float3 _up,float3 _forward)
{
    GAxis axis;
    axis.origin = _origin;
    axis.right = _right;
    axis.up = _up;
    axis.forward = _forward;
    axis.distance = dot(_origin, axis.forward);
    return axis;
}