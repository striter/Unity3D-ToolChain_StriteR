struct GTorus
{
    float3 center;
    float majorRadius;
    float minorRadius;
    

    float SDF(float3 _position)
    {
        _position -= center;
        float2 q = float2(length(_position.xz)-majorRadius,_position.y);
        return length(q)-minorRadius;
    }
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
    float SDF(float3 _position)
    {
        float3 q=_position-torus.center;
        q.y=max(abs(q.y)-extend,0.0);
        return length(float2(length(q.xy)-torus.majorRadius,q.z))-torus.minorRadius;
    }
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
    float SDF(float3 _position)
    {
        float2 sc=capRadianSinCos;
        float ra=torus.majorRadius;
        float rb=torus.minorRadius;
        float3 p=_position-torus.center;
        p.x = abs(p.x);
        float k = (sc.y*p.x>sc.x*p.y) ? dot(p.xy,sc) : length(p.xy);
        return sqrt( dot(p,p) + ra*ra - 2.0*ra*k ) - rb;
    }
};
GTorusCapped GTorusCapped_Ctor(float3 _center,float _majorRadius,float _minorRadius,float2 _capRadianSinCos)
{
    GTorusCapped torusCapped;
    torusCapped.torus=GTorus_Ctor(_center,_majorRadius,_minorRadius);
    torusCapped.capRadianSinCos=_capRadianSinCos;
    return torusCapped;
}
