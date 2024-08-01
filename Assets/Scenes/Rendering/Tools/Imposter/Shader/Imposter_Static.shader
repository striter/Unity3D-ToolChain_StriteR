Shader "Game/Optimize/Imposter/Static"
{
    Properties
    {
    	_AlphaClip("Clip",Range(0,1)) = 0.5
        [NoScaleOffset]_Static("_Static",2D) = "white"
	    _Weights("Weights",Vector) = (1,1,1,1)
    }
    SubShader
    {
    	Blend Off
    	HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
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

            TEXTURE2D(_Static);SAMPLER(sampler_Static);
            
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
            
             float4 frag (v2f i) : SV_TARGET
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float4 albedoAlpha = 0;
            	for(int index=0;index<3;index++)
            	{
            		float4 uv = GetFragmentUV(i,index);
            		float4 directionNWeight = _Weights[index];
            		float weight = directionNWeight.w;
            		float bias = (1-weight) * 2;
            		albedoAlpha += SAMPLE_TEXTURE2D_BIAS(_Static,sampler_Static, uv.xy,bias) * weight;
            	}
            	clip(albedoAlpha.a-_AlphaClip);
                return float4(albedoAlpha.rgb,1);
            }
        ENDHLSL
    	
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
			Tags{"LightMode" = "UniversalForward"}
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
    }
}
