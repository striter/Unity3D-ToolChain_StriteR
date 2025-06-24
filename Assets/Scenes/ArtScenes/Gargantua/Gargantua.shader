Shader "Hidden/Gargantua"
{
    Properties
    {
        
        [Header(Gas Disc)]
        _DiskRadius("Disk Radius",Range(0,1))=0.1
        _GasDiskTexture("Texture",2D)="black"{}
        [ColorUsage(false,true)]_GasDiskColor("Color",Color)=(1,1,1,1)
        
        [Header(Haze)]
        _HazeOffset("Haze Offset",Range(-1,1))=0
        _TorusMajorRadius("Major Radius",Range(0,.5))=0.5
        _TorusMinorRadius("Minor Radius",Range(0,.5))=0.005
        [MinMaxRange]_HazeRange("Torus Gradient Range",Range(-.1,1))=0.01
        [HideInInspector]_HazeRangeEnd("",float)=0.5
        [ColorUsage(false,true)]_Color("Color Tint",Color)=(1,1,1,1)
        
        [Header(Warp)]
        _WarpAmount("Warp Amount",Range(0,1))=0.1
    }
    SubShader
    {
        Cull Front
        ZTest Always
        ZWrite On
        Blend One One
        Tags {"Queue" = "Transparent"}
        
        //&https://www.shadertoy.com/view/lstSRS
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            #define ITERATIONS 256
            struct a2v
            {
                float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID  
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 positionHCS : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_GasDiskTexture);SAMPLER(sampler_GasDiskTexture);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float,_HazeOffset)
                INSTANCING_PROP(float,_HazeRange)
                INSTANCING_PROP(float,_HazeRangeEnd)
                INSTANCING_PROP(float,_TorusMajorRadius)
                INSTANCING_PROP(float,_TorusMinorRadius)
                INSTANCING_PROP(float,_WarpAmount)

                INSTANCING_PROP(float,_DiskRadius)
                INSTANCING_PROP(float3,_GasDiskColor)
                INSTANCING_PROP(float4,_GasDiskTexture_ST)
            INSTANCING_BUFFER_END

            float3 WarpSpace(GTorus torus,float3 direction,float3 position)
            {
                float3 origin = torus.center;
                float singularityDist = distance(position, origin);
                float warpFactor = 1 / (pow(singularityDist, 2.0) + 0.0001);
                float3 singularityDir = normalize(origin - position);
                return normalize(direction + singularityDir * warpFactor * _WarpAmount / ITERATIONS);
            }
            
            float3 Haze(GTorus torus,float3 pos)
            {
                float sdf = torus.SDF(pos - float3(0,_HazeOffset,0));
                float torusBlurred = saturate(1 - invlerp( _HazeRange, _HazeRangeEnd,sdf));
                torusBlurred *= step(sdf,.5);
                torusBlurred *= pow(torusBlurred, 2.0);
                return torusBlurred;
            }

            float3 GasDisk(float3 pos)
            {
                GCylinderCapped cylinder = GCylinderCapped_Ctor(0, _TorusMajorRadius ,.001);

                float2 coords = pos.xz - cylinder.cylinder.center.xz;
                coords = CartesianToPolar(coords);
                coords = TransformTex_Flow(coords,_GasDiskTexture_ST);

                float cylinderSDF = cylinder.SDF(pos);
                float coverage = saturate(invlerp(_DiskRadius,0,cylinderSDF));
                coverage *= saturate(invlerp(.03,0.01,abs(cylinder.cylinder.center.y - pos.y)));
                coverage *= step(0.04,cylinderSDF);
                coverage = pow(coverage,2);
                return SAMPLE_TEXTURE2D(_GasDiskTexture,sampler_GasDiskTexture,coords) * coverage / ITERATIONS;
            }
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS = o.positionCS;
                return o;
            }

            #define SceneSDF(xxx) SDF_TorusLink(xxx) 
            SDFSurface SDF_TorusLink(float3 position)
            {
                float3 origin=0;
                // GTorus torus=GTorus_Ctor(origin,_TorusMajorRadius,_TorusMinorRadius);
                GCylinderRound cylinder = GCylinderRound_Ctor(0, _TorusMajorRadius - _DiskRadius,.001, _DiskRadius * 2);
                return SDFSurface_Ctor(cylinder.SDF(position),0);
            }
            
            #include "Assets/Shaders/Library/Geometry/GeometrySDFOutput.hlsl"
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float2 ndc = TransformHClipToNDC(i.positionHCS);


                float3 color;
                float3 originWS = GetCameraPositionWS();
                float3 directionWS = TransformNDCToFrustumCornersRay(ndc);

                float3 positionOS = TransformWorldToObject(originWS);
                float3 directionOS = TransformWorldToObjectDir(directionWS);

                GSphere sphereOS = GSphere_Ctor(0,.5);
                positionOS += sphereOS.SDF(positionOS) * directionOS;
                
                GTorus torusOS = GTorus_Ctor(0, _TorusMajorRadius, _TorusMinorRadius);

                float2 distances = Distance(sphereOS,GRay_Ctor(positionOS,directionOS));
                float2 startEndOS = float2(min(distances.x,distances.y,0),max(distances.x,distances.y,0));

                startEndOS.y *= 2;

                // SDFHitInfo result;
                // if(!RaymarchSDF(GRay_Ctor(positionOS,directionOS),startEndOS.x,startEndOS.y,result))
                // {
                // clip(-1);
                // return 0;
                // }
                
                // return 1;
                
                float marchStepOS = (startEndOS.y - startEndOS.x) / ITERATIONS;
                // float dither = dither01((_Time.y - floor(_Time.y)) * 144);
                // positionOS += directionOS * dither * marchStepOS;
                for (int iteration = 0; iteration < ITERATIONS; iteration++)
                {
                    directionOS = WarpSpace(torusOS,directionOS, positionOS);
                    positionOS += directionOS * marchStepOS;
                    color += Haze(torusOS, positionOS) * _Color;
                    color += GasDisk(positionOS) * _GasDiskColor;
                }
                return float4(color,1);
            }
            ENDHLSL
        }
    }
}
