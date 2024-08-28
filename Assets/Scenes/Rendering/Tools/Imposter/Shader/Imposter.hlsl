UNITY_INSTANCING_BUFFER_START(Imposter)
    UNITY_DEFINE_INSTANCED_PROP(float4, _ImposterTexel)
    UNITY_DEFINE_INSTANCED_PROP(float4, _ImposterBoundingSphere)
UNITY_INSTANCING_BUFFER_END(Imposter)

//[KeywordEnum(CUBE,OCTAHEDRAL,CONCENTRIC_OCTAHEDRAL,CENTRIC_HEMISPHERE,OCTAHEDRAL_HEMISPHERE)]_MAPPING("Sphere Mode",int) = 4
// #pragma shader_feature_vertex _MAPPING_CUBE _MAPPING_OCTAHEDRAL _MAPPING_CONCENTRIC_OCTAHEDRAL _MAPPING_CENTRIC_HEMISPHERE _MAPPING_OCTAHEDRAL_HEMISPHERE

#if defined(_MAPPING_CENTRIC_HEMISPHERE) || defined(_MAPPING_OCTAHEDRAL_HEMISPHERE)
    #define _HEMISPHERE
#endif

#define _ImposterTexel UNITY_ACCESS_INSTANCED_PROP(Imposter,_ImposterTexel)
#define _BoundingSphere UNITY_ACCESS_INSTANCED_PROP(Imposter,_ImposterBoundingSphere)

#include "SphereMapping.hlsl"

float3 TransformUVToObjectDirection(float2 _uv)
{
    #if defined(_MAPPING_CONCENTRIC_OCTAHEDRAL)
        return ConcentricOctahedral_ToPosition(_uv);
    #elif defined(_MAPPING_OCTAHEDRAL)
        return Octahedral_ToPosition(_uv);
    #elif defined(_MAPPING_CENTRIC_HEMISPHERE)
        return CentricHemisphere_ToPosition(_uv);
    #elif defined(_MAPPING_OCTAHEDRAL_HEMISPHERE)
        return OctahedralHemisphere_ToPosition(_uv);
    #elif defined(_MAPPING_CUBE)
        return Cube_ToPosition(_uv);
    #endif

    return 0;
}

float2 TransformObjectDirectionToUV(float3 _direction)
{
    #if defined(_MAPPING_CONCENTRIC_OCTAHEDRAL)
        return ConcentricOctahedral_ToUV(_direction);
    #elif defined(_MAPPING_OCTAHEDRAL)
        return Octahedral_ToUV(_direction);
    #elif defined(_MAPPING_CENTRIC_HEMISPHERE)
        return CentricHemisphere_ToUV(_direction);
    #elif defined(_MAPPING_OCTAHEDRAL_HEMISPHERE)
        return OctahedralHemisphere_ToUV(_direction);
    #elif defined(_MAPPING_CUBE)
        return Cube_ToUV(_direction);
    #endif
    
    return 0;
}

void ImposterVertexEvaluate(float2 uv,float3 _viewPositionWS,out float3 imposterPositionOS,out float3 forwardOS,out float2 imposterUV)
{
    float3 viewPositionOS = TransformWorldToObject(_viewPositionWS);
    float3 viewDirectionOS = normalize(viewPositionOS - _BoundingSphere.xyz);

    float2 _imposterUV = TransformObjectDirectionToUV(viewDirectionOS);
    int2 cellIndex = floor(_imposterUV * _ImposterTexel.xy) % _ImposterTexel.xy;
    float2 uvMin = cellIndex * _ImposterTexel.zw;
    imposterUV = TransformTex(uv,float4(_ImposterTexel.zw,uvMin));
    
    forwardOS = TransformUVToObjectDirection((cellIndex + .5f) * _ImposterTexel.zw);
    float3 rightOS = normalize( cross( forwardOS, float3( 0,1,0 ) ) );
    float3 upOS = cross( rightOS, forwardOS );
    uv -= .5;
    imposterPositionOS = _BoundingSphere.xyz + (uv.x * rightOS + uv.y * upOS) * _BoundingSphere.w * 2;
}

float2 ImposterTilling(int2 _pixelIndex,int _N)
{
    #if defined(_MAPPING_CONCENTRIC_OCTAHEDRAL)
        if (((_pixelIndex.x ^ _pixelIndex.y) & _N) != 0)
        {
            _pixelIndex.x = _N - 1 - _pixelIndex.x;
            _pixelIndex.y = _N - 1 - _pixelIndex.y;
        }
        _pixelIndex &= (_N - 1);
        return _pixelIndex;
    #endif

    #if defined(_HEMISPHERE)
        _pixelIndex = clamp(_pixelIndex,0,_N - 1);
        return _pixelIndex;
    #endif
    
    _pixelIndex = (_pixelIndex + _N) % _N;
    return _pixelIndex;
}

float2 CalculateBilinearLerpUV(float2 _uv,float2 imposterUV,float _parallax,int2 cellIndex)
{
    float2 scale = _ImposterTexel.zw;
    int N = _ImposterTexel.x;
    
    float2 tilling = ImposterTilling(cellIndex,N) * scale;
    float4 st = float4(scale,tilling);
    UNITY_BRANCH
    if(_parallax > 0)
    {
        float2 center = st.zw + st.xy * .5f;
        float2 offset = center - imposterUV;

        _uv += offset * _parallax;
    }
    return clamp(TransformTex(_uv,st),tilling,tilling + scale);
}

void ImposterVertexEvaluate_Bilinear(float2 _uv,float _parallax,float3 _viewPositionWS,out float3 imposterPositionOS,out float3 _forwardOS,out float4 _imposterUV01,out float4 _imposterUV23,out float4 _imposterWeights)
{
    float3 viewPositionOS = TransformWorldToObject(_viewPositionWS);
    _forwardOS = viewPositionOS - _BoundingSphere.xyz;

    #if defined(_HEMISPHERE)
        _forwardOS.y = max(0.01f, _forwardOS.y);
        _forwardOS = normalize(_forwardOS);
    #endif

    _forwardOS = normalize(_forwardOS);
    
    float2 imposterUV = TransformObjectDirectionToUV(_forwardOS);
    float s = imposterUV.x;
    float t = imposterUV.y;
    int N = _ImposterTexel.x;

    float x = floor(s * N - .5f) ;
    float y = floor(t * N - .5f);
    float aX = (s * N - 0.5f) - x;
    float aY = (t * N - 0.5f) - y;

    int2 cellIndex = int2(x, y);
    _imposterUV01 = float4(CalculateBilinearLerpUV(_uv,imposterUV,_parallax,cellIndex),CalculateBilinearLerpUV(_uv,imposterUV,_parallax,cellIndex + float2(1,0)));
    _imposterUV23 = float4(CalculateBilinearLerpUV(_uv,imposterUV,_parallax,cellIndex + float2(1,1)),CalculateBilinearLerpUV(_uv,imposterUV,_parallax,cellIndex + float2(0,1)));
    _imposterWeights = float4((1 - aX) * (1 - aY), aX * (1 - aY) , aX * aY, ( 1 - aX ) * aY);

    float3 rightOS = normalize( cross( _forwardOS, float3( 0,1,0 ) ) );
    float3 upOS = cross( rightOS, _forwardOS);

    _uv -= .5;
    imposterPositionOS = _BoundingSphere.xyz + (_uv.x * rightOS + _uv.y * upOS) * _BoundingSphere.w * 2;
}