Shader "Game/Effects/VertexDistort"
{
	Properties
	{
		_MainTex("Main Tex",2D)="black"{}
		_DistortDirection("Distort Direction",Vector)=(0,1,0,0)
		_DistortStrength("Distort Strength",Range(0,5))=1
		[HDR]_DistortColor("Distort Color",Color)=(1,1,1,1)
		_DistortColorPow("Distort Color Range",Range(0,1))=.5
		_DistortFlow("Distort",Range(0,10))=1
	}
	SubShader
	{
		Tags { "Queue" = "Geometry-1" }
		Blend Off

		HLSLINCLUDE
			
			#include "../CommonInclude.hlsl"
			#include "../CommonLightingInclude.hlsl"
			TEXTURE2D( _MainTex);SAMPLER(sampler_MainTex);
			INSTANCING_BUFFER_START
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float3,_DistortDirection)
			INSTANCING_PROP(float,_DistortStrength)
			INSTANCING_PROP(float3,_DistortColor)
			INSTANCING_PROP(float,_DistortColorPow)
			INSTANCING_PROP(float,_DistortFlow)
			INSTANCING_BUFFER_END

			float3 GetDistortPositionWS(float3 positionOS,float3 normalWS,out float strength)
			{
					strength=saturate( invlerp(0,1, dot(normalWS,INSTANCE(_DistortDirection))));
				return INSTANCE(_DistortDirection)*random3(frac(positionOS+floor(INSTANCE(_DistortFlow)*_Time.y)/100))*strength*INSTANCE(_DistortStrength);
			}
		ENDHLSL
		Pass
		{
			Name "Main"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

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
				float strength:COLOR;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert(a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
				float3 normalWS= TransformObjectToWorldNormal( v.normalOS);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				positionWS+=GetDistortPositionWS(v.positionOS,normalWS, o.strength);
				o.positionCS = TransformWorldToHClip(positionWS);
				o.strength=saturate(invlerp(INSTANCE(_DistortColorPow) ,1,o.strength));
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 finalCol=lerp(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv),INSTANCE(_DistortColor),i.strength).rgb;
				return float4(finalCol,1);
			}
			ENDHLSL
	}
		
		Pass
		{
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex vertshadow
			#pragma fragment fragshadow
			#pragma multi_compile_instancing

			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vertshadow(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				float strength=0;
				positionWS+=GetDistortPositionWS(v.positionOS,TransformObjectToWorldNormal(v.normalOS), strength);
				v.positionOS = TransformWorldToObject(positionWS);
				SHADOW_CASTER_VERTEX(v,o);
				return o;
			}

			float4 fragshadow(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}
		
		Pass
		{
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vertshadow
			#pragma fragment fragshadow

			float4 vertshadow(float3 positionOS:POSITION,float3 normalOS:NORMAL):SV_POSITION
			{
				float3 positionWS=TransformObjectToWorld(positionOS);
				float strength=0;
				positionWS+=GetDistortPositionWS(positionOS,TransformObjectToWorldNormal(normalOS), strength);
				return TransformWorldToHClip(positionWS);
			}

			float4 fragshadow() :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}
	}
}
