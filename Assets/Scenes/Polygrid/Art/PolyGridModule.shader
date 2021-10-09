Shader "Hidden/PolyGridModule"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	_Lambert("Lambert",Range(0,1))=.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		
    	Blend Off
    	ZWrite On
        HLSLINCLUDE
			#pragma multi_compile_instancing
			#pragma multi_compile _ _POP
            #include "Assets/Shaders/Library/Common.hlsl"
			#define IGI
			#include "Assets/Shaders/Library/Lighting.hlsl"
            float3 _PopPosition;
            float _PopRadiusSqr;
            float _PopStrength;

            float3 PopPosition(float3 _positionWS)
            {
                float distanceSqr=sqrDistance(_positionWS-_PopPosition);
                float dstMultiply=invlerp(0,_PopRadiusSqr,distanceSqr);
                float dstParam=step(dstMultiply,1);
                return lerp(_positionWS,_PopPosition,dstParam*(1-_PopStrength)*(1-dstMultiply));
            }
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            struct a2v
            {
                float3 positionOS : POSITION;
            	float3 normalOS:NORMAL;
            	float4 color:COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            	float3 normalWS:NORMAL;
            	float4 color:COLOR;
                float2 uv : TEXCOORD0;
            	float4 shadowCoordWS:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Lambert;

            v2f vert (a2v v)
            {
                v2f o;
                float3 positionWS=TransformObjectToWorld(v.positionOS);
            	#if _POP
                positionWS=PopPosition(positionWS);
            	#endif
                o.positionCS = TransformWorldToHClip(positionWS);
            	o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            	o.shadowCoordWS=TransformWorldToShadowCoord(positionWS);
            	o.color=v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
            	float3 lightDirWS=normalize(_MainLightPosition.xyz);
				float3 normalWS=normalize(i.normalWS);
            	
            	half3 indirectDiffuse=IndirectBRDFDiffuse(i.normalWS);
            	
                float3 col =indirectDiffuse*.5 + tex2D(_MainTex, i.uv).rgb;//*i.color.rgb;
            	half ndl=dot(normalWS,lightDirWS);
            	half atten=MainLightRealtimeShadow(i.shadowCoordWS);
            	half diffuse=ndl*atten*_Lambert+(1-_Lambert);
                return float4( col*diffuse,1);
            }
            ENDHLSL
        }
        
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment

			struct a2f
			{
				float3 positionOS:POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
            	#if _POP
                positionWS=PopPosition(positionWS);
            	#endif
				o.positionCS=TransformWorldToHClip(positionWS);
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}		
    	
		Pass
		{
			Blend Off
			ZWrite On
			ZTest LEqual
			
			NAME "MAIN"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			
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

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				float3 positionWS=TransformObjectToWorld(v.positionOS.xyz);
            	#if _POP
                positionWS=PopPosition(positionWS);
            	#endif
				SHADOW_CASTER_VERTEX(v,positionWS);
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}	
    }
}
