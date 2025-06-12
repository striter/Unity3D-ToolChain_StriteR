#define HSV_CELL 6u
#define INV_HSV_CELL 0.166666h

half3 HSVtoRGB(half3 hsv)
{
    half h = hsv.x;
    half s = hsv.y;
    half v = hsv.z;
    h  *= HSV_CELL;
    half C=s*v;
    half X=C*(1.h-abs(fmod(h,2)-1.h));
    half r=0,g=0,b=0;
    [branch]
    switch (h%6u)
    {
        case 0u:  { r=C; g=X;  } break;
        case 1u:  { r=X; g=C;  } break;
        case 2u:  { g=C; b=X;  } break;
        case 3u:  { g=X; b=C;  } break;
        case 4u:  { b=C; r=X;  } break;
        case 5u:  { b=X; r=C;  } break;
    }
    half m=v-C;
    return half3(r+m,g+m,b+m);
    // half hIndex = floor(h);
    // half offset =  h - hIndex;
    // half p = v * (1.h - s);
    // half q = v * (1.h - offset * s);
    // half t = v * (1.h - (1.h - offset) * s);
    // [branch]
    // switch (hIndex%6u)
    // {
    //     default:return 0.h;
    //     case 0u: return half3(v, t, p);
    //     case 1u: return half3(q, v, p);
    //     case 2u: return half3(p, v, t);
    //     case 3u: return half3(p, q, v);
    //     case 4u: return half3(t, p, v);
    //     case 5u: return half3(v, p, q);
    // }
}

float LinearToGamma_Accurate(float col)
{
    if (col <= 0.0031308)
        col *= 12.92;
    else
        col = 1.055 * pow(abs(col), 0.4166667) - 0.055;
    return col;
}

float2 LinearToGamma_Accurate(float2 _col)
{
    return float2(LinearToGamma_Accurate(_col.r),
        LinearToGamma_Accurate(_col.g));
}
float3 LinearToGamma_Accurate(float3 _col)
{
    return float3(LinearToGamma_Accurate(_col.r),
        LinearToGamma_Accurate(_col.g),
        LinearToGamma_Accurate(_col.b)
    );
}
float4 LinearToGamma_Accurate(float4 _col)
{
    return float4(LinearToGamma_Accurate(_col.r),
        LinearToGamma_Accurate(_col.g),
        LinearToGamma_Accurate(_col.b),
        LinearToGamma_Accurate(_col.a)
    );
}

float3 GammaToLinear(float3 _col)
{
    return pow(abs(_col),2.2);
}

half3 RGBtoHSV(half3 rgb)
{
    half cmax = max(rgb);
    half cmin = min(rgb);
    half offset = cmax - cmin;
    half h = 1.0h;
    if (offset == 0)
    {
        h = 0.0h;
    }
    else
    {
        if (abs(cmax - rgb.r)<HALF_EPS)
            h = INV_HSV_CELL * ((rgb.g>=rgb.b?0.h:6.h) + (rgb.g - rgb.b) / offset);
        else if (abs(cmax - rgb.g)<HALF_EPS)
            h = INV_HSV_CELL * (2.0h + (rgb.b - rgb.r) / offset);
        else
            h = INV_HSV_CELL * (4.0h + (rgb.r - rgb.g) / offset);
    }
    half s = cmax == 0 ? 0.0h : offset / cmax;
    half v = cmax;
    return half3(h, s, v);
}

half RGBtoLuminance(half3 color)
{
    return dot(color,half3(0.2126729h,  0.7151522h, 0.0721750h));
}

half3 Saturation(half3 c,half _saturation)
{
    half lum = RGBtoLuminance(c);
    return lum.xxx + _saturation.xxx * (c - lum.xxx);
}

static const float kRGBMEncodeRange=8.0;
static const float kInvRGBMRange=.125;
static const float k8Byte=255.;
static const float kInv8Byte=0.003921568627451;
half4 EncodeToRGBM(float3 color)
{
    color*=kInvRGBMRange;
    half m=max(color);
    m=ceil(m*k8Byte)*kInv8Byte;
    return saturate(half4(color*rcp(m),m));
}
half3 DecodeFromRGBM(half4 rgbm)
{
    return rgbm.rgb*rgbm.w*kRGBMEncodeRange;
}