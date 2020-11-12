Shader "Game/Particle/Diffuse"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags{ "RenderType" = "Opaque" "IgnoreProjector" = "True" "Queue" = "Geometry" "PreviewType" = "Sphere"}
			Cull Back Lighting Off ZWrite On ZTest On Fog { Color(0,0,0,0) }

			Pass
			{
				name "Main"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "../CommonLightingInclude.cginc"
				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"
				#pragma multi_compile_instancing
				struct appdata
				{
					float4 vertex : POSITION;
					float4 color:COLOR;
					float3 normal:NORMAL;
					float2 uv:TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float4 color:TEXCOORD0;
					float2 uv:TEXCOORD1;
					float3 worldPos:TEXCOORD2;
					float3 objNormal:TEXCOORD3;
					float3 objLightDir:TEXCOORD4;
				};
				sampler2D _MainTex;
				float4 _MainTex_ST;
				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				UNITY_INSTANCING_BUFFER_END(Props)

				v2f vert(appdata v)
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.color = v.color*UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					o.objNormal=v.normal;
					o.objLightDir=ObjSpaceLightDir(v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
					float diffuse=GetDiffuse(normalize( i.objNormal),normalize( i.objLightDir));
					return float4( tex2D(_MainTex, i.uv)*i.color*(UNITY_LIGHTMODEL_AMBIENT.xyz+diffuse *  _LightColor0.rgb*atten+(1-atten)),1);
				}
				ENDCG
			}
		}
}
