//#pragma shader_feature _HORIZONBEND

float3 _HorizonBendPosition;
float3 _HorizonBendDirection;
float3 _HorizonBendDistances;

float3 HorizonBend(float3 positionWS)
{
    #ifndef _HORIZONBEND
        return positionWS;
    #endif
    float3 cameraOffset=_HorizonBendPosition-positionWS;
    float2 offset=cameraOffset.xz;
    float distance=length(offset);
    float param=max(0,(distance-_HorizonBendDistances.x)/_HorizonBendDistances.y);
    param*=param;
    return positionWS+_HorizonBendDirection*param;
}
