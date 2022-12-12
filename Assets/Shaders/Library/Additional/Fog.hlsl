half4 _FogParameters;
half4 _FogColor;

half CalculateFogFactor(half3 _positionWS)
{
    half verticalFog = invlerp(_FogParameters.w,_FogParameters.z, _positionWS.y);
    half distanceFog = invlerp(_FogParameters.x,_FogParameters.y,-TransformWorldToView(_positionWS).z);

    half fogFactor = saturate(max(verticalFog,distanceFog));
    fogFactor = saturate(fogFactor)*fogFactor;
    return fogFactor*_FogColor.a;
}

void FogColor(inout half3 finalCol,half fogFactor)
{
    finalCol = lerp(finalCol,_FogColor.rgb,fogFactor);
}

#if defined(_GFOG)
    #define V2F_FOG(i) float fogFactor:TEXCOORDi;
    #define FOG_TRANSFER(o) o.fogFactor = CalculateFogFactor(o.positionWS);
    #define FOG_MIX(i,col) FogColor(col,i.fogFactor);
#else
    #define V2F_FOG(i)  
    #define FOG_TRANSFER(v)  
    #define FOG_MIX(i,col) 
#endif

