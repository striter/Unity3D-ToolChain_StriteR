Shader "Hidden/SDF_Combination"
{
    SubShader
    {
        Tags { "RenderType"="UniversalForward" "Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"

            #define SceneSDF(xxx) SDF_Combination(xxx)

            #define MAX_SDF_COUNT 128
            uniform int _SDFCount;
            float4 _Parameters1[MAX_SDF_COUNT];
            float4 _Parameters2[MAX_SDF_COUNT];
            float4 _Colors[MAX_SDF_COUNT];

            SDFSurface SDF_Sample(SDFSurface _src,float3 _position, int _index)
            {
                float4 parameter1 = _Parameters1[_index];
                float4 parameter2 = _Parameters2[_index];
                float3 origin = parameter1.xyz;

                float sdfDistance = -1;
                switch (parameter1.w)
                {
                    default : break;
                    case 0: sdfDistance = GCapsule_Ctor(origin,parameter2.x,float3(0,1,0),parameter2.y).SDF(_position);break;
                    case 1: sdfDistance = GSphere_Ctor(origin,parameter2.x).SDF(_position);break;
                    case 2: sdfDistance = GBox_Ctor_Extent(origin,parameter2.xyz).SDF(_position);break;
                    case 3: sdfDistance = GPlane_Ctor(parameter2.xyz,origin).SDF(_position);break;
                }
                return UnionSmin(_src,SDFSurface_Ctor(sdfDistance,_Colors[_index]),2,2);
            }
            
            SDFSurface SDF_Combination(float3 position)
            {
                SDFSurface output;
                output.distance = FLT_MAX;
                for(int i = 0 ; i < _SDFCount ; i++)
                    output = SDF_Sample(output,position,i);
                return output;
            }

            #pragma vertex vertSDF2
            #pragma fragment fragSDF2
            #include "Assets/Shaders/Library/Passes/GeometrySDFPass.hlsl"
            struct a2vSDF2
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
            };

            struct v2fSDF2
            {
                float4 positionCS:SV_POSITION;
                float4 positionHCS:TEXCOORD0;
                float3 normalWS: NORMAL;
            };
            v2fSDF2 vertSDF2 (a2vSDF2 v)
            {
                v2fSDF2 o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS = o.positionCS;
                o.normalWS = TransformObjectToWorldDir(v.normalOS);
                return o;
            }

            float4 SDF(float3 viewDirWS)
            {
                SDFHitInfo result;
                GRay viewRay=GRay_Ctor(GetCameraPositionWS(),viewDirWS);
                if(!RaymarchSDF(viewRay,_ProjectionParams.y,_ProjectionParams.z,result))
                    return 0;
                
                viewDirWS=-viewDirWS;
                float3 positionWS=viewRay.GetPoint(result.distance);
                float3 lightPosition =_MainLightPosition.xyz * 100;
                float shadow = RaymarchSDFSoftShadow(positionWS,lightPosition,0.05);
                
                float3 lightDirWS=normalize(lightPosition);
                
                float3 normalWS=RaymarchSDFNormal(positionWS);
                float3 halfDirWS=normalize(lightDirWS+viewDirWS);
                float NDL=saturate(dot(normalWS,lightDirWS));
                float NDH=saturate(dot(normalWS,halfDirWS));
                float3 albedo=result.data.color;
                float3 lightColor=_MainLightColor.rgb;
                float diffuse=saturate(NDL)*.5+.5;
                float3 color=albedo*diffuse*lightColor;

                float specular=pow(NDH,PI);

                return float4(IndirectDiffuse_SH(normalWS) + lerp(color,specular*lightColor*albedo,specular)*shadow,result.distance <= _ProjectionParams.z);
            }
            
            float4 fragSDF2 (v2fSDF2 i) : SV_Target
            {
                float2 positionNDC=TransformHClipToNDC(i.positionHCS);
                float3 viewDirWS = normalize(TransformNDCToFrustumCornersRay(positionNDC));
                
                float4 sdfColor = SDF(viewDirWS);

                viewDirWS = -viewDirWS;
                float3 normalWS = normalize(i.normalWS);
                float3 reflectDir = reflect(viewDirWS,normalWS);
                float3 glassSpecular = saturate(IndirectCubeSpecular(-reflectDir,0));

                float NDV = 1 - saturate(dot(normalWS,viewDirWS));
                NDV = sqr(NDV);
                float3 color = lerp(sdfColor, glassSpecular,NDV);
                float alpha = saturate(NDV + sdfColor.a);
                return float4(color,alpha);
            }
            ENDHLSL
        }
    }
}
