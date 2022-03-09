//Screen Space Calculations
//xyz direction z length
float3 _FrustumCornersRayBL;
float3 _FrustumCornersRayBR;
float3 _FrustumCornersRayTL;
float3 _FrustumCornersRayTR;

float3 TransformNDCToFrustumCornersRay(float2 uv)
{
    return bilinearLerp(_FrustumCornersRayTL, _FrustumCornersRayTR, _FrustumCornersRayBL, _FrustumCornersRayBR, uv);
}
float3 _OrthoCameraPosBL;
float3 _OrthoCameraPosBR;
float3 _OrthoCameraPosTL;
float3 _OrthoCameraPosTR;
float3 GetCameraRealPositionWS(float2 uv)
{
    [branch]
    if(unity_OrthoParams.w)
        return bilinearLerp(_OrthoCameraPosTL,_OrthoCameraPosTR,_OrthoCameraPosBL,_OrthoCameraPosBR,uv);
    else
        return _WorldSpaceCameraPos;
}

float3 _OrthoCameraDirection;
float3 GetCameraRealDirectionWS(float3 _positionWS)
{
    [branch]
    if(unity_OrthoParams.w)
        return _OrthoCameraDirection;
    else
        return normalize(_positionWS-_WorldSpaceCameraPos);
}

float3 GetViewDirectionWS(float3 _positionWS)
{
    return normalize(_WorldSpaceCameraPos-_positionWS);
}