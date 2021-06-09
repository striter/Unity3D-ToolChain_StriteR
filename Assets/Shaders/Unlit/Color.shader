Shader "Game/Unlit/Color"
{
	Properties
	{
	    [HDR]_Color("HDR Color",Color)=(1,1,1,1)
		_ShapeTexture("Shape",2D)="white"{}
	}

	SubShader
	{ 
		Tags {"RenderType" = "HDREmitter" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		
		Lighting Off Fog { Color(0,0,0,0) }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Cull Back 
			name "MAIN"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
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

			sampler2D _ShapeTexture;
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4, _Color)
			INSTANCING_BUFFER_END

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv = v.uv;
				o.color = v.color*INSTANCE(_Color);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return tex2D(_ShapeTexture,i.uv).r*i.color;
			}
			ENDHLSL
		}

	}
}
