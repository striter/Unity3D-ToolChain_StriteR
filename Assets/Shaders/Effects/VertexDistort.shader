Shader "Game/Effects/VertexDistort"
{
	Properties
	{
		_MainTex("Main Tex",2D)="white"{}
		_DistortDirection("Distort Direction",Vector)=(0,1,0,0)
		_DistortStrength("Distort Strength",Range(0,5))=1
		[HDR]_DistortColor("Distort Color",Color)=(1,1,1,1)
		_DistortColorPow("Distort Color Range",Range(0,1))=.5
		_DistortFlow("Distort",Range(0,10))=1
	}
		SubShader
		{
			Tags { "Queue" = "Geometry" }
			Blend Off
			Pass
			{
				Name "Main"
				Tags {"LightMode"="ForwardBase"}
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "../CommonInclude.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal:NORMAL;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float3 normal:NORMAL;
					float strength:COLOR;
					float2 uv : TEXCOORD0;
					float3 lightDir:TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float3 _DistortDirection;
				float _DistortStrength;
				float3 _DistortColor;
				float _DistortColorPow;
				float _DistortFlow;
				v2f vert (appdata v)
				{
					v2f o;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.lightDir=WorldSpaceLightDir(v.vertex);
					o.normal= mul(unity_ObjectToWorld, v.normal);
					float3 worldPos=mul(unity_ObjectToWorld,v.vertex);

					float strength=saturate( invlerp(0,1, dot(o.normal,_DistortDirection)));
					worldPos+=_DistortDirection*random3(frac(v.vertex+floor(_DistortFlow*_Time.y)/100))*strength*_DistortStrength;
					o.strength=saturate(invlerp(_DistortColorPow ,1,strength));
					o.vertex = UnityWorldToClipPos(worldPos);
					return o;
				}
			
				fixed4 frag (v2f i) : SV_Target
				{
					float3 normal=normalize(i.normal);
					float3 lightDir=normalize(i.lightDir);
					float NDL=(dot(normal,lightDir)+1)/2;
					float3 albedo=tex2D(_MainTex,i.uv);
					float3 finalCol=lerp( albedo*NDL,_DistortColor,i.strength);
					return float4(finalCol,1);
				}
				ENDCG
		}
	}
}
