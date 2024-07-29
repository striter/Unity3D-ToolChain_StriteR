Shader "Game/Unfinished/Imposter"
{
    Properties
    {
    	_AlphaClip("Clip",Range(0,1)) = 0.5
        [NoScaleOffset]_MainTex("_MainTex",2D) = "white"
        [NoScaleOffset]_NormalDepthTex("_NormalDepthTex",2D) = "white"
    }
    SubShader
    {
    	HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/Geometry.hlsl"
            #include "Imposter.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
            	float4 uv0 : TEXCOORD0;
            	float4 uv1 : TEXCOORD1;
            	float4 uv2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 uv0:TEXCOORD0;
            	float4 uv1:TEXCOORD1;
				float4 uv2:TEXCOORD2;
                float3 positionWS :TEXCOORD4;
            	float4 positionHCS : TEXCOORD5;
            	float3 positionOS : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct f2o
            {
                float4 result : SV_TARGET;
                float depth : SV_DEPTH;
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalDepthTex);SAMPLER(sampler_NormalDepthTex);
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Rotation)
            INSTANCING_BUFFER_END

            float3 quaternionMul(float4 quaternion,float3 direction)
            {
	            float3 t = 2 * cross(quaternion.xyz, direction);
				return direction + quaternion.w * t + cross(quaternion.xyz, t);
            }
            
			float3 _ImposterViewDirection;
            float _AlphaClip;
            float3 _Weights;

			float4 GetColumn(float4x4 _matrix,int _index)
			{
				return float4(_matrix[0][_index],_matrix[1][_index],_matrix[2][_index],_matrix[3][_index]);
			}
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
            	o.positionOS = v.positionOS;
            	o.positionHCS = o.positionCS;
				o.uv0 = v.uv0;
				o.uv1 = v.uv1;
				o.uv2 = v.uv2;
                return o;
            }

			float4 GetFragmentUV(v2f i,int index)
            {
	            switch (index)
	            {
		            case 0: return i.uv0;
		            case 1: return i.uv1;
		            case 2: return i.uv2;
	            }
            	return i.uv0;
            }
            
            f2o frag (v2f i)
            {
				UNITY_SETUP_INSTANCE_ID(i);
                f2o o;
            	
                float4 albedoAlpha = 0;
				float3 normalWS = 0;
				float depthExtrude = 0;

            	for(int index=0;index<3;index++)
            	{
            		float4 uv = GetFragmentUV(i,index);
            		float4 directionNWeight = _Weights[index];
            		float weight = directionNWeight.w;
            		albedoAlpha += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, uv.xy) * weight;
            		float4 normalDepth = SAMPLE_TEXTURE2D(_NormalDepthTex, sampler_NormalDepthTex, uv.xy);
            		normalWS += normalDepth.rgb * weight;
            		depthExtrude += normalDepth.a * weight;
            	}
				normalWS = normalWS * 2 - 1;
				normalWS = quaternionMul(_Rotation,normalWS);
                
                float diffuse = saturate(dot(normalWS,normalize(_MainLightPosition))) ;
                
                float3 albedo = albedoAlpha.rgb;
            	clip(albedoAlpha.a-_AlphaClip);

                o.result = float4(albedo * diffuse * _MainLightColor + albedo * SHL2Sample(normalWS,unity),albedoAlpha.a);
                o.depth = EyeToRawDepth(TransformWorldToEyeDepth(TransformObjectToWorld(i.positionOS + _ImposterViewDirection * saturate(depthExtrude))));
                return o;
            }
        ENDHLSL
    	
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
        
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
				
			ENDHLSL
		}
    }
}
