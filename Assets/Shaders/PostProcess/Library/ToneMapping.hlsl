#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ACES.hlsl"

float3 AcesTonemap(float3 aces)
{
#if TONEMAPPING_USE_FULL_ACES

    float3 oces = RRT(aces);
    float3 odt = ODT_RGBmonitor_100nits_dim(oces);
    return odt;

#else

    // --- Glow module --- //
    float saturation = rgb_2_saturation(aces);
    float ycIn = rgb_2_yc(aces);
    float s = sigmoid_shaper((saturation - 0.4) / 0.2);
    float addedGlow = 1.0 + glow_fwd(ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
    aces *= addedGlow;

    // --- Red modifier --- //
    float hue = rgb_2_hue(aces);
    float centeredHue = center_hue(hue, RRT_RED_HUE);
    float hueWeight;
    {
        //hueWeight = cubic_basis_shaper(centeredHue, RRT_RED_WIDTH);
        hueWeight = smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH));
        hueWeight *= hueWeight;
    }

    aces.r += hueWeight * saturation * (RRT_RED_PIVOT - aces.r) * (1.0 - RRT_RED_SCALE);

    // --- ACES to RGB rendering space --- //
    float3 acescg = max(0.0, ACES_to_ACEScg(aces));

    // --- Global desaturation --- //
    //acescg = mul(RRT_SAT_MAT, acescg);
    acescg = lerp(dot(acescg, AP1_RGB2Y).xxx, acescg, RRT_SAT_FACTOR.xxx);

    // Apply RRT and ODT
    // https://github.com/TheRealMJP/BakingLab/blob/master/BakingLab/ACES.hlsl
    const float a = 0.0245786f;
    const float b = 0.000090537f;
    const float c = 0.983729f;
    const float d = 0.4329510f;
    const float e = 0.238081f;

#if defined(SHADER_API_SWITCH)
    // To reduce the likelyhood of extremely large values, we avoid using the x^2 term and therefore
    // divide numerator and denominator by it. This will lead to the constant factors of the
    // quadratic in the numerator and denominator to be divided by x; we add a tiny epsilon to avoid divide by 0.
    float3 rcpAcesCG = rcp(acescg + FLT_MIN);
    float3 rgbPost = (acescg + a - b * rcpAcesCG) /
        (acescg * c + d + e * rcpAcesCG);
#else
    float3 rgbPost = (acescg * (acescg + a) - b) /
        (acescg * (c * acescg + d) + e);
#endif

    // Scale luminance to linear code value
    // float3 linearCV = Y_2_linCV(rgbPost, CINEMA_WHITE, CINEMA_BLACK);

    // Apply gamma adjustment to compensate for dim surround
    float3 linearCV = darkSurround_to_dimSurround(rgbPost);

    // Apply desaturation to compensate for luminance difference
    //linearCV = mul(ODT_SAT_MAT, color);
    linearCV = lerp(dot(linearCV, AP1_RGB2Y).xxx, linearCV, ODT_SAT_FACTOR.xxx);

    // Convert to display primary encoding
    // Rendering space RGB to XYZ
    float3 XYZ = mul(AP1_2_XYZ_MAT, linearCV);

    // Apply CAT from ACES white point to assumed observer adapted white point
    XYZ = mul(D60_2_D65_CAT, XYZ);

    // CIE XYZ to display primaries
    linearCV = mul(XYZ_2_REC709_MAT, XYZ);

    return linearCV;

#endif
}