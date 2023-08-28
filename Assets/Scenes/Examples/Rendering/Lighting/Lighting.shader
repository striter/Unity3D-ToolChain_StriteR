Shader "Game/Unfinished/Lighting"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct FLightBufferElement
            {
                uint type;
                float3 position;
                float3 direction;
                float3 color;
                float4 lightParameters;
                float3 Radiance(float3 _positionWS,float3 _normalWS)
                {
                    float3 radiance;
                    float3 lightOffset = position - _positionWS;
                    float3 lightDirection = normalize(lightOffset);
                    float d = length(lightOffset);
                    
                    float3 L = lightDirection;
                    float3 R = direction;
                    float3 N = _normalWS;
                    switch (type)
                    {
                        case 0u:    //Directional
                            {
                                radiance = color;
                                radiance *= max(dot(N,-R),0);
                            }
                            break;
                        case 1u:         //Point
                            {
                                float attenuation =  1;
                                    attenuation /= (lightParameters.x + d*lightParameters.y + d*d*lightParameters.z);
                                radiance = color * attenuation;
                                radiance *= max(dot(N,L),0);
                            }
                            break;
                        case 2u:         //Spot
                            {
                                
                                float attenuation =  pow(max(-dot(R,L),0),lightParameters.w);
                                    attenuation /= (lightParameters.x + d*lightParameters.y + d*d*lightParameters.z);
                                radiance = color * attenuation;
                                radiance *= max(dot(N,L),0);
                            }
                            break;
                    }
                    return radiance ;
                }
            };
            int _LightCount;
            StructuredBuffer<FLightBufferElement> _LightArray;
            
            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS:TEXCOORD1;
                float3 normalWS:TEXCOORD2;
                float3 viewDirWS:TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            SHL2Input()
            
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_BUFFER_END

            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS = GetCameraRealDirectionWS(o.positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 positionWS = i.positionWS;
                float3 normalWS = normalize(i.normalWS);
                float3 viewDirWS = normalize(i.viewDirWS);
                float3 indirectDiffuse = SHL2Sample(normalWS,unity);
                
                float3 albedo=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb * INSTANCE(_Color).rgb;
            
                float3 output = albedo * indirectDiffuse;
                int count = _LightCount;
                for(int lightIndex = 0; lightIndex < count ; lightIndex++)
                {
                    FLightBufferElement light  =_LightArray[lightIndex];
                    float3 radiance = light.Radiance(positionWS,normalWS);
                    output += albedo * radiance;
                }
                
                return float4(output,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
