Shader "Game/Effects/Depth/Decal"
{
	Properties
	{
		_MainTex("Decal Texture",2D) = "white"{}
		_Color("Decal Color",Color)=(1,1,1,1)
		[KeywordEnum(NONE, BOX,SPHERE)]_DECALCLIP("Decal Clip Volume",int)=0
	}
	SubShader
	{
		Tags{"Queue" = "Geometry+1"  "IgnoreProjector" = "True" "DisableBatching"="True"  }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			ZWrite Off

			CGPROGRAM
			#pragma multi_compile  _DECALCLIP_NONE _DECALCLIP_BOX _DECALCLIP_SPHERE
			#pragma vertex vert
			#pragma fragment frag
            #include "UnityCG.cginc"
			#include "../../CommonInclude.cginc"
			struct a2f
			{
				float4 vertex:POSITION;
			};
			struct v2f
			{
				float4 pos:SV_POSITION;
				float4 screenPos: TEXCOORD0;
				float3 viewDir:TEXCOORD1;
				float3 worldPos:TEXCOORD2;
			};
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _CameraDepthTexture;
			float4 _Color;
			v2f vert(a2f v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				o.viewDir=WorldSpaceViewDir(v.vertex);
				o.worldPos=mul(unity_ObjectToWorld,v.vertex);
				return o;
			}

			float4 frag(v2f i):SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;
				float depthOffset = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r - i.screenPos.w;
				float3 wpos = i.worldPos-normalize(i.viewDir)*depthOffset;
				float3 opos = mul(unity_WorldToObject, float4(wpos,1));
				half2 decalUV=opos.xy+.5;
				decalUV=TRANSFORM_TEX(decalUV,_MainTex);
				float4 color=tex2D(_MainTex,decalUV)* _Color;
				#if _DECALCLIP_SPHERE
				color.a*=step(sqrDistance(opos),.25);
				#elif _DECALCLIP_BOX
				color.a*=step(abs(opos.x),.5)*step(abs(opos.y),.5)*step(abs(opos.z),.5);
				#endif
				return color;
			}
			ENDCG
			}
	}
}
