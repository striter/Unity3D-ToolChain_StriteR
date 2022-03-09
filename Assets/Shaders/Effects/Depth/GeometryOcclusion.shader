Shader "Game/Unfinished/GeometryOcclusion"
{
    Properties
    {
        _Color("Color Tint",Color)=(1,1,1,1)
        _Strength("Occlusion Strength",Range(0,5))=1
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraNormalTexture);SAMPLER(sampler_CameraNormalTexture);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float,_Strength)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS=o.positionCS;
                return o;
            }

            float SphereOcclusion(float3 _position,float3 _normal,GSphere _sphere)
            {
                float3 distance=_sphere.center-_position;
                float l=length(distance);
                float ndl=dot(_normal,distance/l);
                float h= l/_sphere.radius;
                float h2=h*h;
                float k2=1.0-h2*ndl*ndl;
                float res=max(0,ndl)/h2;
                if(k2>0)
                {
                    #if 0
                        res=ndl*acos(-ndl*sqrt((h2-1.0)/(1.0-ndl*ndl)))-sqrt(k2*(h2-1.0));
                        res = res/h2 + atan(sqrt(k2/(h2-1.0)));
                        res /= PI;
                    #else
                        // res = (ndl*h+1.0)/h2;
                        // res=  0.33*res*res;
                        res = .5*(res+1.-sqrt(1.-1./h2)); 
                    #endif
                }
                return saturate(invlerp(0.1,1,res));
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float2 screenUV=TransformHClipToNDC(i.positionHCS);
                float rawDepth=SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
                float3 normalWS= (SAMPLE_TEXTURE2D(_CameraNormalTexture,sampler_CameraNormalTexture,screenUV).rgb-.5h)*2;
                float3 positionWS=TransformNDCToWorld(screenUV,rawDepth);
                
                float occlusion=0;
                occlusion=SphereOcclusion(TransformWorldToObject(positionWS),TransformWorldToObjectNormal(normalWS),GSphere_Ctor(0,.25f));
                return float4(_Color.rgb,occlusion*_Color.a*_Strength);
            }
            ENDHLSL
        }
    }
}
