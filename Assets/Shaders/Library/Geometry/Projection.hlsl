
float Projection(GRay _ray, float3 _point)
{
    return dot(_point - _ray.origin, _ray.direction);
}
float Projection(GLine _line, float3 _point)
{
    return clamp(Projection(_line.ToRay(), _point), 0., _line.length);
}

float2 Projection(GRay _ray1, GRay _ray2)
{
    float3 diff = _ray2.origin - _ray1.origin;
    float a01 = -dot(_ray2.direction, _ray1.direction);
    float b0 = dot(diff, _ray2.direction);
    float b1 = -dot(diff, _ray1.direction);
    float det = 1. - a01 * a01;
    return float2((a01 * b0 - b1) / det, (a01 * b1 - b0) / det);
}

float2 Projection(GLine _line, GRay _ray)
{
    float2 distances = Projection(_line.ToRay(), _ray);
    distances.x = clamp(distances.x, 0., _line.length);
    distances.y = Projection(_ray, _line.GetPoint(distances.x));
    return distances;
}