Shader "Game/Effect/DepthVolumeFog_Vertical"
{
	Properties
	{
		_Color("Fog Color",Color)=(1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" ="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos:TEXCOORD0;
				float4 screenPos:TEXCOORD1;
			};

			float4 _Color;
			sampler2D _CameraDetphTexture;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
