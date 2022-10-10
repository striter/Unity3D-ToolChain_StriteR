Shader "Hidden/FilterableProcedurals"
{
    Properties
    {
        _Scale("Scale",Range(0.01,20))=1
        [Toggle(_FILTER)]_Filter("Filter",int)=1
		[KeywordEnum(CHECKER,GRID,SQUARE,CROSS,XOR)]_SHAPE("Shape",int)=0
        
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        Pass
        {
			Tags{"LightMode" = "UniversalForward"}
            
            HLSLPROGRAM
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma shader_feature_local_fragment _FILTER
			#pragma shader_feature_local_fragment _SHAPE_CHECKER _SHAPE_GRID _SHAPE_SQUARE _SHAPE_CROSS _SHAPE_XOR
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionWS:TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float,_Scale)
            INSTANCING_BUFFER_END

            float Checker(float2 uv)
            {
                #if _FILTER
                    float2 w = max(ddx(uv),ddy(uv));
                    float2 i = 2.0*(abs(frac((uv-0.5*w)/2.0)-0.5)-abs(frac((uv+0.5*w)/2.0)-0.5))/w;
                    return 0.5 - 0.5*i.x*i.y;          
                #else
                    float2 q = floor(uv);
                    return abs(fmod(q.x+q.y,2.0));
                #endif
            }

            #define N 10
            float Grid(float2 uv)
            {
                #if _FILTER
                    float2 w = max(ddx(uv),ddy(uv));
                    float2 a = uv+0.5*w;
                    float2 b = uv - 0.5*w;
                    float2 i = (floor(a)+min(frac(a)*N,1)-floor(b)-min(frac(b)*N,1))/(N*w);
                #else
                    float2 i = step( frac(uv), 1.0f/N);
                #endif
                return (1.0-i.x)*(1.0-i.y);
            }

            float Square(float2 uv)
            {
                #if _FILTER
                    float2 w = max(ddx(uv),ddy(uv));
                    float2 a = uv+0.5*w;
                    float2 b = uv - 0.5*w;
                    float2 i = (floor(a)+min(frac(a)*N,1)-floor(b)-min(frac(b)*N,1))/(N*w);
                #else
                    float2 i = step( frac(uv), 1.0f/N);
                #endif
                return 1.0-i.x*i.y;
            }

            float Cross(float2 uv)
            {
                #if _FILTER
                    float2 w = max(ddx(uv),ddy(uv));
                    float2 a = uv+0.5*w;
                    float2 b = uv - 0.5*w;
                    float2 i = (floor(a)+min(frac(a)*N,1)-floor(b)-min(frac(b)*N,1))/(N*w);
                #else
                    float2 i = step( frac(uv), 1.0f/N);
                #endif
                return 1.0-i.x-i.y+2.0*i.x*i.y;
            }

            float Xor(float2 uv)
            {
                float xor = 0;
                #if _FILTER
                    float2 dpdx = abs(ddx(uv));
                    float2 dpdy = abs(ddy(uv));
                    for(int i=0;i<8;i++)
                    {
                        float2 w = max(abs(dpdx), abs(dpdy)) + 0.01;  
                        float2 f = 2.0*(abs(frac((uv-0.5*w)/2.0)-0.5)-
		                              abs(frac((uv+0.5*w)/2.0)-0.5))/w;
                        xor += 0.5 - 0.5*f.x*f.y;
                        
                        dpdx *= 0.5;
                        dpdy *= 0.5;
                        uv   *= 0.5;
                        xor  *= 0.5;
                    }
                #else
                    for( int i=0; i<8; i++ )
                    {
                        xor += abs(fmod( floor(uv.x)+floor(uv.y), 2.0 ));

                        uv *= 0.5;
                        xor *= 0.5;
                    }
                #endif
                return xor;
            }
            
            float GetAlbedo(float2 uv)
            {
                #if _SHAPE_CHECKER
                    return Checker(uv);
                #elif _SHAPE_GRID
                    return Grid(uv);
                #elif _SHAPE_SQUARE
                    return Square(uv);
                #elif _SHAPE_CROSS
                    return Cross(uv);
                #elif _SHAPE_XOR
                    return Xor(uv);
                #endif
                    return Checker(uv);
            }
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 normalWS = normalize(i.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float ndl = saturate(dot(normalWS,mainLight.direction));

                float3 positionWS = i.positionWS;
                float2 uv =  positionWS.xz/_Scale;
                
                float3 albedo = INSTANCE(_Color).rgb * GetAlbedo(uv);
                float3 finalCol = IndirectDiffuse_SH(normalWS)*albedo + albedo*ndl*mainLight.shadowAttenuation*mainLight.color;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
