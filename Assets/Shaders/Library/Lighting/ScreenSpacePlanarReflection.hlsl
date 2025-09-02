//Indirect Specular
sampler2D _CameraReflectionTexture0;
sampler2D _CameraReflectionTexture1;
sampler2D _CameraReflectionTexture2;
sampler2D _CameraReflectionTexture3;

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial_PlanarReflection)
    INSTANCING_PROP(uint, _CameraReflectionTextureOn)
    INSTANCING_PROP(uint, _CameraReflectionTextureIndex)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial_PlanarReflection)

sampler2D _ScreenSpaceReflectionTexture;

half4 IndirectSSRSpecular(float2 screenUV,float eyeDepth, half3 normalTS,float _distort)
{
    [branch]
    if (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureOn) == 1)
    {
        screenUV += normalTS.xy * _distort*rcp(eyeDepth);
        [branch]
        switch (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureIndex))
        {
        default:return 0;
        case 0:return tex2D(_CameraReflectionTexture0, screenUV);
        case 1:return tex2D(_CameraReflectionTexture1, screenUV);
        case 2:return tex2D(_CameraReflectionTexture2, screenUV);
        case 3:return tex2D(_CameraReflectionTexture3, screenUV);
        }
    }
    else    //Avoid warning
    {
        return 0;//tex2D(_ScreenSpaceReflectionTexture,screenUV);
    }
}
