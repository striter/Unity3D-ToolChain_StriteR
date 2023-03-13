
//& https://iquilezles.org/articles/smoothvoronoi/
//& https://iquilezles.org/articles/voronoilines/

float voronoi(float2 _v)
{
    int2 p = floor(_v);
    float2 f = frac(_v);
    float res = 8;
    for(int j=-1;j<=1;j++)
        for(int i=-1;i<=1;i++)
        {
            int2 b = int2(i,j);
            float2 r = b - f + random2(b+p);
            res = min(res, dot(r,r));
        }
    return sqrt(res);
}

float smoothvoronoi(float2 _v)
{
    int2 p = floor(_v);
    float2 f = frac(_v);
    float res = 0;
    for(int j=-1;j<=1;j++)
        for(int i=-1;i<=1;i++)
        {
            int2 b = int2(i,j);
            float2 r = b - f + random2(b+p);
            res += exp2(-32 * dot(r,r));
        }
    return -(1/32.0)*log2(res);
}

float2 voronoi2(float2 _v,float _seed = 0)
{
    float2 n = floor(_v);
    float2 f = frac(_v);

    float2 res = 8.0;
    for(int j=-2;j<=2;j++)
        for(int i=-2;i<=2;i++)
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

    int i,j;
    float2 mg, mr;

    float md = 8.0;
    for( j=-2; j<=2; j++ )
        for( i=-2; i<=2; i++ )
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
    for(j=-2; j<=2; j++ )
        for(i=-2; i<=2; i++ )
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
