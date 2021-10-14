Shader "Hidden/AlphaBlend"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Intensity("Intensity",Range(5,20))=5
		_Alpha("Alpha",Range(0,1)) = 1
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" "PreviewType" = "Plane"} 
		Blend SrcAlpha OneMinusSrcAlpha
		Cull [_Cull]
		ZTest [_ZTest]
		Lighting Off 
		ZWrite Off
		
		Pass
		{		
			name "Main"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/Common.hlsl"
			#pragma multi_compile_instancing

			INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_Alpha)
			INSTANCING_BUFFER_END
			
			struct a2v
			{
				float3 positionOS : POSITION;
				float4 color:COLOR;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 positionCS : SV_POSITION;
				float4 color:TEXCOORD0;
				float2 uv:TEXCOORD1;
			};
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Intensity;
			

			v2f vert(a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return float4(i.color.rgb*_Intensity,saturate(tex2D(_MainTex,i.uv).r*INSTANCE(_Alpha)*_Intensity));
			}
			ENDHLSL
		}
	}
}
