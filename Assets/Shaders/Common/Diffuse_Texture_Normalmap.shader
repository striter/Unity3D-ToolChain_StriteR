Shader "Game/Common/Diffuse_Texture_Normalmap"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Normal Map",2D) = "white"{}
	}
		SubShader
	{
	Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"
		#pragma multi_compile_instancing

		sampler2D _MainTex;
		half4 _MainTex_ST;
		sampler2D _BumpMap;

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent:TANGENT;
			float3 normal:NORMAL;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 tLightDir:TEXCOORD3;
			float3 worldPos:TEXCOORD4;
			SHADOW_COORDS(5)
		};
		struct v2fa
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 tLightDir:TEXCOORD3;
			float3 worldPos:TEXCOORD4;
		};

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			o.pos = UnityObjectToClipPos(v.vertex);

			o.uv = TRANSFORM_TEX(v.uv,_MainTex);

			TANGENT_SPACE_ROTATION;

			o.tLightDir = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);

			TRANSFER_SHADOW(o);

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			float3 tangentLightDir = normalize(i.tLightDir);
			float3 tangentNormal = UnpackNormal(tex2D(_BumpMap,i.uv));

			fixed3 albedo = tex2D(_MainTex, i.uv);

			fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz*albedo;

			fixed3 diffuse = _LightColor0.rgb*albedo*max(0, dot(tangentLightDir, tangentNormal));

			UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);

			return fixed4(ambient + diffuse * atten,1);
		}

		ENDCG


		Pass
		{
			Tags{"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			ENDCG
		}

		Pass
		{
			Name "ForwardAdd"
			Tags{"LightMode" = "ForwardAdd"}
			Blend One One
			CGPROGRAM
			#pragma multi_compile_fwdadd
			#pragma vertex vertAdd
			#pragma fragment fragAdd

		v2fa vertAdd(appdata v)
		{
			v2fa o;
			o.pos = UnityObjectToClipPos(v.vertex);

			o.uv = TRANSFORM_TEX(v.uv,_MainTex);

			TANGENT_SPACE_ROTATION;

			o.tLightDir = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);

			return o;
		}
		fixed4 fragAdd(v2fa i) :SV_TARGET
		{
			float3 tangentLightDir = normalize(i.tLightDir);
			float3 tangentNormal = UnpackNormal(tex2D(_BumpMap,i.uv));

			fixed3 albedo = tex2D(_MainTex, i.uv);

			fixed3 diffuse = _LightColor0.rgb*albedo*max(0, dot(tangentLightDir, tangentNormal));

			UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);

			return fixed4(diffuse * atten,1);
		}
			ENDCG
		}

		USEPASS "Game/Common/Diffuse_Base/SHADOWCASTER"
	}
}