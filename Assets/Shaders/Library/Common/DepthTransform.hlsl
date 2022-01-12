
//Depth Conversion
#if !UNITY_REVERSED_Z
    #define Z_Multiply 1.h
    #define Z_BEGIN 0.h
    #define Z_END 1.h
#else
    #define Z_Multiply -1.h
    #define Z_BEGIN 1.h
    #define Z_END 0.h
#endif

bool DepthGreater(float _depthSrc,float _depthComp)
{
    #if !UNITY_REVERSED_Z
    return _depthSrc>_depthComp;
    #else
    return _depthSrc<_depthComp;
    #endif
}

bool DepthLesser(float _depthSrc,float _depthComp)
{
    #if !UNITY_REVERSED_Z
        return _depthSrc<_depthComp;
    #else
        return _depthSrc>_depthComp;
    #endif
}

float TransformWorldToEyeDepth(float3 _positionWS,float4x4 _matrixV)
{
    return -(_positionWS.x*_matrixV._m20+_positionWS.y*_matrixV._m21+_positionWS.z*_matrixV._m22+_matrixV._m23);
}

//Eye Raw 01 Depth Transformation
inline float EyeToRawDepthOrtho(float _eyeDepth,float4 _projectionParams)
{
    float rawDepth=invlerp(_projectionParams.y,_projectionParams.z,_eyeDepth);
    #if UNITY_REVERSED_Z
        rawDepth=1.0-rawDepth;
    #endif
    return rawDepth;
}

inline float EyeToRawDepthPerspective(float _eyeDepth,float4 _zBufferParams)
{
    return (1. - _zBufferParams.w * _eyeDepth) / (_zBufferParams.z * _eyeDepth);
}

inline float RawToEyeDepthPerspective(float _rawDepth,float4 _zBufferParams)
{
    return LinearEyeDepth(_rawDepth,_zBufferParams);
}

inline float RawToEyeDepthOrthographic(float _rawDepth,float4 _projectionParams)
{
    #if UNITY_REVERSED_Z
        _rawDepth=1.0f-_rawDepth;
    #endif
    return lerp(_projectionParams.y,_projectionParams.z,_rawDepth);
}

inline float RawTo01DepthPerspective(float _rawDepth,float4 _zBufferParams)
{
   return Linear01Depth(_rawDepth,_zBufferParams); 
}

inline float RawTo01DepthOrthographic(float _rawDepth)
{
    #if UNITY_REVERSED_Z
        _rawDepth=1.0f-_rawDepth;
    #endif
    return _rawDepth;
}

float RawToEyeDepth(float _rawDepth)
{
    [branch]
    if(unity_OrthoParams.w)
        return RawToEyeDepthOrthographic(_rawDepth,_ProjectionParams);
    else
        return RawToEyeDepthPerspective(_rawDepth,_ZBufferParams);
}

float RawTo01Depth(float _rawDepth)
{
    [branch]
    if(unity_OrthoParams.w)
        return RawTo01DepthOrthographic(_rawDepth);
    else
        return RawTo01DepthPerspective(_rawDepth,_ZBufferParams);
}

float EyeToRawDepth(float _eyeDepth)
{
    [branch]
    if(unity_OrthoParams.w)
        return EyeToRawDepthOrtho(_eyeDepth,_ProjectionParams);
    else
        return EyeToRawDepthPerspective(_eyeDepth,_ZBufferParams);
}

//TL TR BR BL
float4 _FrustumCornersScaling;
float RawToDistance(float _rawDepth,float2 _uv)
{
    float eyeDepth=RawToEyeDepth(_rawDepth);
    if(unity_OrthoParams.w)
        return eyeDepth;
    else
        return eyeDepth * length(TransformNDCToFrustumCornersRay(_uv));
}
