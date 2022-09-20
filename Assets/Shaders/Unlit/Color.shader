Shader "Game/Unlit/Color"
{
	Properties
	{
		[NoScaleOffset]_MainTex("Mask Tex",2D)="white"{}
	    [HDR]_Color("HDR Color",Color)=(1,1,1,1)

		[Header(Misc)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
		[Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("Color Mask",int)=15
		[Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
	}

	SubShader
	{ 
		Tags {"Queue" = "Transparent" }
		
		ZTest [_ZTest]
		Cull [_Cull]
		ColorMask [_ColorMask]
		ZWrite [_ZWrite]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Cull Back 
			name "MAIN"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			
			#include "Assets/Shaders/Library/Common.hlsl"
			struct a2v
			{
				float3 positionOS : POSITION;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD1;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4, _Color)
			INSTANCING_BUFFER_END

			v2f vert(a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float4 positionOS= TransformObjectToHClip(v.positionOS);
				o.positionCS =positionOS;
				o.uv = v.uv;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * INSTANCE(_Color);
				return col;
			}
			ENDHLSL
		}

	}
}
