Shader "Game/Effect/Bog"
{
	Properties
	{
		_MainTex("Main texture", 2D) = "white" {}
		_Color("Color", Color) = (0,1,0,1)
		_Mask("Bog Mask", 2D) = "white" {}
		_Vector("Speed UV, Pow Z, Emission W", Vector) = (0,0,1,0)
		_CutOff("Alpha CutOff", Float) = 1
	}


	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Fog { Color(0,0,0,0) }
			Blend SrcAlpha One
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 uv : TEXCOORD0;
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 uv : TEXCOORD0;
					
				};
				
			 sampler2D _MainTex;
			 float4 _MainTex_ST;
			 float4 _Vector;
			 float4 _Color;
			 sampler2D _Mask;
			 float4 _Mask_ST;
			 float _CutOff;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.uv = v.uv;
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					float2 uv = TRANSFORM_TEX(i.uv,_MainTex);
					float4 albedo = tex2D(_MainTex,uv + float2(-1,-1)* _Vector.xy*_Time.y)*tex2D(_MainTex,uv+ float2(.5,.5)* _Vector.xy*_Time.y)*_Color;

					float emmision = _Vector.w;

					float uvAlpha = pow( tex2D(_Mask, TRANSFORM_TEX(i.uv, _Mask)).r, _Vector.z);

					float alpha =albedo.a *uvAlpha >(0.99-_CutOff)?1:0;

					return float4(albedo.rgb,alpha);
				}
				ENDCG 
			}
		}	
	}
}