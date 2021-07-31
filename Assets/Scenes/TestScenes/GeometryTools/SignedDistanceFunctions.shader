Shader "Hidden/SignedDistanceFunctions"
{
    Properties
    {
        [Header(Sphere)]
        _SphereColor("Color",Color)=(1,1,1,1)
        [Vector3]_SpherePosition("Position",Vector)=(0,0,0,0)
        _SphereRadius("Raidus",float)=3
        [Header(Round Box)]
        _BoxColor("Color",Color)=(1,1,1,1)
        [Vector3]_BoxPosition("Position",Vector)=(0,0,0,0)
        [Vector3]_BoxSize("Size",Vector)=(1,1,1,0)
        _BoxRoundness("Roundness",Range(0,.5))=.1
        [Header(Frame Box)]
        [HDR]_FrameBoxColor("Color",Color)=(1,1,1,1)
        [Vector3]_FrameBoxPosition("Position",Vector)=(0,0,0,0)
        [Vector3]_FrameBoxSize("Size",Vector)=(1,1,1,0)
        _FrameBoxExtend("Extend",Range(0,.5))=.1
        [Header(Torus)]
        [HDR]_TorusColor("Color",Color)=(1,1,1,1)
        [Vector3]_TorusPosition("Position",Vector)=(0,0,0,0)
        _TorusMajorRadius("Major Radius",Range(0,1))=0.5
        _TorusMinorRadius("Minor Radius",Range(0,0.2))=0.1
    }
    SubShader
    {
        Tags { "RenderType"="UniversalForward" "RenderQueue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Shaders/CommonInclude.hlsl"
            #include "Assets/Shaders/GeometryInclude.hlsl"
            #define MAX_MARCH_STEPS 255
            #define FLOAT_EPSILON 0.00001      
            struct a2v
            {
                float3 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS:SV_POSITION;
                float4 positionHCS:TEXCOORD0;
            };
            float3 _SphereColor;
            float3 _SpherePosition;
            float _SphereRadius;
            float3 _BoxColor;
            float3 _BoxPosition;
            float3 _BoxSize;
            float _BoxRoundness;
            float3 _FrameBoxColor;
            float3 _FrameBoxPosition;
            float3 _FrameBoxSize;
            float _FrameBoxExtend;
            float3 _TorusColor;
            float3 _TorusPosition;
            float _TorusMajorRadius;
            float _TorusMinorRadius;
            SDFOutput SceneSDF(float3 positionWS)
            {
                GSphere sphere=GSphere_Ctor(_SpherePosition,_SphereRadius);
                GRoundBox roundBox=GRoundBox_Ctor(_BoxPosition,_BoxSize,_BoxRoundness);
                GFrameBox frameBox=GFrameBox_Ctor(_FrameBoxPosition,_FrameBoxSize,_FrameBoxExtend);
                GTorus torus=GTorus_Ctor(_TorusPosition,_TorusMajorRadius,_TorusMinorRadius);
                SDFOutput distA= SDSphere(sphere,SDFInput_Ctor(positionWS,_SphereColor));
                SDFOutput distB=SDRoundBox(roundBox,SDFInput_Ctor(mul(Rotate3x3(_Time.y,float3(1,0,0)),positionWS),_BoxColor));
                SDFOutput distC=SDFrameBox(frameBox,SDFInput_Ctor(mul(Rotate3x3(_Time.y,float3(0,1,0)), positionWS),_FrameBoxColor));
                SDFOutput distD=SDTorus(torus,SDFInput_Ctor(mul(Rotate3x3(_Time.y,float3(0,0,1)), positionWS),_TorusColor));
                return SDFUnion( SDFDifference(distB,distA),distC,distD);
            }
            
            bool RaymarchSDF(GRay ray,float start,float end,out float distance,out SDFOutput _output)
            {
                distance=start;
                for(int i=0;i<MAX_MARCH_STEPS;i++)
                {
                    _output=SceneSDF(ray.GetPoint(distance));
                    float sdfDistance=_output.distance;
                    if(sdfDistance < FLOAT_EPSILON)
                        return true;
                    distance+=sdfDistance;
                    if(distance>=end)
                        break;
                }
                return false;
            }
            
            float3 RaymarchSDFNormal(float3 marchPos)
            {
                return normalize(float3(
                    SceneSDF(float3(marchPos.x+FLOAT_EPSILON,marchPos.y,marchPos.z)).distance- SceneSDF(float3(marchPos.x-FLOAT_EPSILON,marchPos.y,marchPos.z)).distance,
                    SceneSDF(float3(marchPos.x,marchPos.y+FLOAT_EPSILON,marchPos.z)).distance- SceneSDF(float3(marchPos.x,marchPos.y-FLOAT_EPSILON,marchPos.z)).distance,
                    SceneSDF(float3(marchPos.x,marchPos.y,marchPos.z+FLOAT_EPSILON)).distance- SceneSDF(float3(marchPos.x,marchPos.y,marchPos.z-FLOAT_EPSILON)).distance
                ));
            }
            
            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS=o.positionCS;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float2 positionNDC=TransformHClipToNDC(i.positionHCS);

                float3 viewDirWS=normalize(TransformNDCToViewDirWS(positionNDC));
                GRay viewRay=GRay_Ctor(GetCameraPositionWS(),viewDirWS);
                
                SDFOutput  output;
                float distance;
                if(!RaymarchSDF(viewRay,_ProjectionParams.y,_ProjectionParams.z,distance,output))
                    return 0;
                viewDirWS=-viewDirWS;
                float3 positionWS=viewRay.GetPoint(distance);
                float3 normalWS=RaymarchSDFNormal(positionWS);
                float3 lightDirWS=normalize(_MainLightPosition.xyz);
                float3 halfDirWS=normalize(lightDirWS+viewDirWS);
                float NDL=saturate(dot(normalWS,lightDirWS));
                float NDV=saturate(dot(normalWS,viewDirWS));
                float NDH=saturate(dot(normalWS,halfDirWS));
                float3 albedo=output.color;
                float3 lightColor=_MainLightColor.rgb;
                float diffuse=saturate(NDL)*.5+.5;
                float3 color=albedo*diffuse*lightColor;

                float specular=pow(NDH,PI);
                color=lerp(color,specular*lightColor*albedo,specular);

                return float4(color,1);
            }
            ENDHLSL
        }
    }
}
