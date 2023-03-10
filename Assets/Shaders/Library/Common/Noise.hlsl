half dither01(float2 value)
{
    float4x4 thresholdMatrix =
    {  1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
      13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
       4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
      16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    value=fmod(value,4);
    return thresholdMatrix[value.x][value.y];
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

float quinterp(float _f)
{
    return _f * _f * _f * (_f * (_f * 6 - 15) + 10);
}
float perlin(float2 _v)
{
    float2 pos00 = floor(_v);
    float2 pos10 = pos00 + float2(1.0f, 0.0f);
    float2 pos01 = pos00 + float2(0.0f, 1.0f);
    float2 pos11 = pos00 + float2(1.0f, 1.0f);

    float2 rand00 = randomUnitCircle(pos00);
    float2 rand10 = randomUnitCircle(pos10);
    float2 rand01 = randomUnitCircle(pos01);
    float2 rand11 = randomUnitCircle(pos11);
    
    float dot00 = dot(rand00, pos00 - _v);
    float dot01 = dot(rand01, pos01 - _v);
    float dot10 = dot(rand10, pos10 - _v);
    float dot11 = dot(rand11, pos11 - _v);
    
    float2 d = frac(_v);
    float interpolate = quinterp(d.x);
    float x1 = lerp(dot00, dot10, interpolate);
    float x2 = lerp(dot01, dot11, interpolate);
    return lerp(x1, x2, quinterp(d.y));
}

float perlinUnit(float2 _v)
{
    return perlin(_v) * 2 - 1;
}

//& https://iquilezles.org/articles/voronoilines/
float2 voronoi(float2 _v,float _seed = 0)
{
    float2 n = floor(_v);
    float2 f = frac(_v);

    float2 res = 8.0;
    for(int j=-1;j<=1;j++)
        for(int i=-1;i<=1;i++)
        {
            float2 g = float2(float(i),float(j));
            float2 o = random2( n + g );
            if(_seed!=0)
                o = 0.5 + 0.5*sin( _Time.y + TWO_PI*o );
            float2 r = g + o - f;

            float d = dot(r,r);
            if(d<res.x)
            {
                res.y = res.x;
                res.x = d;
            }
            else
            {
                res.y = d;
            }
        }
    return sqrt(res);
}

float3 voronoiDistances(float2 x,float _seed = 0)
{
    float2 n = floor(x);
    float2 f = frac(x);

    float2 mg, mr;

    float md = 8.0;
    for( int j=-1; j<=1; j++ )
        for( int i=-1; i<=1; i++ )
        {
            float2 g = float2(float(i),float(j));
            float2 o = random2( n + g );
            if(_seed!=0)
                o = 0.5 + 0.5*sin( _Time.y + TWO_PI*o );
            float2 r = g + o - f;
            float d = dot(r,r);

            if( d<md )
            {
                md = d;
                mr = r;
                mg = g;
            }
        }

    md = 8.0;
    for( int j=-2; j<=2; j++ )
        for( int i=-2; i<=2; i++ )
        {
            float2 g = mg + float2(float(i),float(j));
            float2 o = random2( n + g );
            if(_seed!=0)
                o = 0.5 + 0.5*sin( _Time.y + TWO_PI*o );
            
            float2 r = g + o - f;

            if( dot(mr-r,mr-r)>0.00001 )
                md = min( md, dot( 0.5*(mr+r), normalize(r-mr) ) );
        }

    return float3( md, mr );
}


half4 hash4(float2 _p)
{
    return frac(sin( float4(1.0+dot(_p,float2(37.0,17.0)),
                            2.0+dot(_p,float2(11.0,47.0)),
                            3.0+dot(_p,float2(41.0,29.0)),
                            4.0+dot(_p,float2(23.0,31.0)))*130
            ));
}