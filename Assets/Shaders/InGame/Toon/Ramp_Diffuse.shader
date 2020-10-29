
Shader "Game/Toon/Diffuse_Ramp" {

	Properties{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Sub Color",color) = (1,1,1,1)
		_Outline("Thick of Outline",range(0,0.1)) = 0.02
		_Factor("Factor",range(0,1)) = 0.5
		_OutLineColor("OutLineColor",Color) = (1,1,1,1)
		_DiffuseSegment("Diffuse Segment",Vector)=(.1,.3,.6,1.0)
		_ActiveRamp("Activate Ramp",Range(0,1))=1
		_RampTexture("Ramp",2D)="white"{}
	}

		SubShader{

			pass {	
		Name "OutLine"
			Tags{"LightMode" = "Always"}
			Offset 1,1
			Cull Front
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			float _Outline;
			float _Factor;
			float4 _OutLineColor;
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
				dir = dir * _Factor + dir2 * (1 - _Factor);
				o.pos = UnityObjectToClipPos(v.vertex+dir*_Outline);
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				return _OutLineColor;
			}
			ENDCG
			}

			pass {		//Toon Model/Light Pass
			Tags{"LightMode" = "ForwardBase"}
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _LightColor0;
			float4 _Color;
			float _Steps;
			float _ToonEffect;
			float4 _DiffuseSegment;
			half _ActiveRamp;
			sampler2D _RampTexture;
			struct a2f
			{
				float4 vertex:POSITION;
				float4 normal:NORMAL;
				float2 uv:TEXCOORD0;
			};
			struct v2f {
				float4 pos:SV_POSITION;
				float3 lightDir:TEXCOORD0;
				float3 viewDir:TEXCOORD1;
				float3 normal:TEXCOORD2;
				float2 uv:TEXCOORD3;
			};

			v2f vert(a2f v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.viewDir = ObjSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX( v.uv, _MainTex);
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				float4 c = tex2D(_MainTex,i.uv);
				float3 N = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);
				float3 lightDir = normalize(i.lightDir);
				float diff = max(0, dot(N, i.lightDir));
			diff = (diff + 1) / 2;

				if (_ActiveRamp == 1)
				{
					diff = tex2D(_RampTexture, float2(diff, diff)).r;
				}
				else
				{
					fixed w = fwidth(diff) * 2.0;
					if (diff < _DiffuseSegment.x + w) {
						diff = lerp(_DiffuseSegment.x, _DiffuseSegment.y, smoothstep(_DiffuseSegment.x - w, _DiffuseSegment.x + w, diff));
					}
					else if (diff < _DiffuseSegment.y + w) {
						diff = lerp(_DiffuseSegment.y, _DiffuseSegment.z, smoothstep(_DiffuseSegment.y - w, _DiffuseSegment.y + w, diff));
					}
					else if (diff < _DiffuseSegment.z + w) {
						diff = lerp(_DiffuseSegment.z, _DiffuseSegment.w, smoothstep(_DiffuseSegment.z - w, _DiffuseSegment.z + w, diff));
					}
					else {
						diff = _DiffuseSegment.w;
					}
				}

				float4 lightColor = _LightColor0 * (diff);
				c = c * _Color * lightColor ;
				return c;
			}
			ENDCG
			}
	}
}