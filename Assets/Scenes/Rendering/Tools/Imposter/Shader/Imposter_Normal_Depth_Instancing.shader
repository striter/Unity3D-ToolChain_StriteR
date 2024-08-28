Shader "Game/Optimize/Imposter/Normal_Depth_Instancing"
{
    Properties
    {
    	_AlphaClip("Clip",Range(0,1)) = 0.5
    	[Toggle(_INTERPOLATE)]_Interpolate("Interpolate",int) = 1
    	_Parallax("Parallax",Range(0,1)) = 0.1
    	
        [NoScaleOffset]_AlbedoAlpha("_AlbedoAlpha",2D) = "white"
        [NoScaleOffset]_NormalDepth("_NormalDepth",2D) = "white"
    	_ImposterTexel("Texel",Vector)=(1,1,1,1)
    	_ImposterBoundingSphere("Bounding Sphere",Vector)=(1,1,1,1)
    	[KeywordEnum(CUBE,OCTAHEDRAL,CONCENTRIC_OCTAHEDRAL,CENTRIC_HEMISPHERE,OCTAHEDRAL_HEMISPHERE)]_MAPPING("Sphere Mode",int) = 4
    }
	CustomEditor "Runtime.Optimize.Imposter.ImposterShaderGUI"
    SubShader
    {
    	Blend Off
    	HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/Geometry.hlsl"

            #pragma multi_compile_instancing
            #pragma shader_feature_local _INTERPOLATE
			#pragma shader_feature_vertex _MAPPING_CUBE _MAPPING_OCTAHEDRAL _MAPPING_CONCENTRIC_OCTAHEDRAL _MAPPING_CENTRIC_HEMISPHERE _MAPPING_OCTAHEDRAL_HEMISPHERE

            TEXTURE2D(_AlbedoAlpha);SAMPLER(sampler_AlbedoAlpha);
            TEXTURE2D(_NormalDepth);SAMPLER(sampler_NormalDepth);
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_AlphaClip)
				INSTANCING_PROP(float,_Parallax)
            INSTANCING_BUFFER_END

            #include "Imposter.hlsl"
            struct a2v
            {
            	float4 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            	#if _INTERPOLATE
            		float4 uv01 : TEXCOORD1;
            		float4 uv23 : TEXCOORD2;
            		float4 uvWeights : TEXCOORD3;
            	#else
					float2 uv0 : TEXCOORD0;
            	#endif
            	float3 positionWS : TEXCOORD4;
            	float3 forwardWS : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct f2o
            {
                float4 result : SV_TARGET;
                // float depth : SV_DEPTH;
            };

            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float3 positionOS = 0;
            	float3 viewPositionOS = TransformWorldToObject(_WorldSpaceCameraPos);
				float3 forwardOS = viewPositionOS - _BoundingSphere.xyz;
            	
            	#if _INTERPOLATE
					ImposterVertexEvaluate_Bilinear(v.uv,INSTANCE(_Parallax),forwardOS, positionOS,o.uv01,o.uv23,o.uvWeights);
            	#else
					ImposterVertexEvaluate(v.uv,forwardOS, positionOS, o.uv0);
            	#endif
                o.positionCS = TransformObjectToHClip(positionOS);
				o.positionWS = TransformObjectToWorld(positionOS);
				o.forwardWS = TransformObjectToWorldDir(forwardOS);
                return o;
            }

			void Sample(float2 _uv,float _weight,inout float4 albedoAlpha,inout float3 normalOS,inout float depthExtrude)
            {
            	float2 uv = _uv.xy;
            	float weight = _weight;
            	float bias = (1-weight) * 2;

            	float4 sample = SAMPLE_TEXTURE2D_BIAS(_AlbedoAlpha,sampler_AlbedoAlpha, uv,bias);
            	albedoAlpha += sample * weight;

            	float4 normalDepth = SAMPLE_TEXTURE2D_BIAS(_NormalDepth, sampler_NormalDepth, uv,bias);
            	normalOS += normalDepth.rgb * weight;
            	depthExtrude += normalDepth.a * weight;
            }

            f2o frag (v2f i)
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float4 albedoAlpha = 0;
				float3 normalOS = 0;
				float depthExtrude = 0;
            	#if _INTERPOLATE
					Sample(i.uv01.xy,i.uvWeights.x,albedoAlpha,normalOS,depthExtrude);
					Sample(i.uv01.zw,i.uvWeights.y,albedoAlpha,normalOS,depthExtrude);
					Sample(i.uv23.xy,i.uvWeights.z,albedoAlpha,normalOS,depthExtrude);
					Sample(i.uv23.zw,i.uvWeights.w,albedoAlpha,normalOS,depthExtrude);
            	#else
					Sample(i.uv0,1,albedoAlpha,normalOS,depthExtrude);
            	#endif

                float3 albedo = albedoAlpha.rgb;
            	clip(albedoAlpha.a - INSTANCE(_AlphaClip));

				float3 normalWS = normalOS * 2 - 1;
				normalWS = normalize(normalWS);
				normalWS = TransformObjectToWorldNormal(normalWS);
				depthExtrude = depthExtrude * 2 -1;
                float diffuse = saturate(dot(normalWS,_MainLightPosition.xyz)) ;

                f2o o;
                o.result = float4(albedo * diffuse * _MainLightColor.rgb + albedo * SHL2Sample(normalWS,unity),1);
            	// o.result = float4(i.uv3.z,0,0,1);
                // o.depth = EyeToRawDepth(TransformWorldToEyeDepth(i.positionWS + normalize(i.forwardWS) * saturate(depthExtrude)));
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
			Tags{"LightMode"="ShadowCaster"}
            Cull Off
            HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
				
			struct a2fSC
			{
				A2V_SHADOW_CASTER;
            	half2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2fSC
			{
				V2F_SHADOW_CASTER;
            	#if _INTERPOLATE
            		float4 uv01 : TEXCOORD1;
            		float4 uv23 : TEXCOORD2;
            		float4 uvWeights : TEXCOORD3;
            	#else
					float2 uv0 : TEXCOORD0;
            	#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2fSC ShadowVertex(a2fSC v)
			{
				v2fSC o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float3 positionOS = 0;
            	float3 viewPositionOS = TransformWorldToObjectDir(_MainLightPosition.xyz);
				
            #if _INTERPOLATE
				ImposterVertexEvaluate_Bilinear(v.uv,INSTANCE(_Parallax),viewPositionOS, positionOS,o.uv01,o.uv23,o.uvWeights);
            #else
				ImposterVertexEvaluate(v.uv,viewPositionOS, positionOS, o.uv0);
            #endif
				
				SHADOW_CASTER_VERTEX(v,TransformObjectToWorld(positionOS));
				return o;
			}

			float4 ShadowFragment(v2fSC i) :SV_TARGET
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float4 albedoAlpha = 0;
				float3 normalOS = 0;
				float depthExtrude = 0;
            #if _INTERPOLATE
				Sample(i.uv01.xy,i.uvWeights.x,albedoAlpha,normalOS,depthExtrude);
				Sample(i.uv01.zw,i.uvWeights.y,albedoAlpha,normalOS,depthExtrude);
				Sample(i.uv23.xy,i.uvWeights.z,albedoAlpha,normalOS,depthExtrude);
				Sample(i.uv23.zw,i.uvWeights.w,albedoAlpha,normalOS,depthExtrude);
            #else
				Sample(i.uv0,1,albedoAlpha,normalOS,depthExtrude);
            #endif
            	clip(albedoAlpha.a - INSTANCE(_AlphaClip));
				return 0;
			}
            ENDHLSL
		}
    }
}
