Shader "Game/Lit/Transparecy/Glass"
{
    Properties
    {
        [ToggleTex(_NORMALTEX)]_NormalTex("Normal Tex",2D)="white"{}
    	_DistortStrength("Distort Strength",Range(0,0.1))=0.05
    	_ConnectionBegin("Connection Begin",Range(0,1))=0.1
    	_ConnectionWidth("Connection Width",Range(0,1))=0.1
    	
    	[Header(Specular)]
        [HDR]_ColorTint("Color Tint",Color)=(1,1,1,1)
    	_SpecularStrength("Specular Strength",Range(0,5))=1
    	_SpecularGlossiness("Specular Glossiness",Range(0,1))=0.75
    }
    SubShader
    {
        ZWrite On
        Blend One One
    	Cull Back
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _NORMALTEX

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float4 tangentOS:TANGENT;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
				half3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
            	float depthDistance:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_ColorTint)
                INSTANCING_PROP(float4,_NormalTex_ST)
				INSTANCING_PROP(float,_DistortStrength)
				INSTANCING_PROP(float,_SpecularGlossiness)
				INSTANCING_PROP(float,_ConnectionBegin)
            	INSTANCING_PROP(float,_ConnectionWidth)
            INSTANCING_BUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS=o.positionCS;
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _NormalTex);
                o.positionWS=TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.depthDistance=-TransformWorldToView(o.positionWS).z;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3 normalTS=half3(0,0,1);
            	float3 cameraDirWS=GetCameraRealDirectionWS(i.positionWS);
            	float3 viewDirWS=-cameraDirWS;
            	float3 reflectDirWS=normalize(reflect(cameraDirWS, normalWS));
            	float3 lightDirWS=normalize(_MainLightPosition.xyz);
            	float3 halfDirWS=normalize(lightDirWS+viewDirWS);
            	float2 screenUV=TransformHClipToNDC(i.positionHCS)+normalTS.xy*INSTANCE(_DistortStrength)/i.depthDistance;
            	float depthDistance=RawToDistance(SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r,screenUV);
				float depthOffset=depthDistance-i.depthDistance;
            	float depthParameter=invlerp(INSTANCE(_ConnectionBegin)+INSTANCE(_ConnectionWidth),INSTANCE(_ConnectionBegin),depthOffset);
            	
				#if _NORMALTEX
					float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv));
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
            	
                float ndv=dot(viewDirWS,normalWS);
            	float fresnel=pow5(1-ndv)+saturate(depthParameter);
                float3 indirectSpecular = IndirectCubeSpecular(reflectDirWS,0) * INSTANCE(_ColorTint.rgb);
            	
            	float3 finalCol=indirectSpecular;
				float alpha=saturate(fresnel);
            	
				float specular= GetSpecular(normalWS,halfDirWS,INSTANCE(_SpecularGlossiness))*INSTANCE(_SpecularGlossiness);
				finalCol +=_MainLightColor.rgb*specular;
            	alpha +=saturate(specular);
            	
                return float4(finalCol*saturate(alpha),1);
            }
            ENDHLSL
        }
    }
}
