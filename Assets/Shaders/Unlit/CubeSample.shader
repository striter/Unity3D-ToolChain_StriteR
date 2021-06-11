Shader "Game/Unlit/CubeSample"
{
    Properties
    {
        _CubeMap("Cube Map",CUBE)=""{}
        _Offset("Offset",Range(0,.5))=1
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "../CommonInclude.hlsl"
            #include "../GeometryInclude.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS:TEXCOORD0;
                float3 viewDirOS:TEXCOORD1;
            };

            TEXTURECUBE(_CubeMap);SAMPLER(sampler_CubeMap);
            CBUFFER_START(UnityPerMaterial)
            float _Offset;
            CBUFFER_END
            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionOS=v.positionOS;
                o.viewDirOS=o.positionOS-TransformWorldToObject(GetCameraPositionWS());
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                half3 viewDirOS=normalize(i.viewDirOS);
                float3 offset=float3(0,0,_Offset);
                GBox _box=GetBox(-.5+offset,.5+offset);
                GRay _ray=GetRay(i.positionOS,viewDirOS);
                float2 distances=AABBRayDistance(_box,_ray);
                float3 sdfPosOS=_ray.GetPoint(distances.x+distances.y);
                float3 reflectDir=sdfPosOS-offset;
                return SAMPLE_TEXTURECUBE(_CubeMap,sampler_CubeMap,reflectDir);
            }
            ENDHLSL
        }
    }
}
