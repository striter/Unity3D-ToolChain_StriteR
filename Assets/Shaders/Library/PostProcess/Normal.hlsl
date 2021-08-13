TEXTURE2D( _CameraNormalTexture); SAMPLER(sampler_CameraNormalTexture);
float4 _CameraNormalTexture_TexelSize;

half3 SampleNormalWS(float2 uv)
{
    half3 normalEncoded=SAMPLE_TEXTURE2D(_CameraNormalTexture,sampler_CameraNormalTexture,uv).rgb;
    half3 normalWS=(normalEncoded-.5h)*2.h;
    return normalWS;
}