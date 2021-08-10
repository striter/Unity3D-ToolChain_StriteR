Shader "Game/Effects/Depth/Decal"
{
	Properties
	{
		_MainTex("Decal Texture",2D) = "white"{}
		_Color("Decal Color",Color)=(1,1,1,1)
		[KeywordEnum(NONE, BOX,SPHERE)]_DECALCLIP("Decal Clip Volume",int)=0
	}
	SubShader
	{
		Tags{"Queue" = "Geometry+1"  "IgnoreProjector" = "True" "DisableBatching"="True"  }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			ZWrite Off
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local  _DECALCLIP_NONE _DECALCLIP_BOX _DECALCLIP_SPHERE
			
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            // #pragma multi_compile_local _ _MAIN_LIGHT_CALCULATE_SHADOWS
            // #pragma multi_compile_local _ _SHADOWS_SOFT

			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"

			struct v2f
			{
				half4 positionCS:SV_POSITION;
				half3 positionWS:TEXCOORD0;
				half3 viewDirWS:TEXCOORD1;
				half4 positionHCS: TEXCOORD2;
			};
			
			TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
			TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
			CBUFFER_START(UnityPerMaterial)
			half4 _MainTex_ST;
			half4 _Color;
			CBUFFER_END
			v2f vert(half3 positionOS:POSITION)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(positionOS);
				o.positionWS=TransformObjectToWorld(positionOS);
				o.viewDirWS=TransformWorldToViewDir(o.positionWS,UNITY_MATRIX_V);
				o.positionHCS = o.positionCS;
				return o;
			}

			half4 frag(v2f i):SV_Target
			{
				float2 ndc=TransformHClipToNDC(i.positionHCS);
				float3 positionWS=TransformNDCToWorld(ndc ,SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, TransformHClipToNDC(i.positionHCS)).r);
				half3 positionOS = TransformWorldToObject(positionWS);
				half2 decalUV=positionOS.xy+.5;
				decalUV=TRANSFORM_TEX(decalUV,_MainTex);
				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,decalUV)* _Color;
				#if _DECALCLIP_SPHERE
				color.a*=step(sqrDistance(positionOS),.25h);
				#elif _DECALCLIP_BOX
				color.a*=step(abs(positionOS.x),.5h)*step(abs(positionOS.y),.5h)*step(abs(positionOS.z),.5h);
				#endif
				
				// half atten=MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS));
				// color.a*=atten;
				color.rgb+=_GlossyEnvironmentColor.rgb;
				return color;
			}
			ENDHLSL
		}
	}
}
