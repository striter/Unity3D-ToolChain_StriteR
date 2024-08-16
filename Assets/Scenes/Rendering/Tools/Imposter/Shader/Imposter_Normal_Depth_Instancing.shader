Shader "Game/Optimize/Imposter/Normal_Depth_Instancing"
{
    Properties
    {
    	_AlphaClip("Clip",Range(0,1)) = 0.5
        [NoScaleOffset]_AlbedoAlpha("_AlbedoAlpha",2D) = "white"
        [NoScaleOffset]_NormalDepth("_NormalDepth",2D) = "white"
    	_ImposterTexel("Texel",Vector)=(1,1,1,1)
    	_ImposterBoundingSphere("Texel",Vector)=(1,1,1,1)
    }
    SubShader
    {
    	HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/Geometry.hlsl"
            #include "Imposter.hlsl"

            #pragma multi_compile_instancing

            struct a2v
            {
            	float4 uv0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv0:TEXCOORD0;
            	float3 positionWS : TEXCOORD1;
            	float3 forwardWS : TEXCOORD2;
            	float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct f2o
            {
                float4 result : SV_TARGET;
                float depth : SV_DEPTH;
            };

            TEXTURE2D(_AlbedoAlpha);SAMPLER(sampler_AlbedoAlpha);
            TEXTURE2D(_NormalDepth);SAMPLER(sampler_NormalDepth);
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_AlphaClip)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float3 viewDirectionOS = normalize(TransformWorldToObject(_WorldSpaceCameraPos));
				float3 positionOS = 0;
				float3 forwardOS = 0;
				ImposterVertexEvaluate(v.uv0,viewDirectionOS, positionOS, o.uv0,forwardOS);
                o.positionCS = TransformObjectToHClip(positionOS);
				o.positionWS = TransformObjectToWorld(positionOS);
				o.forwardWS = TransformObjectToWorldDir(forwardOS);
                return o;
            }

            f2o frag (v2f i)
            {
				UNITY_SETUP_INSTANCE_ID(i);
                f2o o;
            	
                float4 albedoAlpha = 0;
				float3 normalWS = 0;
				float depthExtrude = 0;

            	float2 uv = i.uv0;
            	float weight = 1;// directionNWeight.w;
            	float bias = (1-weight) * 2;

            	float4 sample = SAMPLE_TEXTURE2D_BIAS(_AlbedoAlpha,sampler_AlbedoAlpha, uv,bias);
            	albedoAlpha += sample * weight;
            	
            	float4 normalDepth = SAMPLE_TEXTURE2D_BIAS(_NormalDepth, sampler_NormalDepth, uv,bias);
            	normalWS += normalDepth.rgb * weight;
            	depthExtrude += normalDepth.a * weight;
				normalWS = normalWS * 2 - 1;
				normalWS = normalize(normalWS);
				normalWS = TransformObjectToWorldNormal(normalWS);
                
                float diffuse = saturate(dot(normalWS,_MainLightPosition.xyz)) ;
                
                float3 albedo = albedoAlpha.rgb;
            	clip(albedoAlpha.a- INSTANCE(_AlphaClip));

                o.result = float4(albedo * diffuse * _MainLightColor + albedo * SHL2Sample(normalWS,unity),1);
				depthExtrude = depthExtrude * 2 -1;
                o.depth = EyeToRawDepth(TransformWorldToEyeDepth(i.positionWS + normalize(i.forwardWS) * saturate(depthExtrude)));
                return o;
            }
        ENDHLSL
    	
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
    		Blend Off
			Tags{"LightMode" = "UniversalForward"}
            ZTest LEqual
            ZWrite On
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
    }
}
