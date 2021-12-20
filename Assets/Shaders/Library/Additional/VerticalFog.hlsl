//Horizon Fog
//#pragma multi_compile _ _VERTICALFOG
half _VerticalFogBegin;
half _VerticalFogEnd;
half _VerticalFogPow;
half4 _VerticalFogColor;
half3 VerticalFog(half3 _srcColor,half3 _positionWS)
{
    #ifndef _VERTICALFOG
        return _srcColor;
    #endif
    
    half fog=saturate(1.0h-invlerp(_VerticalFogBegin,_VerticalFogEnd, _positionWS.y));
    fog=pow(fog,_VerticalFogPow);
    fog*=_VerticalFogColor.a;
    return lerp(_srcColor,_VerticalFogColor.rgb,fog);
}
