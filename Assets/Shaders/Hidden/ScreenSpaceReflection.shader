Shader "Hidden/ScreenSpaceReflection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Shaders/Library/PostProcess.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 normalWS = SampleNormalWS(i.uv);
                float3 positionWS = TransformNDCToWorld(i.uv);
                float3 viewDirWS = GetCameraRealDirectionWS(positionWS);
                float3 marchDirWS = normalize(reflect(viewDirWS,normalWS));
                
                float3 marchStep = marchDirWS * .3;
                float3 currentMarchPos = positionWS+marchStep*.5;
                float4 finalCol=0;
                float2 uv;
                float depth = 0;
                [unroll(32)]
                for(int index = 0; index < 32;index++)
                {
                    currentMarchPos+=marchStep;
                    TransformHClipToUVDepth(mul(_Matrix_VP,float4( currentMarchPos,1)),uv,depth);

                    float sourceDepth=RawToEyeDepth(depth);
                    float compareDepth=RawToEyeDepth(SampleRawDepth(uv));
                    
                    if(sourceDepth < compareDepth)
                        continue;

                    finalCol=float4(SampleMainTex(uv).rgb,1);
                    break;
                }

                return finalCol;
            }
            ENDHLSL
        }
    }
}
