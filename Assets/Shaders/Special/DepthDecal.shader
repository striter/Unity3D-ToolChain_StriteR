Shader "Game/Special/DepthDecal"
{
	Properties
	{
		_MainTex("Decal Texture",2D) = "white"{}
	}
		SubShader
	{
		Tags{"Queue" = "Geometry+1"}
		Pass
	{
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
		struct v2f
		{
			float4 pos:SV_POSITION;
			float4 screenPos: TEXCOORD0;
			float3 ray:TEXCOORD1;
		};
		sampler2D _MainTex;
		sampler2D _CameraDepthTexture;

		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.screenPos = ComputeScreenPos(o.pos);
			o.ray = UnityObjectToViewPos(v.vertex).xyz*float3(-1, -1, 1);
			return o;
		}

		float4 frag(v2f i):SV_Target
		{
		i.ray = i.ray*(_ProjectionParams.z / i.ray.z);
		float2 uv = i.screenPos.xy / i.screenPos.w;
		float depth = Linear01Depth(  SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
		float3 vpos = i.ray*depth;
		float3 wpos = mul(unity_CameraToWorld, float4(vpos,1));
		float3 opos = mul(unity_WorldToObject, float4(wpos,1));
		return tex2D(_MainTex, opos.xy + .5);

		}

		ENDCG
		
	}
	}

		FallBack Off
}
