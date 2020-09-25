Shader "Hidden/AnimationInstance_Bone_Sample"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGINCLUDE
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AnimationInstanceInclude.cginc"
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float2 uv : TEXCOORD0;
                float4 boneIndexes:TEXCOORD1;
                float4 boneWeights:TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float diffuse:TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                SampleBoneInstance(v.boneIndexes,v.boneWeights, v.vertex, v.normal);
                o.diffuse=dot(v.normal,ObjSpaceLightDir(v.vertex));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

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
                float4 boneIndexes:TEXCOORD1;
                float4 boneWeights:TEXCOORD2;
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
                SampleBoneInstance(v.boneIndexes,v.boneWeights, v.vertex, v.normal);
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
