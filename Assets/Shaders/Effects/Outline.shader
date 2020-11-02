Shader "Game/Effects/Outline"
{
    Properties
    {
        _OutlineColor("Color",Vector)=(0,0,0,0)
        _OutlineWidth("Width",Range(0,1))=0.1
        _OutlineSmoothFactor("Smooth Factor",Range(0,1))=1
    }
    SubShader
    {
		
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }

        Pass 
        {	
            ZWrite Off
		    Name "OutLine"
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			float _OutlineWidth;
			float _OutlineSmoothFactor;
			float4 _OutlineColor;
			struct appdata
			{
				float4 vertex:POSITION;
				float3 normal:NORMAL;
			};
			struct v2f {
				float4 pos:SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;
				float3 dir = normalize(v.vertex.xyz);
				float3 dir2 = normalize(v.normal);
				dir = dir * sign(dot(dir, dir2));
				dir = dir * _OutlineSmoothFactor + dir2 * (1 - _OutlineSmoothFactor);
				o.pos = UnityObjectToClipPos(v.vertex+dir*_OutlineWidth);
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				return _OutlineColor;
			}
		ENDCG
		}
    }
}
