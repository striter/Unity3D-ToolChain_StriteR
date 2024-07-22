Shader "Game/Unfinished/Imposter"
{
    Properties
    {
        _MainTex("_MainTex",2D) = "white"
        [NoScaleOffset]_NormalTex("_NormalTex",2D)= "white"
        [NoScaleOffset]_PositionTex("_PositionTex",2D) = "white"
    }
    SubShader
    {
    	HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Imposter.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS :TEXCOORD1;
                float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct f2o
            {
                float4 result : SV_TARGET;
                float depth : SV_DEPTH;
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
            TEXTURE2D(_PositionTex);SAMPLER(sampler_PositionTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float,_Radius)
				INSTANCING_PROP(float4,_Rotation)
            INSTANCING_BUFFER_END

            float3 quaternionMul(float4 quaternion,float3 direction)
            {
	            float3 t = 2 * cross(quaternion.xyz, direction);
				return direction + quaternion.w * t + cross(quaternion.xyz, t);
            }
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            f2o frag (v2f i)
            {
				UNITY_SETUP_INSTANCE_ID(i);
                f2o o;
                float4 sample = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
                float3 normalWS = SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex, i.uv) * 2 -1;
				normalWS = quaternionMul(_Rotation,normalWS);
            	
            	float3 positionOS = (SAMPLE_TEXTURE2D(_PositionTex,sampler_PositionTex, i.uv) * 2 - 1) * _BoundingSphere.w + _BoundingSphere.xyz;
                float3 positionWS = TransformObjectToWorld(positionOS);
                
                float diffuse = dot(normalWS,normalize(_MainLightPosition)) ;
                
                float3 finalCol=  sample.rgb;
                clip(sample.a-0.01);
                o.result = float4(finalCol * diffuse+ SHL2Sample(normalWS,unity),sample.a);
                o.depth = EyeToRawDepth(TransformWorldToEyeDepth(positionWS,UNITY_MATRIX_V));
                return o;
            }
        ENDHLSL
    	
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On
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
