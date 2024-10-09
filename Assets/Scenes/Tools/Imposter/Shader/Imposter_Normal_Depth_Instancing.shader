Shader "Game/Optimize/Imposter/Normal_Depth_Instancing"
{
    Properties
    {
    	_AlphaClip("Clip",Range(0,1)) = 0.25
    	[Toggle(_INTERPOLATE)]_Interpolate("Interpolate",int) = 1
//    	[Foldout(_INTERPOLATE)]_Parallax("Parallax",Range(0,1)) = 0.1
    	
        [NoScaleOffset]_AlbedoAlpha("_AlbedoAlpha",2D) = "white"
        [NoScaleOffset]_NormalDepth("_NormalDepth",2D) = "white"
    	[Header(Constant)]_ImposterTexel("Texel",Vector)=(1,1,1,1)
    	_ImposterBoundingSphere("Bounding Sphere",Vector)=(1,1,1,1)
    	[KeywordEnum(CUBE,OCTAHEDRAL,CONCENTRIC_OCTAHEDRAL,CENTRIC_HEMISPHERE,OCTAHEDRAL_HEMISPHERE)]_MAPPING("Sphere Mode",int) = 4
    }
	CustomEditor "Runtime.Optimize.Imposter.ImposterShaderGUI"
    SubShader
    {
    	HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Imposter.hlsl"

            #pragma multi_compile_instancing
            #pragma shader_feature_local _INTERPOLATE
			#pragma shader_feature_vertex _MAPPING_CUBE _MAPPING_OCTAHEDRAL _MAPPING_CONCENTRIC_OCTAHEDRAL _MAPPING_CENTRIC_HEMISPHERE _MAPPING_OCTAHEDRAL_HEMISPHERE

            TEXTURE2D(_AlbedoAlpha);SAMPLER(sampler_AlbedoAlpha);
            TEXTURE2D(_NormalDepth);SAMPLER(sampler_NormalDepth);
            
			struct ImposterFragmentOutput
            {
				float4 albedoAlpha ;
				float3 normalWS;
				float depthExtrude;
            };
            
            struct f2o
            {
                float3 result : SV_TARGET;
                // float depth : SV_DEPTH;
            };
            
			void Sample(float2 _uv,float _weight,inout ImposterFragmentOutput o)
            {
            	float2 uv = _uv.xy;
            	float weight = _weight;
            	float bias = (1-weight) * 2;

            	float4 sample = SAMPLE_TEXTURE2D_BIAS(_AlbedoAlpha,sampler_AlbedoAlpha, uv,bias);
            	o.albedoAlpha += sample * weight;

            	float4 normalDepth = SAMPLE_TEXTURE2D_BIAS(_NormalDepth, sampler_NormalDepth, uv,bias);
            	o.normalWS += normalDepth.rgb * weight;
            	o.depthExtrude += normalDepth.a * weight;
            }

            void PostSample(inout ImposterFragmentOutput o)
			{
				o.normalWS = o.normalWS * 2 -1;
				o.normalWS = normalize(o.normalWS);
				o.normalWS = TransformObjectToWorldNormal(o.normalWS);
				o.depthExtrude = o.depthExtrude * 2 -1;
            	clip(o.albedoAlpha.a - INSTANCE(_AlphaClip));
			}

            f2o Output(float3 albedo,ImposterFragmentOutput imposterOutput,float3 positionWS,float3 forwardWS)
			{
				f2o o;
				o.result = albedo;
				float depthExtrude = imposterOutput.depthExtrude;
				return o;
			}

            #define A2V_IMPOSTER float2 uv:TEXCOORD0
            #define F2O_RESULT(i,imposterOutput,albedo) Output(albedo,imposterOutput,i.positionWS,i.forwardWS)
            #if _INTERPOLATE
				#define V2F_IMPOSTER float4 uv01 : TEXCOORD0; float4 uv23 : TEXCOORD1; float4 uvWeights:TEXCOORD2; float3 positionWS : TEXCOORD3; float3 forwardWS : TEXCOORD4
				#define V2F_IMPOSTER_TRANSFER(v,o) ImposterVertexEvaluate_Bilinear(v.uv,INSTANCE(_Parallax),o.forwardWS,o.positionWS,o.uv01,o.uv23,o.uvWeights);
            	ImposterFragmentOutput ImposterFragment(float4 uv01,float4 uv23, float4 uvWeights)
				{
					ImposterFragmentOutput o;
					ZERO_INITIALIZE(ImposterFragmentOutput,o);
					Sample(uv01.xy,uvWeights.x,o);
					Sample(uv01.zw,uvWeights.y,o);
					Sample(uv23.xy,uvWeights.z,o);
					Sample(uv23.zw,uvWeights.w,o);
					PostSample(o);
					return o;
				}
				#define F2O_IMPOSTER_FRAGMENT(i) ImposterFragment(i.uv01,i.uv23,i.uvWeights);
            #else
				#define V2F_IMPOSTER float2 uv0 : TEXCOORD0;float3 positionWS : TEXCOORD1; float3 forwardWS : TEXCOORD2
				#define V2F_IMPOSTER_TRANSFER(v,o) ImposterVertexEvaluate(v.uv,o.forwardWS, o.positionWS, o.uv0);
				ImposterFragmentOutput ImposterFragment(float2 uv01)
				{
					ImposterFragmentOutput o;
					ZERO_INITIALIZE(ImposterFragmentOutput,o);
					Sample(uv01.xy,1,o);
					PostSample(o);
					return o;
				}
				#define F2O_IMPOSTER_FRAGMENT(i) ImposterFragment(i.uv0);
            #endif
        ENDHLSL
    	
        Pass
        {
			Tags{"LightMode" = "UniversalForward"}
    		Blend Off
            ZTest LEqual
            ZWrite On
            Cull Off
            HLSLPROGRAM
            #pragma shader_feature_fragment _ _ENABLE_DEBUG_MODE
			
			float _CurrentLodForDebug;
			void GrassDebugMode(inout half3 col)
		    {
		        #if defined(_ENABLE_DEBUG_MODE)
		            if (_CurrentLodForDebug == 1)
		            {
		                // 当前为LOD1，设置为蓝色
		                col = half4(0, 0, 1, 1);
		            }
		            else if (_CurrentLodForDebug == 2)
		            {
		                // 当前为LOD2，设置为红色
		                col = half4(1, 0, 0, 1);
		            }
		        #endif
		    }
            
            struct a2v
            {
				A2V_IMPOSTER;
            	float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
				V2F_IMPOSTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (a2v v)
            {
                v2f o;
            	ZERO_INITIALIZE(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
            	V2F_IMPOSTER_TRANSFER(v,o);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            f2o frag (v2f i)
            {
				UNITY_SETUP_INSTANCE_ID(i);
				ImposterFragmentOutput output = F2O_IMPOSTER_FRAGMENT(i);
                float3 albedo = output.albedoAlpha.rgb;
                float diffuse = saturate(dot(output.normalWS,_MainLightPosition.xyz));
                float3 result =  albedo * diffuse * _MainLightColor.rgb + albedo * SHL2Sample(output.normalWS,unity);

            	f2o o = F2O_RESULT(i,output,result);
				GrassDebugMode(o.result);
                return o;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        
        Pass
        {
			Tags{"LightMode" = "DepthNormals"}
    		Blend Off
            ZTest LEqual
            ZWrite On
            Cull Off
            HLSLPROGRAM

            struct a2v
            {
				A2V_IMPOSTER;
            	float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
				V2F_IMPOSTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert (a2v v)
            {
                v2f o;
            	ZERO_INITIALIZE(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
            	V2F_IMPOSTER_TRANSFER(v,o);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_TARGET0
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	ImposterFragmentOutput imposterFragmentOutput = F2O_IMPOSTER_FRAGMENT(i);
                return float4(imposterFragmentOutput.normalWS,0);
            }
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

		Pass
		{
			Tags{"LightMode" = "DepthOnly"}
			
			Cull Off
			ZWrite On
			ZTest LEqual
			ColorMask R
			
			HLSLPROGRAM
            struct a2v
            {
				A2V_IMPOSTER;
            	float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
				V2F_IMPOSTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert (a2v v)
            {
                v2f o;
            	ZERO_INITIALIZE(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
            	V2F_IMPOSTER_TRANSFER(v,o);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            half frag (v2f i) : SV_TARGET0
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	ImposterFragmentOutput imposterFragmentOutput = F2O_IMPOSTER_FRAGMENT(i);
				return EyeToRawDepth(TransformWorldToEyeDepth(i.positionWS + normalize(imposterFragmentOutput.normalWS) * saturate(imposterFragmentOutput.depthExtrude) * _BoundingSphere.w * 2));
            }
            
            #pragma vertex vert
            #pragma fragment frag
			ENDHLSL
		}

		Pass
		{
			Tags{"LightMode" = "SceneSelectionPass"}
			
			Cull Off
			ZWrite On
			ZTest LEqual
			ColorMask R
			
			HLSLPROGRAM
            struct a2v
            {
				A2V_IMPOSTER;
            	float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
				V2F_IMPOSTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            v2f vert (a2v v)
            {
                v2f o;
            	ZERO_INITIALIZE(v2f,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
            	V2F_IMPOSTER_TRANSFER(v,o);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_TARGET
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	F2O_IMPOSTER_FRAGMENT(i);
				return 1;
            }
            #pragma vertex vert
            #pragma fragment frag
			ENDHLSL
		}

		Pass
		{
			Tags{"LightMode"="ShadowCaster"}
            Cull Off
            HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
				
			struct a2fSC
			{
				A2V_SHADOW_CASTER;
            	A2V_IMPOSTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2fSC
			{
				V2F_SHADOW_CASTER;
				V2F_IMPOSTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2fSC ShadowVertex(a2fSC v)
			{
				v2fSC o;
            	ZERO_INITIALIZE(v2fSC,o);
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
            	V2F_IMPOSTER_TRANSFER(v,o);
				SHADOW_CASTER_VERTEX(v,o.positionWS);
				return o;
			}

			float4 ShadowFragment(v2fSC i) :SV_TARGET
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	F2O_IMPOSTER_FRAGMENT(i);
				return 1;
			}
            ENDHLSL
		}

    }
}
