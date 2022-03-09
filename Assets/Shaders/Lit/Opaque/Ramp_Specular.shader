Shader "Game/Lit/Opaque/Ramp_Specular"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color("Color",Color)=(1,1,1,1)
		[Header(Diffuse Setting)]
		[NoScaleOffset]_Ramp("Ramp Tex",2D)="white"{}
		[Toggle(_RAMP_RIM_V)]_RampRimV("2D Ramp(Rim as V)",float)=0
		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularRange("Specular Range",Range(.9,1))=.98
		[Header(Rim Setting)]
		[KeywordEnum(None,Hard,Smooth)]_Rim("Rim Type",float)=1
		_RimRange("Rim Range",Range(0,1))=0.5
	}
	SubShader
	{
		Cull Back
		Blend Off
		
		Pass
		{
			Tags{"LightMode" = "UniversalForward"}
			Cull Back
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _RAMP_RIM_V
			#pragma multi_compile_local _RIM_NONE _RIM_HARD _RIM_SMOOTH
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
		
			TEXTURE2D(_Ramp); SAMPLER(sampler_Ramp);
			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

			INSTANCING_BUFFER_START
			INSTANCING_PROP(float4,_Color)
			INSTANCING_PROP(float4, _MainTex_ST)
			INSTANCING_PROP(float4 ,_SpecularColor)
			INSTANCING_PROP(float ,_SpecularRange)
			INSTANCING_PROP(float ,_RimRange)
			INSTANCING_BUFFER_END

			struct a2v
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float3 normalWS:TEXCOORD0;
				float2 uv : TEXCOORD1;
				float3 positionWS:TEXCOORD2;
				float3 viewDirWS:TEXCOORD3;
				float4 shadowCoordWS:TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			v2f vert (a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv = TRANSFORM_TEX_INSTANCE( v.uv, _MainTex);
				o.positionWS=TransformObjectToWorld(v.positionOS);
				o.normalWS=TransformObjectToWorldNormal(v.normalOS);
				o.viewDirWS=GetCameraPositionWS()-o.positionWS;
				o.shadowCoordWS=TransformWorldToShadowCoord(o.positionWS);
				return o;
			}
			

			float4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS=normalize(i.normalWS);
				return float4(i.normalWS,1);
				float3 lightDirWS=normalize(_MainLightPosition.xyz);
				float3 viewDirWS = normalize(i.viewDirWS);
				
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				float3 ambient=_GlossyEnvironmentColor.rgb;
				float3 lightCol=_MainLightColor.rgb;
				float3 albedo = SAMPLE_TEXTURE2D (_MainTex,sampler_MainTex, i.uv).rgb*INSTANCE( _Color).rgb+ambient;
				float diffuse = GetDiffuse(normalWS, lightDirWS);
				diffuse*=atten;
				float rim=dot(normalWS,viewDirWS);
				float2 rampUV=diffuse;
				#if _RAMP_RIM_V
				rampUV.y=rim;
				#endif
				float3 diffuseCol = lightCol*SAMPLE_TEXTURE2D(_Ramp,sampler_Ramp,rampUV).rgb;
				float3 finalCol=albedo*(diffuseCol);

				#if _RIM_HARD
				float range= INSTANCE( _RimRange);
				rim=step(rim,range)*step(0.001,range);
				finalCol=lerp(finalCol,lightCol,rim);
				#elif _RIM_SMOOTH
				rim=(1-rim)*INSTANCE( _RimRange);
				finalCol+= lightCol*rim;
				#endif
			
				#if _SPECULAR
				float specular = GetSpecular(normalWS,lightDirWS,viewDirWS,INSTANCE( _SpecularRange));
				specular*=atten;
				specular=1-step(specular,0);
				finalCol += specular*lightCol ;
				#endif
				return float4(finalCol ,1);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}
