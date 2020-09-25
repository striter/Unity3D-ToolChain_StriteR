Shader "Hidden/AnimationInstance_Vertex_Sample"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGINCLUDE
            #include "UnityCG.cginc"
            #include "AnimationInstanceInclude.cginc"
			#pragma multi_compile_instancing
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                uint vertexID:SV_VertexID;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float diffuse:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                int startIndex=v.vertexID*2;
                half4 vertex;
                half3 normal;
                SampleVertexInstance(v.vertexID, vertex, normal);
                o.diffuse=dot(normal,ObjSpaceLightDir(vertex));
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col*i.diffuse;
            }
            ENDCG
        }

        Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			CGPROGRAM
            #include "Lighting.cginc"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
            struct a2fs
            {
                half4 vertex:POSITION;
                half3 normal:NORMAL;
                uint vertexID:SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
				
			struct v2fs
			{
				V2F_SHADOW_CASTER;
			};

			v2fs ShadowVertex(a2fs v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2fs o;
                SampleVertexInstance(v.vertexID, v.vertex,v.normal);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			fixed4 ShadowFragment(v2fs i) :SV_TARGET
			{
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
    }
}
