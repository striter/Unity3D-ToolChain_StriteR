Shader "Hidden/GPUSkinning"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        HLSLINCLUDE
		#include "Assets/Shaders/Library/Common.hlsl"
		#include "Assets/Shaders/Library/Lighting.hlsl"
		#define _NORMALOFF
		#define _PBROFF
		#define _EMISSIONOFF
		TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
        #define A2V_ADDITIONAL float4 bindIndexes : TEXCOORD1; float4 bindWeights : TEXCOORD2;
		INSTANCING_BUFFER_START
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float4,_Color)
		INSTANCING_BUFFER_END
        StructuredBuffer<float4x4> _BoneMatrices;
        float4x4 GetSkinningMatrix(uint4 indexes,float4 weights) {
                return _BoneMatrices[indexes.x] * weights.x
                    + _BoneMatrices[indexes.y] * weights.y
                    + _BoneMatrices[indexes.z]* weights.z
                    + _BoneMatrices[indexes.w] * weights.w;
        }

		void ApplicationToVertex(inout float3 positionOS,inout float3 normalOS,inout float4 tangentOS,float4 bindIndexes,float4 bindWeights)
		{
            float3 positionBS = positionOS;
            float4x4 skinningMatrix = GetSkinningMatrix(bindIndexes, bindWeights);
			positionOS = mul(skinningMatrix, float4(positionBS,1)).xyz;
			normalOS = mul(skinningMatrix, float4(normalOS,0)).xyz;
			tangentOS.xyz = mul(skinningMatrix, float4(tangentOS.xyz,0)).xyz;
		}
		#define A2V_TRANSFER(v) ApplicationToVertex(v.positionOS,v.normalOS,v.tangentOS,v.bindIndexes,v.bindWeights);
		
        ENDHLSL
    
		Pass
		{
			NAME "Main"
			Tags{"LightMode" = "UniversalForward"}
			Cull Off
			HLSLPROGRAM
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			ENDHLSL
		}
		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull Off
			
			HLSLPROGRAM
			
            #include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}

		Pass
		{
			NAME "DEPTH"
			Tags{"LightMode" = "DepthOnly"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull Off
			
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
            #include "Assets/Shaders/Library/Passes/DepthOnly.hlsl"
			ENDHLSL
		}

		Pass
		{
            Tags{"LightMode" = "SceneSelectionPass"}
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull Off

            HLSLPROGRAM
            #pragma vertex VertexSceneSelection
            #pragma fragment FragmentSceneSelection
            #include "Assets/Shaders/Library/Passes/SceneOutlinePass.hlsl"
            ENDHLSL
		}
    }
}
