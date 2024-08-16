
UNITY_INSTANCING_BUFFER_START(Imposter)
    UNITY_DEFINE_INSTANCED_PROP(float4, _ImposterTexel)
    UNITY_DEFINE_INSTANCED_PROP(float4, _ImposterBoundingSphere)
UNITY_INSTANCING_BUFFER_END(Imposter)

#define _ImposterTexel UNITY_ACCESS_INSTANCED_PROP(Imposter,_ImposterTexel)
#define _BoundingSphere UNITY_ACCESS_INSTANCED_PROP(Imposter,_ImposterBoundingSphere)

#define hemisphere

float3 Octahedral(float2 _uv)
{
    _uv %= 1;
    
    float2 sample = 2.0f * _uv - 1.0f;
    float3 N = float3( sample, 1.0f - dot( 1.0f, abs(sample) ) );
    if( N.z < 0 )
        N.xy = ( 1 - abs(N.yx) ) * sign( N.xy);
    return normalize(N);
}

float2 OctahedralToUV(float3 _direction)
{
    float3 d = _direction;
    d /= dot(1.0f, abs(d));
    if (d.z <= 0)
        d.xy = (1.0f - abs(d.yx)) * sign(d.xy);
    return d.xy * 0.5f + 0.5f;
}

#define hemisphere

float3 TransformUVToObjectDirection(float2 _uv)
{
    #if defined(hemisphere)
        _uv.y = lerp(0.5f,1.0f, _uv.y);
    #endif
    return Octahedral(_uv);
}

float2 TransformObjectDirectionToUV(float3 _direction)
{
    #if defined(hemisphere)
        _direction.y = max(_direction.y,0.01f);
        _direction = normalize(_direction);
    #endif

    float2 uv = OctahedralToUV(_direction);
    #if defined(hemisphere)
        uv.y = invlerp(0.5f,1.0f, uv.y);
    #endif
    return uv;
}

void ImposterVertexEvaluate(float2 uv,float3 viewDirectionOS,out float3 imposterPositionOS,out float2 imposterUV,out float3 forwardOS)
{
    
    float2 estimateUV = floor((TransformObjectDirectionToUV(viewDirectionOS) * _ImposterTexel.xy) % _ImposterTexel.xy) * _ImposterTexel.zw;
    forwardOS = TransformUVToObjectDirection(estimateUV);
    float3 rightOS = normalize( cross( forwardOS, float3( 0,1,0 ) ) );
    float3 upOS = cross( rightOS, forwardOS );

    imposterUV = TransformTex(uv,float4(_ImposterTexel.zw,estimateUV));

    uv -= .5;
    imposterPositionOS = (uv.x * rightOS + uv.y * upOS) * _BoundingSphere.w * 2;
}