
#include "Assets/Shaders/Library/Geometry.hlsl"

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

float2 ImposterVertexEvaluate(out float3 _viewPositionWS,out float3 _imposterCenterWS,out float3 _forwardOS)
{
    float3 worldOrigin = 0;
    float4 perspective = float4( 0, 0, 0, 1 );
    if( UNITY_MATRIX_P[ 3 ][ 3 ] == 1 ) 
    {
        perspective = float4( 0, 0, 5000, 0 );
        worldOrigin = GetColumn(GetObjectToWorldMatrix(),3);
    }
    _viewPositionWS = worldOrigin + mul( UNITY_MATRIX_I_V, perspective ).xyz;
    _imposterCenterWS = TransformObjectToWorld(_BoundingSphere.xyz);
    _forwardOS = TransformWorldToObject(_viewPositionWS) - _BoundingSphere.xyz ;
    
    #if defined(_HEMISPHERE)
        _forwardOS.y = max(0.01f, _forwardOS.y);
    #endif
    _forwardOS = normalize(_forwardOS);
    
    return TransformObjectDirectionToUV(_forwardOS);
}

#define _PLANE_SAMPLE

void ImposterVertexEvaluate(float2 _uv,out float3 forwardWS,out float3 _imposterPositionWS,out float2 imposterUV)
{
    float3 _viewPositionWS = 0;
    float3 imposterCenterWS = 0;
    float3 forwardOS;
    
    float2 imposterViewUV = ImposterVertexEvaluate(_viewPositionWS,imposterCenterWS,forwardOS);

    int2 imposterCellIndex = floor(imposterViewUV * _ImposterTexel.xy) % _ImposterTexel.xy;
    
    forwardWS = TransformObjectToWorldDir(forwardOS);

    float3 billboardRightWS = normalize( cross( forwardWS, float3( 0,1,0 ) ) );
    float3 billboardUpwardWS = cross( billboardRightWS, forwardWS );
    float2 billboardUV = _uv;
    billboardUV -= .5;
    _imposterPositionWS =  (billboardUV.x * billboardRightWS + billboardUV.y * billboardUpwardWS)  * _BoundingSphere.w * 2 + imposterCenterWS;

    #if defined(_PLANE_SAMPLE)
        float2 imposterViewSampleUV = (imposterCellIndex + .5f) * _ImposterTexel.zw;
        float3 normalWS = TransformObjectToWorldDir(TransformUVToObjectDirection(imposterViewSampleUV));
        float3 rightWS = normalize( cross( normalWS, float3(0,1,0) ) );
        float3 upWS = normalize( cross( rightWS, normalWS ) );
    
        GPlane plane = GPlane_Ctor(normalWS,imposterCenterWS);
        GRay viewRay = GRay_StartEnd(_viewPositionWS,_imposterPositionWS);
        float distance = Distance(plane,viewRay);
        float3 hitOffset = viewRay.GetPoint(distance) - imposterCenterWS;
        float frameX = dot( hitOffset, rightWS );
        float frameZ = dot( hitOffset, upWS );
            
        imposterUV = float2( frameX, frameZ ) / _BoundingSphere.w; // why negative???
        imposterUV = imposterUV * .5f + .5;
        imposterUV = clamp(imposterUV,0,1);

    #else
        imposterUV = _uv;
    #endif

    float2 tilling = imposterCellIndex * _ImposterTexel.zw;
    imposterUV = TransformTex(imposterUV,float4(_ImposterTexel.zw,tilling));
    
    #if UNITY_PASS_SHADOWCASTER
        _imposterPositionWS -= forwardWS * _BoundingSphere.w * .25;
    #endif
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

float2 CalculateBilinearLerpUV(float2 _uv,float3 _imposterPositionWS,float3 _viewPositionWS,float3 _imposterCenterWS,float2 imposterViewUV,float _parallax,int2 cellIndex)
{
    float2 scale = _ImposterTexel.zw;
    int N = _ImposterTexel.x;
    float2 uv = _uv;

    #if defined(_PLANE_SAMPLE)
        float2 imposterViewSampleUV = (cellIndex + .5f)*_ImposterTexel.zw;
        float3 forwardOS = TransformUVToObjectDirection(imposterViewSampleUV);
        float3 forwardWS = TransformObjectToWorldDir(forwardOS);
        float3 rightWS = normalize(cross(forwardWS, float3(0,1,0)));
        float3 upWS = cross(rightWS, forwardWS);

        GPlane plane = GPlane_Ctor(-forwardWS,_imposterCenterWS);
        GRay viewRay = GRay_StartEnd(_viewPositionWS,_imposterPositionWS);
        float distance = Distance(plane,viewRay);
        float3 hitOffset = viewRay.GetPoint(distance) - _imposterCenterWS;
        float frameX = dot( hitOffset, rightWS );
        float frameZ = dot( hitOffset, upWS );
                
        uv = float2( frameX, frameZ ) / _BoundingSphere.w; // why negative???
        uv = uv * .5f + .5;
        uv = clamp(uv,0,1);
    #endif
    
    float2 tilling = ImposterTilling(cellIndex,N) * scale;
    float4 st = float4(scale,tilling);
    
    // UNITY_BRANCH
    // if(_parallax > 0)
    // {
    //     float2 center = st.zw + st.xy * .5f;
    //     float2 offset = center - imposterViewUV;
    //
    //     uv -= offset * _parallax;
    // }
    
    uv = TransformTex(uv,st);
    return uv;
}

void ImposterVertexEvaluate_Bilinear(float2 _uv,float _parallax,out float3 forwardWS,out float3 _imposterPositionWS,out float4 _imposterUV01,out float4 _imposterUV23,out float4 _imposterWeights)
{
    float3 forwardOS = 0;
    float3 imposterCenterWS = 0;
    float3 _viewPositionWS = 0;
    float2 imposterViewUV = ImposterVertexEvaluate(_viewPositionWS,imposterCenterWS,forwardOS);
    float s = imposterViewUV.x;
    float t = imposterViewUV.y;
    int N = _ImposterTexel.x;

    float x = floor(s * N - .5f) % N;
    float y = floor(t * N - .5f) % N;
    float aX = (s * N - 0.5f) - x;
    float aY = (t * N - 0.5f) - y;

    int2 imposterCellIndex = int2(x,y);
    float2 imposterViewSampleUV = (imposterCellIndex + 1) * _ImposterTexel.zw;
    // float3 forwardOS = TransformUVToObjectDirection(imposterViewSampleUV);
    forwardWS = TransformObjectToWorldDir(forwardOS);
    float3 rightWS = normalize( cross( forwardWS, float3( 0,1,0 ) ) );
    float3 upWS = cross( rightWS, forwardWS );
    
    float2 billboardUV = _uv;
    billboardUV -= .5;
    _imposterPositionWS = imposterCenterWS + ( billboardUV.x * rightWS + billboardUV.y * upWS )  * _BoundingSphere.w * 2;
    
    _imposterUV01 = float4(CalculateBilinearLerpUV(_uv,_imposterPositionWS,_viewPositionWS,imposterCenterWS,imposterViewSampleUV,_parallax,imposterCellIndex)
                        ,CalculateBilinearLerpUV(_uv,_imposterPositionWS,_viewPositionWS,imposterCenterWS,imposterViewSampleUV,_parallax,imposterCellIndex + int2(1,0)));
    _imposterUV23 = float4(CalculateBilinearLerpUV(_uv,_imposterPositionWS,_viewPositionWS,imposterCenterWS,imposterViewSampleUV,_parallax,imposterCellIndex + int2(1,1))
                        ,CalculateBilinearLerpUV(_uv,_imposterPositionWS,_viewPositionWS,imposterCenterWS,imposterViewSampleUV,_parallax,imposterCellIndex + int2(0,1)));
    _imposterWeights = float4((1 - aX) * (1 - aY), aX * (1 - aY) , aX * aY, ( 1 - aX ) * aY);
    #if UNITY_PASS_SHADOWCASTER
        _imposterPositionWS -= forwardWS * _BoundingSphere.w * .25;
    #endif
}