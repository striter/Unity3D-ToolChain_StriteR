Shader "Game/Unfinished/LiquidVolume"
{
    Properties
    {
        [ToggleTex(_NORMALTEX)]_NormalTex("Normal Tex",2D)="white"{}
    	_NormalOffset("Normal Offset",Range(-1,1))=-.1
    	_NormalStrength("Normal Strength",Range(0,1))=1
    	_DepthDistance("Depth Distance",Range(0,1))=0.5
    	_DepthWidth("Depth Width",Range(0,1))=0.1
    	[Vector2]_NormalFlow("Distort Strength",Vector)=(0.05,0.05,1,1)
        _ColorTint("Color Tint",Color)=(1,1,1,1)
    	_DistortStrength("Distort Strength",Range(0,0.1))=0.05
    }
    SubShader
    {
        ZWrite Off
        Blend Off
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
            TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_NormalStrength)
				INSTANCING_PROP(float,_NormalOffset)
                INSTANCING_PROP(float4,_ColorTint)
                INSTANCING_PROP(float4,_NormalTex_ST)
				INSTANCING_PROP(float2,_NormalFlow)
				INSTANCING_PROP(float,_DistortStrength)
				INSTANCING_PROP(float,_DepthDistance)
				INSTANCING_PROP(float,_DepthWidth)
            INSTANCING_BUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
            	float3 positionWS=TransformObjectToWorld(v.positionOS);

            	float wave=1 + (sin((_Time.y+v.positionOS.y*5)*PI)+1)*.5;
            	positionWS+=o.normalWS*(INSTANCE(_NormalOffset)*wave);
                o.positionWS=positionWS;
				o.depthDistance=-TransformWorldToView(o.positionWS).z;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.positionHCS=o.positionCS;
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _NormalTex)+INSTANCE(_NormalFlow)*_Time.y;
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
            	
				#if _NORMALTEX
					float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
					normalTS=lerp(normalTS,DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv)),INSTANCE(_NormalStrength));
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
            	
            	float2 screenUV=TransformHClipToNDC(i.positionHCS)+normalTS.xy*INSTANCE(_DistortStrength)/i.depthDistance;
            	float depthDistance=RawToDistance(SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r,screenUV);
            	float depthOffset=depthDistance-i.depthDistance;
                float ndv=dot(viewDirWS,normalWS);
            	
            	float3 opaqueCol=SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,screenUV);
            	
				float thickness=min(ndv,depthOffset);
            	float thicknessParam=saturate(invlerp(INSTANCE(_DepthDistance),INSTANCE(_DepthDistance)+INSTANCE(_DepthWidth),thickness));
            	float3 finalCol= lerp(opaqueCol,_ColorTint.rgb,_ColorTint.a*thicknessParam);
            	
            	float fresnel=pow5(1-ndv);
            	
            	finalCol += _MainLightColor*_ColorTint.rgb*fresnel;
            	
                return float4(finalCol,1);
            }
            ENDHLSL
        }
    }
}
