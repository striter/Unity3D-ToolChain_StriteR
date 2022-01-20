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

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 normalWS=SampleNormalWS(i.uv);
                float rawDepth=SampleRawDepth(i.uv);
                float eyeDepth=RawToEyeDepth(rawDepth);
                float3 frustumCornersRay=TransformNDCToFrustumCornersRay(i.uv);
                float3 marchPositionWS=GetCameraPositionWS()+frustumCornersRay*eyeDepth;
                float3 marchDirWS=normalize(reflect(normalize(frustumCornersRay),normalWS));

                float marchStep=.3;
                float3 currentMarchPos=marchPositionWS+normalWS*marchStep*.5;
                float4 finalCol=0;
                [unroll(32)]
                for(int i=0;i<32;i++)
                {
                    currentMarchPos+=marchStep*marchDirWS;
                    float2 uv;
                    float depth;
                    TransformHClipToUVDepth(mul(_Matrix_VP,float4( currentMarchPos,1)),uv,depth);
                    float sourceDepth=RawToEyeDepth(depth);
                    float compareDepth=RawToEyeDepth(SampleRawDepth(uv));
                    if(abs(compareDepth-sourceDepth)>marchStep)
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
