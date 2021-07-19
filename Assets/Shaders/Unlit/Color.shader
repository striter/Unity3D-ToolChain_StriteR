Shader "Game/Unlit/Color"
{
	Properties
	{
		_MainTex("Main Tex",2D)="white"{}
	    [HDR]_Color("HDR Color",Color)=(1,1,1,1)

		[Header(Misc)]
		[Toggle(_CSFORWARD)]_ClipSpaceForward("Clip Space Forward",float)=0
		[Foldout(_CSFORWARD)]_ClipSpaceForwardAmount("Forward Amount",Range(0.01,1))=.2
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
	}

	SubShader
	{ 
		Tags {"RenderType" = "HDREmitter" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		
		Lighting Off Fog { Color(0,0,0,0) }
		ZTest [_ZTest]
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Cull Back 
			name "MAIN"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma shader_feature _CSFORWARD

			#include "../CommonInclude.hlsl"
			struct appdata
			{
				float3 positionOS : POSITION;
				float4 color : COLOR;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 color : TEXCOORD0;
				float2 uv:TEXCOORD1;
			};

			sampler2D _MainTex;
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float,_ClipSpaceForwardAmount)
			INSTANCING_BUFFER_END

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float4 positionOS= TransformObjectToHClip(v.positionOS);
				#if _CSFORWARD
				float forward=_ClipSpaceForwardAmount*positionOS.z*Z_Multiply;
				positionOS.z-=forward;
				#endif
				o.positionCS =positionOS;
				o.uv = v.uv;
				o.color = v.color*INSTANCE(_Color);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex,i.uv).r*i.color;
			}
			ENDHLSL
		}

	}
}
