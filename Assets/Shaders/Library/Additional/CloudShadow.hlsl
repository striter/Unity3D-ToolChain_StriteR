// #pragma shader_feature _CLOUDSHADOW
float4 _CloudParam1;        //x strength y scale z planeDistance
float4 _CloudParam2;        //xy SmoothStep zw Flow
TEXTURE2D(_CloudShadowTexture);SAMPLER(sampler_CloudShadowTexture);
float CloudShadowAttenuation(float3 _positionWS,float3 _lightDirWS)
{
    #ifndef _CLOUDSHADOW
        return 1;
    #endif

    float NDL=dot(_lightDirWS,float3(0,1,0));
    float distance=(_CloudParam1.z-_positionWS.y)*rcp(NDL);
    float3 projectPosition=_positionWS+_lightDirWS*distance;
    float2 uv=projectPosition.xz;
    float2 cloudUV=(uv+_CloudParam2.zw*_Time.y)/_CloudParam1.y;
    float cloudSample= SAMPLE_TEXTURE2D_LOD(_CloudShadowTexture,sampler_CloudShadowTexture,cloudUV,0).r;
    cloudSample=smoothstep(_CloudParam2.x,_CloudParam2.y,cloudSample);
    cloudSample=lerp(_CloudParam1.x,1,cloudSample);
    return cloudSample;
}