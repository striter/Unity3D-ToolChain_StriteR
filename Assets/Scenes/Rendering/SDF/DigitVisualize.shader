Shader "Hidden/DigitVisualize"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Blend Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2f
            {
                float3 positionOS : POSITION;
                float2 uv:TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD0;
            };

            float SampleDigit(int fDigit,float2 uv)
            {
                if(uv.x<0||uv.x>1||uv.y<0||uv.y>1)
                    return 0;
                
                int fDigitBinary=0;
                if(fDigit==0)
                    fDigitBinary = 7 + 5*16 + 5*256 + 5*4096 + 7*65536;
                else if(fDigit==1)
                    fDigitBinary = 2 + 2*16 + 2*256 + 2*4096 + 2*65536;
                else if(fDigit==2)
                    fDigitBinary = 7 + 1*16 + 7*256 + 4*4096 + 7*65536;
                else if(fDigit==3)
                    fDigitBinary = 7 + 4*16 + 7*256 + 4*4096 + 7*65536;
                else if(fDigit==4)
                    fDigitBinary = 4 + 4*16 + 7*256 + 5*4096 + 5*65536;
                else if(fDigit==5)
                    fDigitBinary = 7 + 4*16 + 7*256 + 1*4096 + 7*65536;
                else if(fDigit==6)
                    fDigitBinary = 7 + 5*16 + 7*256 + 1*4096 + 1*65536;
                else if(fDigit==7)
                    fDigitBinary = 4 + 4*16 + 4*256 + 4*4096 + 7*65536;
                else if(fDigit==8)
                    fDigitBinary = 7 + 5*16 + 7*256 + 5*4096 + 7*65536;
                else if(fDigit==9)
                    fDigitBinary = 4 + 4*16 + 7*256 + 5*4096 + 7*65536;
                else if(fDigit==10)     //.
                    fDigitBinary = 1;

                int2 pixel=floor(uv*int2(4,5));
                int fIndex = pixel.x + (pixel.y*4);
                return fmod(floor(fDigitBinary/pow(2,fIndex)),2);
            }

            float2 ToDigitUV(float2 _uv,int2 _digitBegin,int2 _digitSize)
            {
                const int2 totalSize=int2(100,100);
                return invlerp(_digitBegin,_digitBegin+_digitSize,_uv*totalSize);
            }
            
            v2f vert (a2f v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv=v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                int timeInt;
                float timeFloat=modf(_Time.y,timeInt);
                int intX=fmod(timeInt,1000)/100;
                int intY=fmod(timeInt,100)/10;
                int intZ=fmod(timeInt,10);

                float digit=SampleDigit(intX,ToDigitUV(i.uv,int2(5,35),int2(20,20)));
                digit+=SampleDigit(intY,ToDigitUV(i.uv,int2(25,35),int2(20,20)));
                digit+=SampleDigit(intZ,ToDigitUV(i.uv,int2(45,35),int2(20,20)));
                digit+=SampleDigit(10,ToDigitUV(i.uv,int2(65,35),int2(20,20)));
                
                timeFloat*=1000;
                int floatZ=fmod(timeFloat,10);
                int floatY=fmod(timeFloat,100)/10;
                int floatX=fmod(timeFloat,1000)/100;
                digit+=SampleDigit(floatX,ToDigitUV(i.uv,int2(5,10),int2(20,20)));
                digit+=SampleDigit(floatY,ToDigitUV(i.uv,int2(25,10),int2(20,20)));
                // digit+=SampleDigit(floatZ,ToDigitUV(i.uv,int2(45,10),int2(20,20)));
                
                clip(digit-.5);
                return digit;
            }
            ENDHLSL
        }
    }
}
