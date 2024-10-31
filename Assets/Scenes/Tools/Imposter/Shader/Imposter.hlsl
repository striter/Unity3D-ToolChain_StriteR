
#include "Assets/Shaders/Library/Geometry.hlsl"

INSTANCING_BUFFER_START
    INSTANCING_PROP(float4, _ImposterTexel)
    INSTANCING_PROP(float4, _ImposterBoundingSphere)
	INSTANCING_PROP(float, _AlphaClip)
	INSTANCING_PROP(float, _Parallax)
INSTANCING_BUFFER_END

//[KeywordEnum(CUBE,OCTAHEDRAL,CONCENTRIC_OCTAHEDRAL,CENTRIC_HEMISPHERE,OCTAHEDRAL_HEMISPHERE)]_MAPPING("Sphere Mode",int) = 4
// #pragma shader_feature_vertex _MAPPING_CUBE _MAPPING_OCTAHEDRAL _MAPPING_CONCENTRIC_OCTAHEDRAL _MAPPING_CENTRIC_HEMISPHERE _MAPPING_OCTAHEDRAL_HEMISPHERE

#if defined(_MAPPING_CENTRIC_HEMISPHERE) || defined(_MAPPING_OCTAHEDRAL_HEMISPHERE)
    #define _HEMISPHERE
#endif

#define _ImposterTexel INSTANCE(_ImposterTexel)
#define _BoundingSphere INSTANCE(_ImposterBoundingSphere)

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

float2 ImposterVertexEvaluate(out float3 _viewPositionOS,out float3 _forwardOS)
{
    float3 worldOrigin = 0;
    float4 perspective = float4( 0, 0, 0, 1 );
    if( UNITY_MATRIX_P[ 3 ][ 3 ] == 1 ) 
    {
        perspective = float4( 0, 0, 5000, 0 );
        worldOrigin = GetColumn(GetObjectToWorldMatrix(),3).xyz;
    }
    float3 viewPositionWS = worldOrigin + mul( UNITY_MATRIX_I_V, perspective ).xyz;
    _viewPositionOS = TransformWorldToObject(viewPositionWS);
    _forwardOS = _viewPositionOS - _BoundingSphere.xyz ;
    
    #if defined(_HEMISPHERE)
        _forwardOS.y = max(0.01f, _forwardOS.y);
    #endif
    _forwardOS = normalize(_forwardOS);
    
    return TransformObjectDirectionToUV(_forwardOS);
}

#define _PLANE_SAMPLE

