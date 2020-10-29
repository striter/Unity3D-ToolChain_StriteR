Shader "Game/Toon/Specular_Ramp"
{
	Properties
	{
		_Color("Color Tint",Color)=(1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Ramp("Ramp Tex",2D)="white"{}
		_Outline("Thick of Outline",range(0,0.1)) = 0.02
		_Factor("Factor",range(0,1)) = 0.5
		_OutLineColor("OutLineColor",Color) = (1,1,1,1)
		_Specular("Specular",Color)=(1,1,1,1)
		_SpecularScale("Specular Scale",Range(0,0.1))=0.01
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque"  "Queue" = "Geometry"}

		UsePass "Game/Toon/Diffuse_Ramp/OUTLINE"

			pass
			{
				Tags{"LightMode" = "ForwardBase"}
				Cull Back
				CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			fixed4 _Color;
			sampler2D _Ramp;
			sampler2D _MainTex;
			half4 _MainTex_ST;
			fixed4 _Specular;
			fixed _SpecularScale;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				float3 tangent:TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos:TEXCOORD1;
				float3 worldNormal:TEXCOORD2;
				SHADOW_COORDS(3)
			};


			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX( v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				TRANSFER_SHADOW(o);
				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
			fixed3 worldNormal = normalize(i.worldNormal);
			fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
			fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
			fixed3 worldHalfDir = normalize(worldLightDir + worldViewDir);


			fixed3 albedo = tex2D(_MainTex, i.uv).rgb*_Color.rgb;
			
			fixed3 ambientCol = UNITY_LIGHTMODEL_AMBIENT.xyz*albedo;

			UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
			fixed diffuse = dot(worldNormal, worldLightDir);
			diffuse = (diffuse*.5 + .5)*atten;
			fixed3 diffuseCol = _LightColor0.rgb*albedo*tex2D(_Ramp, float2(diffuse, diffuse)).rgb;
				
			fixed specular = dot(worldNormal, worldHalfDir);
			fixed w = fwidth(specular) * 2;
			specular = lerp(0, 1, smoothstep(-w, w, specular + _SpecularScale - 1))*step(0.0001, _SpecularScale);

				return fixed4( ambientCol+ diffuseCol+ _Specular.rgb*specular,1);
			}
			ENDCG
		}
	}
}
