Shader "Game/Unfinished/GITest"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ GI_OVERRIDE GI_INTERPOLATE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Assets/Shaders/Library/Lighting/GIOverride.hlsl"
            
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
            	float3 viewDirWS:TEXCOORD1;
            	V2F_LIGHTMAP(0)
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
            	o.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(v.positionOS));
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
            	LIGHTMAP_TRANSFER(v,o);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

            	float3 viewDirWS = normalize(i.viewDirWS);
            	float3 normalWS=normalize(i.normalWS);
            	float3 reflectDir = normalize(reflect(-viewDirWS,normalWS));
            	Light mainLight = GetMainLight();
            	float3 indirectDiffuse = IndirectDiffuse(mainLight,i,normalWS);
            	float3 indirectSpecular = IndirectSpecular(reflectDir,1,0);
            	return float4(indirectDiffuse+indirectSpecular*pow2(1-saturate(dot(viewDirWS,normalWS))),1);
            }
            ENDHLSL
        }
    	
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
