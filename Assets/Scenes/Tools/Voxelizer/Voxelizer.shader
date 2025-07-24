Shader "Hidden/Voxelizer"
{
    Properties
    {
        [NoScaleOffset]_MainTex("Main Tex",3D)="white"{}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 positionHCS : TEXCOORD0;
                float3 positionWS: TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE3D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS = o.positionCS;
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                GBox cube = GBox_Ctor(TransformObjectToWorld(0),1);
                GRay ray = GRay_Ctor(GetCameraPositionWS(),GetCameraRealDirectionWS(i.positionWS));

                float2 distances = Distance(ray,cube);

                if(distances.y  <=  0.001f)
                    return 0;

                int stepCount = 64;
                float marchStep = distances.y / stepCount;

                int iteration = 0;
                float marchDistance = max(0, distances.x);
                float3 color = 0;
                float3 normal = 0;
                while(iteration++ < 128)
                {
                    marchDistance += marchStep;
                    float3 marchPos = ray.GetPoint(marchDistance);
                    float sample = SAMPLE_TEXTURE3D(_MainTex,sampler_MainTex,cube.GetNormalizedPoint(marchPos)).x;
                    if(sample > 0)
                    {
                        color = 1;
                        normal.x += SAMPLE_TEXTURE3D(_MainTex,sampler_MainTex,cube.GetNormalizedPoint(marchPos + float3(marchStep,0,0))).x - sample;
                        normal.y += SAMPLE_TEXTURE3D(_MainTex,sampler_MainTex,cube.GetNormalizedPoint(marchPos + float3(0,marchStep,0))).x - sample;
                        normal.z += SAMPLE_TEXTURE3D(_MainTex,sampler_MainTex,cube.GetNormalizedPoint(marchPos + float3(0,0,marchStep))).x - sample;
                        break;
                    }
                }
                color = saturate(color);
                normal = normalize(normal);
                float NDL = dot(normal,-normalize(_MainLightPosition.xyz));
                float halfLambert = NDL * .5 + .5;
                return float4(saturate(color) * halfLambert,max(color));
            }
            ENDHLSL
        }
    }
}
