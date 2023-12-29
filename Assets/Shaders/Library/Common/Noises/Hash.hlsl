int   seed = 1;
void  srand(int s )
{
    seed = s;
}
int rand()
{
    seed = seed*0x343fd+0x269ec3; return (seed>>16)&32767;
}
float frand()
{
    return float(rand())/32767.0;
}
int hash( int n )
{
    n=(n<<13)^n; return n*(n*n*15731+789221)+1376312589;
}


float random(float value,float seed = 0.546)
{
    return frac(sin(value * 12.9898 + seed) * 43758.5453);
}

float random(float2 value,float seed = 0.546)
{
    return frac(sin(dot(value, float2(12.9898, 78.233) + seed)) * 43758.543123);
}

float random(float3 value,float seed = 0.546)
{
    return frac(sin(dot(value, float3(12.9898, 78.233, 53.539) + seed)) * 43758.543123);
}

float2 random2(float2 value)
{
    return float2(random(value,3.9812),
                  random(value,7.1536));
}

float3 random3(float value)
{
    return float3(random(value,3.9812),
                  random(value,7.1536),
                  random(value,5.7241));
}

float3 randomVector(float value)
{
    return normalize(random3(value) - .5);
}

float randomUnit(float value)
{
    return random(value) * 2 - 1;
}
float randomUnit(float2 value)
{
    return random(value) * 2 - 1;
}
float randomUnit(float3 value)
{
    return random(value) * 2 - 1;
}

float2 randomUnitQuad(float2 value)
{
    return float2(randomUnit(value.xy), randomUnit(value.yx));
}

float2 randomUnitCircle(float2 value)
{
    float theta = 2 * PI * random(value);
    return float2(cos(theta), sin(theta));
}

half4 hash4(float2 _p)
{
    return frac(sin( float4(1.0+dot(_p,float2(37.0,17.0)),
                            2.0+dot(_p,float2(11.0,47.0)),
                            3.0+dot(_p,float2(41.0,29.0)),
                            4.0+dot(_p,float2(23.0,31.0)))*130
            ));
}