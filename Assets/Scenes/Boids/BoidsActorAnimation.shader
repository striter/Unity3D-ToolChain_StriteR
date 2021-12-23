Shader "Hidden/BoidsActorAnimation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Emission("Emission",Color)=(0,0,0,0)
        _Scale("Scale",Range(0.5,2))=1
        _Speed("Speed",Range(0.1,20))=1
        
        _Anim1("Anim1",Range(0,1))=1
        _Anim2("Anim1",Range(0,1))=1
        _Anim3("Anim1",Range(0,1))=1
        _Anim4("Anim1",Range(0,1))=1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" }
		HLSLINCLUDE
        #include "Assets/Shaders/Library/Common.hlsl"
		#include "Assets/Shaders/Library/Lighting.hlsl"
		#pragma multi_compile_instancing
        INSTANCING_BUFFER_START
        INSTANCING_PROP(float,_Scale)
        INSTANCING_PROP(float,_Speed)
        float _Anim1;
        float _Anim2;
        float _Anim3;
        float _Anim4;
        float4 _Emission;
        INSTANCING_BUFFER_END
		float3 GetAnimPositionOS(float3 positionOS,float2 uv1,float2 uv2)
		{
            positionOS.y+=uv1.x* _Anim1*INSTANCE(_Scale);
            positionOS.y+=uv1.y*_Anim2*INSTANCE(_Scale);
            positionOS.y+=uv2.x* _Anim3*INSTANCE(_Scale);
            positionOS.y+=uv2.y*_Anim4*INSTANCE(_Scale);
			return positionOS;
		}
		#define GETANIMATEDVERTEX(i) GetAnimPositionOS(i.positionOS,i.uv1,i.uv2) 
		
		ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct a2f
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1:TEXCOORD1;
                float2 uv2:TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (a2f v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
                o.positionCS = TransformObjectToHClip(GETANIMATEDVERTEX(v));
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv)+_Emission;
                return col;
            }
            ENDHLSL
        }	
    	
    	Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			
			struct a2f
			{
				float3 positionOS:POSITION;
            	float2 uv1:TEXCOORD1;
            	float2 uv2:TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
			};

			v2f DepthVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.positionCS=TransformObjectToHClip(GETANIMATEDVERTEX(v));
				return o;
			}

			float4 DepthFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}		
    			
    	Pass
		{
			Blend Off
			ZWrite On
			ZTest LEqual
			
			NAME "MAIN"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			
			struct a2f
			{
				A2V_SHADOW_CASTER;
                float2 uv1:TEXCOORD1;
                float2 uv2:TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				SHADOW_CASTER_VERTEX(v,TransformObjectToWorld(GETANIMATEDVERTEX(v)));
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}	
    }
}
