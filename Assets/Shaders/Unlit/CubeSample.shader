Shader "Game/Unlit/CubeSample"
{
    Properties
    {
        _CubeMap("Cube Map",CUBE)=""{}
        _Offset("Offset",Range(0,.5))=1
        [Toggle(_SPHERICAL)]_Spherical("Spherical",int)=0
        
		[Header(Render Options)]
        [HideInInspector]_ZWrite("Z Write",int)=1
        [HideInInspector]_ZTest("Z Test",int)=2
        [HideInInspector]_Cull("Cull",int)=2
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" }
            ZTest Always
            Cull Off
        HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            #pragma shader_feature_local _SPHERICAL
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
                GRay viewRay=GRay_Ctor(i.positionOS,viewDirOS);

                #if _SPHERICAL
                    GSphere sphere = GSphere_Ctor(float3(0,0,.5),1);
                    float distance = sum(Distance(sphere,viewRay));
                #else
                    GBox box=GBox_Ctor(float3(0,0,.5),1);
                    float distance=sum(Distance(box,viewRay));
                #endif
                
                float3 sdfPosOS=viewRay.GetPoint(distance);

                float4 positionVS = TransformObjectToView(sdfPosOS);
                positionVS.xyz/=positionVS.w;
                _depth=EyeToRawDepth(-positionVS.z);
                return sdfPosOS-offset;
            }
        ENDHLSL
        
        Pass
        {       
             ZTEST LEqual
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
			ZWrite On
			Blend Off
			ZTest [_ZTest]
			Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float4 frag (v2f i,out float depth:SV_DEPTH) : SV_Target
            {
                SDFFragment(i,depth);
                return 0;
            }
            ENDHLSL
        }
    }
}
