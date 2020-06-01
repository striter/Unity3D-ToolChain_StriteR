Shader "Game/Effect/Ice"
{
	Properties
	{
		_MainTex("MainTex",2D) = "white"{}
		_NoiseTex("DistortTex",2D)="white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		_IceColor("Ice Color",Color) = (1,1,1,1)
		_OpacityMultiple("Opacity Multiple",float) = .7
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Back
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				sampler2D _CameraOpaqueTexture;
				sampler2D _MainTex;
				sampler2D _NoiseTex;
				float4 _MainTex_ST;
				float4 _Color;
				float _OpacityMultiple;
				float4 _IceColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal:NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float rim : TEXCOORD1;
				float4 screenPos:TEXCOORD2;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 normal = normalize(v.normal);
				o.screenPos = ComputeScreenPos(o.vertex);
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.rim = saturate( 1 - dot(normal, viewDir)*2);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 albedo = tex2D(_MainTex, i.uv)*_Color;
				float3 foreCol =  albedo+ _IceColor*i.rim;
				fixed3 backCol = tex2D(_CameraOpaqueTexture, i.screenPos.xy / i.screenPos.w+tex2D(_NoiseTex,i.uv+float2(_Time.y/10,_Time.y/10)).rg).rgb;
				float3 finalCol = lerp(backCol, foreCol, _OpacityMultiple);
				return float4(finalCol, 1);
			}
			ENDCG
		}
	}
}
