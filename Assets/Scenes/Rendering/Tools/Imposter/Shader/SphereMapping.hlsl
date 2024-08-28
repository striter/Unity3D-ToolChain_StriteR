float2 CentricHemisphere_ToUV(float3 _direction)
{
    float x = _direction.x;
    float z = _direction.z;
    float r = sqrt(x * x + z * z);
    float u = r * r;
    float v = atan2(x, z) / kPI2;
    v += step(v,0);
    return float2(u, v);
}

float3 CentricHemisphere_ToPosition(float2 _uv)
{
    float u = _uv.x;
    float v = _uv.y;
    float r = sqrt(u);
    float theta =  kPI2 * v;
    float sinTheta;
    float cosTheta;
    sincos(theta,sinTheta,cosTheta);
    return float3(sinTheta * r,  sqrt(1 - u),  cosTheta * r);
}

float3 OctahedralHemisphere_ToPosition(float2 _uv)
{
    float2 oct = _uv * 2 - 1;
    oct = float2( oct.x + oct.y, oct.x - oct.y ) *0.5f;
    return normalize(float3( oct.x,1.0f - dot( 1.0f, abs(oct) ),oct.y ));
}

float2 OctahedralHemisphere_ToUV(float3 N)
{
    N.xz /= dot( 1.0f, abs(N) );
    float2 oct = float2(N.x + N.z, N.x - N.z);
    return oct * .5f + .5f;
}

float3 Octahedral_ToPosition(float2 _uv)
{
    float2 oct =  _uv * 2 - 1;
    float3 N = float3( oct, 1.0f - dot( 1.0f, abs(oct) ) );
    if( N.z < 0 )
        N.xy = ( 1 - abs(N.yx) ) * sign( N.xy );
    return normalize(N);
}
        
float2 Octahedral_ToUV(float3 N)
{
    N /= dot( 1.0f, abs(N) );
    if (N.z <= 0)
        N.xy = (1 - abs(N.yx)) * sign(N.xy);
    float2 oct = N.xy;
    return oct * .5f + .5f;
}

float3 Cube_ToPosition(float2 _uv) 
{
    float3 position = 0;
    float uvRadius = sin(_uv.y * kPI);
    sincos(kPI2 * _uv.x, position.z, position.x);
    position.xz *= uvRadius;
    position.y = -cos(kPI * _uv.y);
    return position;
}

float2 Cube_ToUV(float3 _direction)
{
    float phi = acos(-_direction.y) / kPI;
    float theta = atan2(_direction.z, _direction.x) / kPI2;
    theta += step(theta,0);
    return float2(theta , phi );
}

float3 ConcentricOctahedral_ToPosition(float2 _uv)
{
    float2 oct = 2 * _uv - 1.0f;
    float u = oct.x;
    float v = oct.y;
    float d = 1 - (abs(u) + abs(v));
    float r = 1 - abs(d);
    float z = signNonZero(d) * (1 - r * r);
    
    float theta = kPIDiv4 * (r==0? 1: ((abs(v) - abs(u)) / r + 1));
    float sinTheta = signNonZero(v) * sin(theta);
    float cosTheta = signNonZero(u) * cos(theta);
    float radius = sqrt(2 - r * r);
    return float3(cosTheta * r * radius, sinTheta * r * radius, z);
}

float2 ConcentricOctahedral_ToUV(float3 _direction)
{
    float3 absD = abs(_direction);
    float x = absD.x;
    float y = absD.y;
    float z = absD.z;

    float r = sqrt(1.0f - z);
    float a = max(absD.xy);
    float b = min(absD.xy);
    b = a == 0.0f ? 0.0f : b / a;

    float phi = atan_Fast_2DivPI(b);

    if (x < y) phi = 1.0f - phi;
        
    float v = phi * r;
    float u = r - v;
    if (_direction.z < 0.0f)
    {
        float tmp = u;
        u = 1.0f - v;
        v = 1.0f - tmp;
    }

    u = flipSign(u, signNonZero(_direction.x));
    v = flipSign(v, signNonZero(_direction.y));
    return float2(u, v) * .5f + .5f;
}