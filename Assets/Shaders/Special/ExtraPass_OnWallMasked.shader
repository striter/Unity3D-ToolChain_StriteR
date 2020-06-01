Shader "Hidden/ExtraPass/OnWallMasked"
{
	Properties
	{
		_MaskColor("Masked Color",Color) = (1,1,1,.5)
		_MaskParam1("Masked Param 1",float) = 1
		_MaskParam2("Masked Param 2",float) = 1000
	}
		SubShader
	{
		CGINCLUDE
		#include "UnityCG.cginc"
		float4 _MaskColor;
		ENDCG
		Pass
		{
			Name "SimpleColor"
			Blend SrcAlpha One
			ZWrite Off
			Cull Back
			ZTest Greater
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			struct v2f
			{
				float4 pos:SV_POSITION;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			fixed4 frag(v2f i) :SV_TARGET
			{
				return _MaskColor;
			}
			ENDCG
		}

		Pass
		{
			Name "Rim"
			Blend SrcAlpha One
			ZWrite Off
			Cull Off
			ZTest Greater
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			struct v2f
			{
				float4 pos:SV_POSITION;
				float rim : TEXCOORD0;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 normal =normalize(v.normal);
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.rim = (1 - pow(dot(normal, viewDir), 2));
				return o;
			}
			fixed4 frag(v2f i) :SV_TARGET
			{
				return i.rim *_MaskColor;
			}
				ENDCG
			}

				Pass
			{ 
				Name "TexFlow"
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }		
			Blend SrcAlpha One
			ZWrite Off
			ZTest Greater
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			float _MaskParam1;
			float _MaskParam2;
			struct v2f
			{
				float4 pos:SV_POSITION;
				float4 screenPos:TEXCOORD0;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			fixed4 frag(v2f i) :SV_TARGET
			{
				float2 screenUV =i.screenPos.xy / i.screenPos.w;
				screenUV *= _MaskParam2;
				float mask = sin(screenUV.y+  _Time.y*_MaskParam1);
				return mask *_MaskColor;
			}
			ENDCG
		}
	}
}
