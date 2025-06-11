using Unity.Mathematics;
using static Unity.Mathematics.math;

public static partial class UColor
{
    private static readonly float3 kLuminanceMultiplier = new float3(0.2126729f, 0.7151522f, 0.0721750f);
    public static float RGBtoLuminance(float3 color) => math.dot(color,kLuminanceMultiplier);
    public static float RoughnessToPerceptualSmoothness(float roughness) => 1.0f - sqrt(roughness);
    public static float PerceptualSmoothnessToRoughness(float perceptualSmoothness) => (1.0f - perceptualSmoothness) * (1.0f - perceptualSmoothness);
    public static float PerceptualSmoothnessToPerceptualRoughness(float perceptualSmoothness) => 1.0f - perceptualSmoothness;
    public static float GammaToLinear_Accurate(float value)
    {
        if (value <= 0.04045F)
            return value / 12.92F;
        if (value < 1.0F)
            return pow((value + 0.055F) / 1.055F, 2.4F);
        if (value == 1.0F)
            return 1.0f;
        return pow(value, 2.2f);
    }

    public static float GammaToLinear(float sRGB) => sRGB * (sRGB * (sRGB * 0.305306011f + 0.682171111f) + 0.012522878f);
    public static float3 GammaToLinear(float3 sRGB) => sRGB * (sRGB * (sRGB * 0.305306011f + 0.682171111f) + 0.012522878f);
    
    public static float LinearToGamma_Accurate(float col)
    {
        if (col <= 0.0031308f)
            col *= 12.92f;
        else
            col = 1.055f * pow(col, 0.4166667f) - 0.055f;
        return col;
    }

    public static float3 LinearToGamma_Accurate(float3 _col)
    {
        return new float3(LinearToGamma_Accurate(_col.x), 
            LinearToGamma_Accurate(_col.y),
            LinearToGamma_Accurate(_col.z));
    }

    public static float3 LinearToSRGB(float3 c)
    {
        return max(1.055f * pow(c, 0.416666667f) - 0.055f, 0.0f);	
    }
    
    private const float LIGHTMAP_HDR_MULTIPLIER = 34.493242f;
    private const float LIGHTMAP_HDR_EXPONENT = 2.2f;
    public static float3 DecodeFromLightmapRGBM(float4 _rgbm)
    {
        var multiplier = pow(_rgbm.w, LIGHTMAP_HDR_EXPONENT) * LIGHTMAP_HDR_MULTIPLIER;
        return _rgbm.xyz * multiplier;
    }

    private const float EMISSIVE_RGBM_SCALE = 97.0f;
    public static float4 PackEmissiveRGBM(float3 rgb)
    {
        float kOneOverRGBMMaxRange = 1.0f / EMISSIVE_RGBM_SCALE;
        const float kMinMultiplier = 2.0f * 1e-2f;

        var rgbm = new float4(rgb * kOneOverRGBMMaxRange, 1.0f);
        rgbm.w = max(max(rgbm.x, rgbm.y), max(rgbm.y, kMinMultiplier));
        rgbm.w = ceil(rgbm.w * 255.0f) / 255.0f;

        // Division-by-zero warning from d3d9, so make compiler happy.
        rgbm.w = max(rgbm.w, kMinMultiplier);

        rgbm.xyz /= rgbm.w;
        return rgbm;
    }
    
}
