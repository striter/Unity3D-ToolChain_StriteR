Shader "Game/Unfinished/GITest"
{
    Properties
    {
		[Foldout(LIGHTMAP_ON,LIGHTMAP_INTERPOLATE)]_LightmapST("CLightmap UV",Vector)=(1,1,1,1)
		[Foldout(LIGHTMAP_ON,LIGHTMAP_INTERPOLATE)]_LightmapIndex("CLightmap Index",int)=0
		[Foldout(LIGHTMAP_INTERPOLATE)]_LightmapInterpolateST("CLightmap Interpolate UV",Vector)=(1,1,1,1)
		[Foldout(LIGHTMAP_INTERPOLATE)]_LightmapInterpolateIndex("CLightmap Interpolate Index",int)=0
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_LightmapST)
			    INSTANCING_PROP(float,_LightmapIndex)
				INSTANCING_PROP(float4,_LightmapInterpolateST)
			    INSTANCING_PROP(float,_LightmapInterpolateIndex)
            INSTANCING_BUFFER_END
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ ENVIRONMENT_CUSTOM ENVIRONMENT_INTERPOLATE
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                A2V_LIGHTMAP
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                V2F_LIGHTMAP(1)
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS=TransformObjectNormalToWorld(v.normalOS);
                LIGHTMAP_TRANSFER(v,o)
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                Light mainLight = GetMainLight();
                float3 finalCol=IndirectDiffuse(mainLight,i,i.normalWS)*.5f;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Hidden/DepthOnly/MAIN"
        USEPASS "Hidden/ShadowCaster/MAIN"
    }
}
