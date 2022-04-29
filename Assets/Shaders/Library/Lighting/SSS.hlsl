half3 SSSLighting(half _thickness,half _influence,half _intensity,Light _light,half3 _normalWS,half3 _viewDirWS)
{
    half3 h=normalize(_light.direction + _normalWS*_influence);
    half vdh=saturate(dot(_viewDirWS,-h));
    half sssIntensity = saturate(vdh*_thickness)*_light.distanceAttenuation;
    return sssIntensity*_intensity*_light.color;
}