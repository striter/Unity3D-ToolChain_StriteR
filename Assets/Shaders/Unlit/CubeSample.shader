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
        HLSLINCLUDE
            #include "../CommonInclude.hlsl"
            #include "../GeometryInclude.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Offset;
            CBUFFER_END
        
            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS:TEXCOORD0;
                float3 viewDirOS:TEXCOORD1;
            };


            v2f vert (float3 positionOS:POSITION)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(positionOS);
                o.positionOS=positionOS;
                o.viewDirOS=o.positionOS-TransformWorldToObject(GetCameraPositionWS());
                return o;
            }

            float3 SDFFragment(v2f i, out float _depth)
            {
                half3 viewDirOS=normalize(i.viewDirOS);
                float3 offset=float3(0,0,_Offset);
                GBox _box=GetBox(-.5+offset,.5+offset);
                GRay _ray=GetRay(i.positionOS,viewDirOS);
                
                float2 distances=AABBRayDistance(_box,_ray);
                float3 sdfPosOS=_ray.GetPoint(distances.x+distances.y);

                float4 positionVS = TransformObjectToView(sdfPosOS);
                positionVS.xyz/=positionVS.w;
                _depth=LinearEyeDepthToOutDepth(-positionVS.z);
                return sdfPosOS-offset;
            }
        ENDHLSL
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURECUBE(_CubeMap);SAMPLER(sampler_CubeMap);
            float4 frag (v2f i,out float depth:SV_DEPTH) : SV_Target
            {
                float4 color= SAMPLE_TEXTURECUBE(_CubeMap,sampler_CubeMap,SDFFragment(i,depth));
                
                return color;
            }
            ENDHLSL
        }
        Pass
        {
			Tags{"LightMode" = "DepthOnly"}    
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 frag(v2f i,out float depth:SV_DEPTH):SV_TARGET
            {
                SDFFragment(i,depth);
                return 0;
            }
            ENDHLSL
        }
    }
}
