Shader "Hidden/ProceduralNoise"
{
    Properties
    {
    }
    SubShader
    {
    HLSLINCLUDE
            
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
            #pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
			#pragma editor_sync_compilation

            StructuredBuffer<uint> _Hashes;
            StructuredBuffer<float3> _Positions;
			StructuredBuffer<float3> _Normals;
			float4 _Config;
            void ConfigureProcedural()
            {
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                float v = floor(_Config.y*unity_InstanceID + 0.00001);
                float h = unity_InstanceID - _Config.x * v;
                unity_ObjectToWorld = 0;
                unity_ObjectToWorld._m03_m13_m23_m33 = float4( _Positions[unity_InstanceID],1);
                unity_ObjectToWorld._m00_m11_m22 = _Config.y;
                unity_ObjectToWorld._m03_m13_m23 += (_Config.z * (1.0/255.0)*((_Hashes[unity_InstanceID]>>24)-.5)*_Normals[unity_InstanceID]);
#endif
            }

            float3 OverrideAlbedo()
            {
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                uint hash = _Hashes[unity_InstanceID];
                return (1.0/255.0)*float3(hash&255,(hash>>8)&255,(hash>>16)&255);
#endif
            	return 1;
            }
        ENDHLSL
    	
        Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
            #define _NORMALOFF
			#define _EMISSIONOFF
			#define _PBROFF
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_BUFFER_END
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
					
			#define GET_ALBEDO(i) OverrideAlbedo()
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}
        USEPASS "Game/Additive/DepthOnly/MAIN"
    }
}
