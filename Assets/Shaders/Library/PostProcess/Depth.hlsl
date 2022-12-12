
TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

float SampleRawDepth(float2 uv){return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,uv).r;}
float SampleEyeDepth(float2 uv){return RawToEyeDepth(SampleRawDepth(uv));}
float Sample01Depth(float2 uv){return RawTo01Depth(SampleRawDepth(uv));}
float SampleEyeDistance(float2 uv) {return RawToDistance(SampleRawDepth(uv) ,uv);}
float3 TransformNDCToWorld(float2 uv){return TransformNDCToWorld(uv,SampleRawDepth(uv));}