void ImposterVertexEvaluate(float2 _uv,out float3 forwardWS,out float3 _imposterPositionWS,out float2 imposterUV)
{
    float3 viewPositionOS = 0;
    float3 viewForwardOS;
    
    float2 imposterViewUV = ImposterVertexEvaluate(viewPositionOS,viewForwardOS);

    int2 imposterCellIndex = floor(imposterViewUV * _ImposterTexel.xy) % _ImposterTexel.xy;
    

    float3 billboardRightOS = normalize( cross( viewForwardOS, float3( 0,1,0 ) ) );
    float3 billboardUpwardOS = cross( billboardRightOS, viewForwardOS );
    float2 billboardUV = _uv;
    billboardUV -= .5;

    float3 centerOS = _BoundingSphere.xyz;
    float3 imposterPositionOS =  centerOS + ( billboardUV.x * billboardRightOS + billboardUV.y * billboardUpwardOS ) * _BoundingSphere.w * 2;
    float2 tilling = imposterCellIndex * _ImposterTexel.zw;
    #if defined(_PLANE_SAMPLE)
        float2 imposterViewSampleUV = (imposterCellIndex + .5f) * _ImposterTexel.zw;
        float3 planeForwardOS = TransformUVToObjectDirection(imposterViewSampleUV);
        float3 planeRightOS = normalize( cross( planeForwardOS, float3(0,1,0) ) );
        float3 planerUpOS = normalize( cross( planeRightOS, planeForwardOS ) );
        
        GPlane plane = GPlane_Ctor(planeForwardOS,centerOS);
        GRay viewRay = GRay_StartEnd(viewPositionOS,imposterPositionOS);
        float distance = Distance(plane,viewRay);
        float3 hitOffset = viewRay.GetPoint(distance) - centerOS;
        float frameX = dot( hitOffset, planeRightOS );
        float frameZ = dot( hitOffset, planerUpOS );
                
        imposterUV = float2( frameX, frameZ ) / _BoundingSphere.w; // why negative???
        imposterUV = imposterUV * .5f + .5;
        imposterUV = clamp(imposterUV,0,1);

    #else
        imposterUV = _uv;
    #endif
    
    forwardWS = TransformObjectToWorldDir(viewForwardOS);
    imposterUV = TransformTex(imposterUV,float4(_ImposterTexel.zw,tilling));
    _imposterPositionWS = TransformObjectToWorld(imposterPositionOS);
    
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

float2 CalculateBilinearLerpUV(float2 _uv,float3 _imposterPositionOS,float3 _viewPositionOS,float2 imposterViewUV,float _parallax,int2 cellIndex)
{
    float2 scale = _ImposterTexel.zw;
    int N = _ImposterTexel.x;
    float2 uv = _uv;

    #if defined(_PLANE_SAMPLE)
        float2 imposterViewSampleUV = (cellIndex + .5f)*_ImposterTexel.zw;
        float3 forwardOS = TransformUVToObjectDirection(imposterViewSampleUV);
        float3 rightOS = normalize(cross(forwardOS, float3(0,1,0)));
        float3 upOS = cross(rightOS, forwardOS);

        float3 centerOS = _BoundingSphere.xyz;
        GPlane plane = GPlane_Ctor(-forwardOS,centerOS);
        GRay viewRay = GRay_StartEnd(_viewPositionOS,_imposterPositionOS);
        float distance = Distance(plane,viewRay);
        float3 hitOffset = viewRay.GetPoint(distance) - centerOS;
        float frameX = dot( hitOffset, rightOS );
        float frameZ = dot( hitOffset, upOS );
                
        uv = float2( frameX, frameZ ) / _BoundingSphere.w;
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
    float3 _viewPositionOS = 0;
    float2 imposterViewUV = ImposterVertexEvaluate(_viewPositionOS,forwardOS);
    float s = imposterViewUV.x;
    float t = imposterViewUV.y;
    int N = _ImposterTexel.x;

    float x = floor(s * N - .5f) % N;
    float y = floor(t * N - .5f) % N;
    float aX = (s * N - 0.5f) - x;
    float aY = (t * N - 0.5f) - y;

    int2 imposterCellIndex = int2(x,y);
    float2 imposterViewSampleUV = (imposterCellIndex + 1) * _ImposterTexel.zw;
    float3 rightOS = normalize( cross( forwardOS, float3( 0,1,0 ) ) );
    float3 upOS = cross( rightOS, forwardOS );
    
    float2 billboardUV = _uv;
    billboardUV -= .5;

    float3 viewPositionOS =  _BoundingSphere.xyz + ( billboardUV.x * rightOS + billboardUV.y * upOS ) * _BoundingSphere.w * 2;
    _imposterUV01 = float4(CalculateBilinearLerpUV(_uv,viewPositionOS,_viewPositionOS,imposterViewSampleUV,_parallax,imposterCellIndex)
                        ,CalculateBilinearLerpUV(_uv,viewPositionOS,_viewPositionOS,imposterViewSampleUV,_parallax,imposterCellIndex + int2(1,0)));
    _imposterUV23 = float4(CalculateBilinearLerpUV(_uv,viewPositionOS,_viewPositionOS,imposterViewSampleUV,_parallax,imposterCellIndex + int2(1,1))
                        ,CalculateBilinearLerpUV(_uv,viewPositionOS,_viewPositionOS,imposterViewSampleUV,_parallax,imposterCellIndex + int2(0,1)));
    _imposterWeights = float4((1 - aX) * (1 - aY), aX * (1 - aY) , aX * aY, ( 1 - aX ) * aY);

    _imposterPositionWS = TransformObjectToWorld(viewPositionOS);
    forwardWS = TransformObjectToWorldDir(forwardOS);
    #if UNITY_PASS_SHADOWCASTER
        _imposterPositionWS -= forwardWS * _BoundingSphere.w * .25;
    #endif
}