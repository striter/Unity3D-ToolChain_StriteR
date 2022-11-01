Shader "PCG/Grid"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        [HDR]_Color("Color Tint",Color)=(1,1,1,1)
        [HDR]_GridColor("Color Tint",Color)=(1,1,1,1)
        
        _Center("Center",Vector)=(0,0,0,0)
        _StartTime("Start Time",float) = 65504
        _Forward("Forward",int) = 1
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Pass
        {
			name "MAIN"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color:COLOR;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_GridColor)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float,_StartTime)
                INSTANCING_PROP(float3,_Center)
                INSTANCING_PROP(int,_Forward)
            INSTANCING_BUFFER_END

            void CalculateSpread(float timeElapsed,float offsetLength,out float spreadParam,out float alpha,float spreadSpeed = 50,float spreadWidth = 5)
            {
                float progress = saturate(timeElapsed / .5f);
                spreadParam= saturate(invlerp(progress * timeElapsed * spreadSpeed,progress*timeElapsed*spreadSpeed+spreadWidth ,offsetLength));
                alpha =  progress * step(spreadParam,0.99) ;
            }
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 positionWS = TransformObjectToWorld(v.positionOS);
                float timeElapsed = _Time.y-_StartTime;
                [branch]
                if (_Forward != 0)
                {
                    float offsetLength =length(positionWS.xz-_Center.xz);
                    float edgeSpread,edgeAlpha;
                    CalculateSpread(timeElapsed,offsetLength,edgeSpread,edgeAlpha);
                    
                    float gridTimeElapsed = saturate(timeElapsed - .3);
                    float gridSpread,gridAlpha;
                    CalculateSpread(gridTimeElapsed,offsetLength,gridSpread,gridAlpha,65,5);
                    
                    gridAlpha *= saturate(lerp(1,0,saturate(timeElapsed - 1)*3));
                    
                    o.color = lerp(INSTANCE(_Color),INSTANCE(_GridColor),v.color.r);
                    o.color.a *= lerp(edgeAlpha,gridAlpha ,v.color.r) + (1-step(_StartTime,_Time.y));
                    
                    positionWS.y += lerp(edgeSpread*2,gridSpread*2,v.color.r);
                }
                else
                {
                    float param = timeElapsed*1.5;
                    positionWS.y += param*.5;
                    o.color = float4(1,1,1,saturate(1-param)*(1-v.color.r)*_Color.a);
                }
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.positionCS = TransformWorldToHClip(positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * i.color;
            }
            ENDHLSL
        }
    }
}
