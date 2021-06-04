Shader "Unlit/GlobalTextureVisualize"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../CommonInclude.hlsl"

            struct appdata
            {
                float3 vertex : POSITION;
                float3 normal:NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal:NORMAL;
                float4 screenPos:TEXCOORD0;
                float3 viewDir:TEXCOORD1;
            };

            uint _CameraReflectionTextureIndex;
            sampler2D _CameraReflectionTexture0;
            sampler2D _CameraReflectionTexture1;
            sampler2D _CameraReflectionTexture2;
            sampler2D _CameraReflectionTexture3;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.screenPos=ComputeScreenPos(o.vertex);
                o.normal=TransformObjectToWorldNormal(v.normal);
                o.viewDir=TransformObjectToWorldNormal(v.vertex)-GetCameraPositionWS().xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 viewDir=-normalize(i.viewDir);
                float3 normal=normalize( i.normal);
                float3 reflectDir=reflect(-viewDir,normal);
                float4 col=0;
                float2 screenUV=i.screenPos.xy/i.screenPos.w;
                [branch]switch(_CameraReflectionTextureIndex)
                {
                    case 0u:col=tex2D(_CameraReflectionTexture0,screenUV);break;
                    case 1u:col=tex2D(_CameraReflectionTexture1,screenUV);break;
                    case 2u:col=tex2D(_CameraReflectionTexture2,screenUV);break;
                    case 3u:col=tex2D(_CameraReflectionTexture3,screenUV);break;
                }
                return float4(col.rgb,1);
            }
            ENDHLSL
        }
    }
}
