import lib-pbr.glsl
import lib-pom.glsl
import lib-utils.glsl
import lib-env.glsl

//: param auto channel_basecolor
uniform SamplerSparse basecolor_tex;
//: param auto channel_glossiness
uniform SamplerSparse glossiness_tex;
//: param auto channel_metallic
uniform SamplerSparse metallic_tex;

//: param auto environment_max_lod
uniform float environment_max_lod;

vec3 lerp(vec3 _a,vec3 _b,float _interpolate)
{
  return _a*(1-_interpolate) + _b*_interpolate;
}

vec3 SHEvalLinearL0L1(vec3 N, vec4 shAr, vec4 shAg, vec4 shAb)
{
    vec4 vA = vec4(N, 1.0);

    vec3 x1;
    // Linear (L1) + constant (L0) polynomial terms
    x1.r = dot(shAr, vA);
    x1.g = dot(shAg, vA);
    x1.b = dot(shAb, vA);

    return x1;
}

vec3 SHEvalLinearL2(vec3 N, vec4 shBr, vec4 shBg, vec4 shBb, vec4 shC)
{
    vec3 x2;
    // 4 of the quadratic (L2) polynomials
    vec4 vB = N.xyzz * N.yzzx;
    x2.r = dot(shBr, vB);
    x2.g = dot(shBg, vB);
    x2.b = dot(shBb, vB);

    // Final (5th) quadratic (L2) polynomial
    float vC = N.x * N.x - N.y * N.y;
    vec3 x3 = shC.rgb * vC;

    return x2 + x3;
}

vec3 SampleSHL2(vec3 _normalWS)
{
    vec4 _SHAr = vec4(0.0109134,0.1585592,0.0129353,0.7254902);
    vec4 _SHAg = vec4(0.0109134,0.1585592,0.0129353,0.7254902);
    vec4 _SHAb = vec4(0.0109134,0.1585592,0.0129353,0.7254902);
    vec4 _SHBr = vec4(0.06141909,0.02248548,0.01431291,0.007843138);
    vec4 _SHBg = vec4(0.06141909,0.02248548,0.01431291,0.007843138);
    vec4 _SHBb = vec4(0.06141909,0.02248548,0.01431291,0.007843138);
    vec4 _SHC = vec4(0.06627715,0.06627715,0.06627715,0);
    vec3 res = SHEvalLinearL0L1(_normalWS, _SHAr, _SHAg, _SHAb);
    res += SHEvalLinearL2(_normalWS, _SHBr, _SHBg, _SHBb, _SHC);
    return res;
}

#define UNITY_SPECCUBE_LOD_STEPS_CUSTOM 6
vec3 IndirectSpecular(vec3 normal,vec3 viewDir, float perceptualRoughness)
{
    float mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness);    
	const float mipmap_start = 0;
	const float mipmap_end = environment_max_lod - 1.5;
	mip = mipmap_start +  ( mipmap_end - mipmap_start ) * mip;
    vec3 reflUVW = normalize(reflect(-viewDir, normal));
    return envSampleLOD(reflUVW,mip);
}

float F_Schlick(float NDV)
{ 
    float x = clamp(1 - NDV,0,1);
    return x*x*x*x;//pow5(x);
}

struct UnityLight
{
    vec3 direction;
    vec3 color;
    float distanceAttenuation;
    float shadowAttenuation;
};

UnityLight lightArray[1];
void PropagateLights( vec3 position )
{
    lightArray[0].direction = normalize(vec3(-1,1,0));
    lightArray[0].color = vec3(.5,.5,.5);
    lightArray[0].shadowAttenuation = 1;
    lightArray[0].distanceAttenuation = 1;
}

void shade(V2F inputs)
{
    PropagateLights(inputs.position.xyz);
	inputs.normal = normalize(inputs.normal);
	LocalVectors vectors = computeLocalFrame(inputs);

    float glossiness = getGlossiness(glossiness_tex, inputs.sparse_coord);
    vec3 albedo = getBaseColor(basecolor_tex, inputs.sparse_coord);
    float metallic = getMetallic(metallic_tex, inputs.sparse_coord);
    float ao = getAO(inputs.sparse_coord);
    
    float oneMinusReflectivity = 0.96 - metallic * 0.96;
    float reflectivity = 1.0 - oneMinusReflectivity;

    vec3 diffuse = albedo * oneMinusReflectivity;
    vec3 specular = lerp(vec3(0.04,0.04,0.04), albedo, metallic);
    float grazingTerm = clamp(glossiness + reflectivity,0,1);
    float perceptualRoughness = 1.0 - glossiness;
    float roughness = max(0.0078125, perceptualRoughness * perceptualRoughness);
    float roughness2 = max(6.103515625e-5, roughness * roughness);

    vec3 normal = vectors.normal;
    vec3 viewDir = normalize(camera_pos - inputs.position);
    vec3 reflectDir = normalize(reflect(-viewDir, normal));

    float NDV = clamp(dot(normal,viewDir),0,1);

    vec3 finalCol = vec3(0,0,0);
    //GI Specular
    vec3 indirectDiffuse = SampleSHL2(normal);
    vec3 indirectSpecular = IndirectSpecular(normal,viewDir,roughness);
    indirectDiffuse *= ao;
    indirectSpecular *= ao;
  
    vec3 giDiffuse = indirectDiffuse * diffuse;
  
    float fresnelTerm = F_Schlick(max(0,NDV));
    vec3 surfaceReduction = 1.0 / (roughness2 + 1.0) * lerp(specular, vec3(grazingTerm,grazingTerm,grazingTerm), fresnelTerm);
    vec3 giSpecular = indirectSpecular * surfaceReduction;

    finalCol += (giDiffuse+giSpecular);
  
  
    //PBR Lighting
    UnityLight mainLight = lightArray[0];
    vec3 halfDir = normalize(viewDir + mainLight.direction);
    float NDL = clamp(dot(normal,mainLight.direction),0,1);
    float NDH = clamp(dot(normal,halfDir),0,1);
    float LDH = clamp(dot(mainLight.direction,halfDir),0,1);
    vec3 radiance = mainLight.color * (mainLight.distanceAttenuation*mainLight.shadowAttenuation * NDL);
  
    // Normal distribution 
    float d = NDH * NDH * (roughness2-1.f) +1.00001f;
    float D=  clamp(roughness2 / (d * d),0,100);
    
    //Normalization term
    float VF = max(0.1, LDH*LDH) * (roughness*4 + 2);
    
    vec3 brdf = diffuse + specular * D / VF;
    finalCol += brdf*radiance;
    diffuseShadingOutput(finalCol);
}
