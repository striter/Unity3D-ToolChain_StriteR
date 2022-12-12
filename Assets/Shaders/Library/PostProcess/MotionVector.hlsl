float4x4 _Matrix_VP_Pre;

TEXTURE2D(_CameraMotionVectorTexture);SAMPLER(sampler_CameraMotionVectorTexture);
float4 _CameraMotionVectorTexture_TexelSize;

float2 SampleMotionVector(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraMotionVectorTexture,sampler_CameraMotionVectorTexture,uv).rg;
}