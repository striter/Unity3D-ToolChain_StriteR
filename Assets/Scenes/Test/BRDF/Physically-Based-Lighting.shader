Shader "Physically-Based-Lighting" {
    Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecularColor ("Specular Color", Color) = (1,1,1,1)
	_SpecularPower("Specular Power", Range(0,1)) = 1
	_SpecularRange("Specular Gloss",  Range(1,40)) = 0
	_Glossiness("Smoothness",Range(0,1)) = 1
	_Metallic("Metallicness",Range(0,1)) = 0
	_Anisotropic("Anisotropic",  Range(-20,1)) = 0
	_Ior("Ior",  Range(1,4)) = 1.5
	_UnityLightingContribution("Unity Reflection Contribution", Range(0,1)) = 1
	[KeywordEnum(BlinnPhong,Phong,Beckmann,Gaussian,GGX,TrowbridgeReitz,TrowbridgeReitzAnisotropic, Ward)] _NormalDistModel("Normal Distribution Model;", Float) = 0
	[KeywordEnum(AshikhminShirley,AshikhminPremoze,Duer,Neumann,Kelemen,ModifiedKelemen,Cook,Ward,Kurt)]_GeoShadowModel("Geometric Shadow Model;", Float) = 0
	[KeywordEnum(None,Walter,Beckman,GGX,Schlick,SchlickBeckman,SchlickGGX, Implicit)]_SmithGeoShadowModel("Smith Geometric Shadow Model; None if above is Used;", Float) = 0
	[KeywordEnum(Schlick,SchlickIOR, SphericalGaussian)]_FresnelModel("Normal Distribution Model;", Float) = 0
	[Toggle] _ENABLE_NDF ("Normal Distribution Enabled?", Float) = 0
	[Toggle] _ENABLE_G ("Geometric Shadow Enabled?", Float) = 0
	[Toggle] _ENABLE_F ("Fresnel Enabled?", Float) = 0
	[Toggle] _ENABLE_D ("Diffuse Enabled?", Float) = 0
    }
    SubShader {
	Tags {
            "RenderType"="Opaque"  "Queue"="Geometry"
        } 
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile _NORMALDISTMODEL_BLINNPHONG _NORMALDISTMODEL_PHONG _NORMALDISTMODEL_BECKMANN _NORMALDISTMODEL_GAUSSIAN _NORMALDISTMODEL_GGX _NORMALDISTMODEL_TROWBRIDGEREITZ _NORMALDISTMODEL_TROWBRIDGEREITZANISOTROPIC _NORMALDISTMODEL_WARD
            #pragma multi_compile _GEOSHADOWMODEL_ASHIKHMINSHIRLEY _GEOSHADOWMODEL_ASHIKHMINPREMOZE _GEOSHADOWMODEL_DUER_GEOSHADOWMODEL_NEUMANN _GEOSHADOWMODEL_KELEMAN _GEOSHADOWMODEL_MODIFIEDKELEMEN _GEOSHADOWMODEL_COOK _GEOSHADOWMODEL_WARD _GEOSHADOWMODEL_KURT 
            #pragma multi_compile _SMITHGEOSHADOWMODEL_NONE _SMITHGEOSHADOWMODEL_WALTER _SMITHGEOSHADOWMODEL_BECKMAN _SMITHGEOSHADOWMODEL_GGX _SMITHGEOSHADOWMODEL_SCHLICK _SMITHGEOSHADOWMODEL_SCHLICKBECKMAN _SMITHGEOSHADOWMODEL_SCHLICKGGX _SMITHGEOSHADOWMODEL_IMPLICIT
            #pragma multi_compile _FRESNELMODEL_SCHLICK _FRESNELMODEL_SCHLICKIOR _FRESNELMODEL_SPHERICALGAUSSIAN
            #pragma multi_compile  _ENABLE_NDF_OFF _ENABLE_NDF_ON
            #pragma multi_compile  _ENABLE_G_OFF _ENABLE_G_ON
            #pragma multi_compile  _ENABLE_F_OFF _ENABLE_F_ON
            #pragma multi_compile  _ENABLE_D_OFF _ENABLE_D_ON
            #pragma target 3.0
            
float4 _Color;
float4 _SpecularColor;
float _SpecularPower;
float _SpecularRange;
float _Glossiness;
float _Metallic;
float _Anisotropic;
float _Ior;
float _NormalDistModel;
float _GeoShadowModel;
float _FresnelModel;
float _UnityLightingContribution;

struct VertexInput {
    float4 vertex : POSITION;       //local vertex position
    float3 normal : NORMAL;         //normal direction
    float4 tangent : TANGENT;       //tangent direction    
    float2 texcoord0 : TEXCOORD0;   //uv coordinates
    float2 texcoord1 : TEXCOORD1;   //lightmap uv coordinates
};

struct VertexOutput {
    float4 pos : SV_POSITION;              //screen clip space position and depth
    float2 uv0 : TEXCOORD0;                //uv coordinates
    float2 uv1 : TEXCOORD1;                //lightmap uv coordinates

//below we create our own variables with the texcoord semantic. 
    float3 normalDir : TEXCOORD3;          //normal direction   
    float3 posWorld : TEXCOORD4;          //normal direction   
    float3 tangentDir : TEXCOORD5;
    float3 bitangentDir : TEXCOORD6;
    LIGHTING_COORDS(7,8)                   //this initializes the unity lighting and shadow
    UNITY_FOG_COORDS(9)                    //this initializes the unity fog
};

VertexOutput vert (VertexInput v) {
     VertexOutput o = (VertexOutput)0;           
     o.uv0 = v.texcoord0;
     o.uv1 = v.texcoord1;
     o.normalDir = UnityObjectToWorldNormal(v.normal);
     o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
     o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
     o.pos = UnityObjectToClipPos(v.vertex);
     o.posWorld = mul(unity_ObjectToWorld, v.vertex);
     UNITY_TRANSFER_FOG(o,o.pos);
     TRANSFER_VERTEX_TO_FRAGMENT(o)
     return o;
}

UnityGI GetUnityGI(float3 lightColor, float3 lightDirection, float3 normalDirection,float3 viewDirection, float3 viewReflectDirection, float attenuation, float roughness, float3 worldPos){
 //Unity light Setup ::
    UnityLight light;
    light.color = lightColor;
    light.dir = lightDirection;
    light.ndotl = max(0.0h,dot( normalDirection, lightDirection));
    UnityGIInput d;
    d.light = light;
    d.worldPos = worldPos;
    d.worldViewDir = viewDirection;
    d.atten = attenuation;
    d.ambient = 0.0h;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION) || defined(UNITY_ENABLE_REFLECTION_BUFFERS)
	d.boxMin[0]=unity_SpecCube0_BoxMin;
	d.boxMin[1]=unity_SpecCube1_BoxMin;
	#endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	d.boxMax[1]=unity_SpecCube1_BoxMax;
	d.boxMax[0]=unity_SpecCube0_BoxMax;
	d.probePosition[0]=unity_SpecCube0_ProbePosition;
	d.probePosition[1]=unity_SpecCube1_ProbePosition;
	#endif
    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    Unity_GlossyEnvironmentData ugls_en_data;
    ugls_en_data.roughness = roughness;
    ugls_en_data.reflUVW = viewReflectDirection;
    UnityGI gi = UnityGlobalIllumination(d, 1.0h, normalDirection, ugls_en_data );
    return gi;
}


//---------------------------
//helper functions
float sqr(float x){
	return x*x; 
}
//------------------------------


//------------------------------------------------
//schlick functions
float SchlickFresnel(float i){
    float x = clamp(1.0-i, 0.0, 1.0);
    float x2 = x*x;
    return x2*x2*x;
}
float3 FresnelLerp (float3 x, float3 y, float d)
{
	float t = SchlickFresnel(d);	
	return lerp (x, y, t);
}

float3 SchlickFresnelFunction(float3 SpecularColor,float LdotH){
    return SpecularColor + (1 - SpecularColor)* SchlickFresnel(LdotH);
}

float SchlickIORFresnelFunction(float ior,float LdotH){
    float f0 = pow((ior-1)/(ior+1),2);
    return f0 +  (1 - f0) * SchlickFresnel(LdotH);
}
float SphericalGaussianFresnelFunction(float LdotH,float SpecularColor)
{	
	float power = ((-5.55473 * LdotH) - 6.98316) * LdotH;
    return SpecularColor + (1 - SpecularColor)  * pow(2,power);
}

//-----------------------------------------------



//-----------------------------------------------
//normal incidence reflection calculation
float F0 (float NdotL, float NdotV, float LdotH, float roughness){
// Diffuse fresnel
    float FresnelLight = SchlickFresnel(NdotL); 
    float FresnelView = SchlickFresnel(NdotV);
    float FresnelDiffuse90 = 0.5 + 2.0 * LdotH*LdotH * roughness;
   return  lerp(1, FresnelDiffuse90, FresnelLight) * lerp(1, FresnelDiffuse90, FresnelView);
}

//-----------------------------------------------




//-----------------------------------------------
//Normal Distribution Functions

float BlinnPhongNormalDistribution(float NdotH, float specularpower, float speculargloss){
    float Distribution = pow(NdotH,speculargloss) * specularpower;
    Distribution *= (2+specularpower) / (2*3.1415926535);
    return Distribution;
}
float PhongNormalDistribution(float RdotV, float specularpower, float speculargloss){
    float Distribution = pow(RdotV,speculargloss) * specularpower;
    Distribution *= (2+specularpower) / (2*3.1415926535);
    return Distribution;
}

float BeckmannNormalDistribution(float roughness, float NdotH)
{
    float roughnessSqr = roughness*roughness;
    float NdotHSqr = NdotH*NdotH;
    return max(0.000001,(1.0 / (3.1415926535*roughnessSqr*NdotHSqr*NdotHSqr))* exp((NdotHSqr-1)/(roughnessSqr*NdotHSqr)));
}

float GaussianNormalDistribution(float roughness, float NdotH)
{
    float roughnessSqr = roughness*roughness;
	float thetaH = acos(NdotH);
    return exp(-thetaH*thetaH/roughnessSqr);
}

float GGXNormalDistribution(float roughness, float NdotH)
{
    float roughnessSqr = roughness*roughness;
    float NdotHSqr = NdotH*NdotH;
    float TanNdotHSqr = (1-NdotHSqr)/NdotHSqr;
    return (1.0/3.1415926535) * sqr(roughness/(NdotHSqr * (roughnessSqr + TanNdotHSqr)));
//    float denom = NdotHSqr * (roughnessSqr-1)

}

float TrowbridgeReitzNormalDistribution(float NdotH, float roughness){
    float roughnessSqr = roughness*roughness;
    float Distribution = NdotH*NdotH * (roughnessSqr-1.0) + 1.0;
    return roughnessSqr / (3.1415926535 * Distribution*Distribution);
}

float TrowbridgeReitzAnisotropicNormalDistribution(float anisotropic, float NdotH, float HdotX, float HdotY){
	float aspect = sqrt(1.0h-anisotropic * 0.9h);
	float X = max(.001, sqr(1.0-_Glossiness)/aspect) * 5;
 	float Y = max(.001, sqr(1.0-_Glossiness)*aspect) * 5;
    return 1.0 / (3.1415926535 * X*Y * sqr(sqr(HdotX/X) + sqr(HdotY/Y) + NdotH*NdotH));
}

float WardAnisotropicNormalDistribution(float anisotropic, float NdotL, float NdotV, float NdotH, float HdotX, float HdotY){
    float aspect = sqrt(1.0h-anisotropic * 0.9h);
    float X = max(.001, sqr(1.0-_Glossiness)/aspect) * 5;
 	float Y = max(.001, sqr(1.0-_Glossiness)*aspect) * 5;
    float exponent = -(sqr(HdotX/X) + sqr(HdotY/Y)) / sqr(NdotH);
    float Distribution = 1.0 / ( 3.14159265 * X * Y * sqrt(NdotL * NdotV));
    Distribution *= exp(exponent);
    return Distribution;
}
//--------------------------



//-----------------------------------------------
//Geometric Shadowing Functions

float ImplicitGeometricShadowingFunction (float NdotL, float NdotV){
	float Gs =  (NdotL*NdotV);       
	return Gs;
}

float AshikhminShirleyGeometricShadowingFunction (float NdotL, float NdotV, float LdotH){
	float Gs = NdotL*NdotV/(LdotH*max(NdotL,NdotV));
	return  (Gs);
}

float AshikhminPremozeGeometricShadowingFunction (float NdotL, float NdotV){
	float Gs = NdotL*NdotV/(NdotL+NdotV - NdotL*NdotV);
	return  (Gs);
}

float DuerGeometricShadowingFunction (float3 lightDirection,float3 viewDirection, float3 normalDirection,float NdotL, float NdotV){
    float3 LpV = lightDirection + viewDirection;
    float Gs = dot(LpV,LpV) * pow(dot(LpV,normalDirection),-4);
    return  (Gs);
}

float NeumannGeometricShadowingFunction (float NdotL, float NdotV){
	float Gs = (NdotL*NdotV)/max(NdotL, NdotV);       
	return  (Gs);
}

float KelemenGeometricShadowingFunction (float NdotL, float NdotV, float LdotH, float VdotH){
//	float Gs = (NdotL*NdotV)/ (LdotH * LdotH);           //this
	float Gs = (NdotL*NdotV)/(VdotH * VdotH);       //or this?
	return   (Gs);
}

float ModifiedKelemenGeometricShadowingFunction (float NdotV, float NdotL, float roughness)
{
	float c = 0.797884560802865; // c = sqrt(2 / Pi)
	float k = roughness * roughness * c;
	float gH = NdotV  * k +(1-k);
	return (gH * gH * NdotL);
}

float CookTorrenceGeometricShadowingFunction (float NdotL, float NdotV, float VdotH, float NdotH){
	float Gs = min(1.0, min(2*NdotH*NdotV / VdotH, 2*NdotH*NdotL / VdotH));
	return  (Gs);
}

float WardGeometricShadowingFunction (float NdotL, float NdotV, float VdotH, float NdotH){
	float Gs = pow( NdotL * NdotV, 0.5);
	return  (Gs);
}

float KurtGeometricShadowingFunction (float NdotL, float NdotV, float VdotH, float alpha){
	float Gs =  (VdotH*pow(NdotL*NdotV, alpha))/ NdotL * NdotV;
	return  (Gs);
}

//SmithModelsBelow
//Gs = F(NdotL) * F(NdotV);

float WalterEtAlGeometricShadowingFunction (float NdotL, float NdotV, float alpha){
    float alphaSqr = alpha*alpha;
    float NdotLSqr = NdotL*NdotL;
    float NdotVSqr = NdotV*NdotV;
    float SmithL = 2/(1 + sqrt(1 + alphaSqr * (1-NdotLSqr)/(NdotLSqr)));
    float SmithV = 2/(1 + sqrt(1 + alphaSqr * (1-NdotVSqr)/(NdotVSqr)));
	float Gs =  (SmithL * SmithV);
	return Gs;
}

float BeckmanGeometricShadowingFunction (float NdotL, float NdotV, float roughness){
    float roughnessSqr = roughness*roughness;
    float NdotLSqr = NdotL*NdotL;
    float NdotVSqr = NdotV*NdotV;
    float calulationL = (NdotL)/(roughnessSqr * sqrt(1- NdotLSqr));
    float calulationV = (NdotV)/(roughnessSqr * sqrt(1- NdotVSqr));
    float SmithL = calulationL < 1.6 ? (((3.535 * calulationL) + (2.181 * calulationL * calulationL))/(1 + (2.276 * calulationL) + (2.577 * calulationL * calulationL))) : 1.0;
    float SmithV = calulationV < 1.6 ? (((3.535 * calulationV) + (2.181 * calulationV * calulationV))/(1 + (2.276 * calulationV) + (2.577 * calulationV * calulationV))) : 1.0;
	float Gs =  (SmithL * SmithV);
	return Gs;
}

float GGXGeometricShadowingFunction (float NdotL, float NdotV, float roughness){
    float roughnessSqr = roughness*roughness;
    float NdotLSqr = NdotL*NdotL;
    float NdotVSqr = NdotV*NdotV;
    float SmithL = (2 * NdotL)/ (NdotL + sqrt(roughnessSqr + ( 1-roughnessSqr) * NdotLSqr));
    float SmithV = (2 * NdotV)/ (NdotV + sqrt(roughnessSqr + ( 1-roughnessSqr) * NdotVSqr));
	float Gs =  (SmithL * SmithV) ;
	return Gs;
}

float SchlickGeometricShadowingFunction (float NdotL, float NdotV, float roughness)
{
    float roughnessSqr = roughness*roughness;
	float SmithL = (NdotL)/(NdotL * (1-roughnessSqr) + roughnessSqr);
	float SmithV = (NdotV)/(NdotV * (1-roughnessSqr) + roughnessSqr);
	return (SmithL * SmithV); 
}


float SchlickBeckmanGeometricShadowingFunction (float NdotL, float NdotV, float roughness){
    float roughnessSqr = roughness*roughness;
    float k = roughnessSqr * 0.797884560802865;
    float SmithL = (NdotL)/ (NdotL * (1- k) + k);
    float SmithV = (NdotV)/ (NdotV * (1- k) + k);
	float Gs =  (SmithL * SmithV);
	return Gs;
}

float SchlickGGXGeometricShadowingFunction (float NdotL, float NdotV, float roughness){
    float k = roughness / 2;
    float SmithL = (NdotL)/ (NdotL * (1- k) + k);
    float SmithV = (NdotV)/ (NdotV * (1- k) + k);
	float Gs =  (SmithL * SmithV);
	return Gs;
}

//--------------------------


float4 frag(VertexOutput i) : COLOR {

//normal direction calculations
     float3 normalDirection = normalize(i.normalDir);
	 float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
     float shiftAmount = dot(i.normalDir, viewDirection);
	 normalDirection = shiftAmount < 0.0f ? normalDirection + viewDirection * (-shiftAmount + 1e-5f) : normalDirection;

//light calculations
	 float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
	 float3 lightReflectDirection = reflect( -lightDirection, normalDirection );
	 float3 viewReflectDirection = normalize(reflect( -viewDirection, normalDirection ));
     float NdotL = max(0.0, dot( normalDirection, lightDirection ));
     float3 halfDirection = normalize(viewDirection+lightDirection); 
     float NdotH =  max(0.0,dot( normalDirection, halfDirection));
     float NdotV =  max(0.0,dot( normalDirection, viewDirection));
     float VdotH = max(0.0,dot( viewDirection, halfDirection));
     float LdotH =  max(0.0,dot(lightDirection, halfDirection)); 
     float LdotV = max(0.0,dot(lightDirection, viewDirection)); 
     float RdotV = max(0.0, dot( lightReflectDirection, viewDirection ));
     float attenuation = LIGHT_ATTENUATION(i);
     float3 attenColor = attenuation * _LightColor0.rgb;
     
     //get Unity Scene lighting data
     UnityGI gi =  GetUnityGI(_LightColor0.rgb, lightDirection, normalDirection, viewDirection, viewReflectDirection, attenuation, 1- _Glossiness, i.posWorld.xyz);
     float3 indirectDiffuse = gi.indirect.diffuse.rgb ;
	 float3 indirectSpecular = gi.indirect.specular.rgb;

	 //diffuse color calculations
	 float roughness = 1-(_Glossiness * _Glossiness);
	 roughness = roughness * roughness;
	 


	//Specular calculations


	 float3 specColor = lerp(_SpecularColor.rgb, _Color.rgb, _Metallic * 0.5);

	 float SpecularDistribution = 1;
	 float GeometricShadow = 1;
	 float3 FresnelFunction = specColor;

	 //Normal Distribution Function/Specular Distribution-----------------------------------------------------	      

           
	#ifdef _NORMALDISTMODEL_BLINNPHONG 
		 SpecularDistribution *=  BlinnPhongNormalDistribution(NdotH, _Glossiness,  max(1,_Glossiness * 40));
 	#elif _NORMALDISTMODEL_PHONG
		 SpecularDistribution *=  PhongNormalDistribution(RdotV, _Glossiness, max(1,_Glossiness * 40));
 	#elif _NORMALDISTMODEL_BECKMANN
		 SpecularDistribution *=  BeckmannNormalDistribution(roughness, NdotH);
 	#elif _NORMALDISTMODEL_GAUSSIAN
		 SpecularDistribution *=  GaussianNormalDistribution(roughness, NdotH);
 	#elif _NORMALDISTMODEL_GGX
		 SpecularDistribution *=  GGXNormalDistribution(roughness, NdotH);
 	#elif _NORMALDISTMODEL_TROWBRIDGEREITZ
		 SpecularDistribution *=  TrowbridgeReitzNormalDistribution(NdotH, roughness);
 	#elif _NORMALDISTMODEL_TROWBRIDGEREITZANISOTROPIC
		 SpecularDistribution *=  TrowbridgeReitzAnisotropicNormalDistribution(_Anisotropic,NdotH, dot(halfDirection, i.tangentDir), dot(halfDirection,  i.bitangentDir));
	#elif _NORMALDISTMODEL_WARD
	 	 SpecularDistribution *=  WardAnisotropicNormalDistribution(_Anisotropic,NdotL, NdotV, NdotH, dot(halfDirection, i.tangentDir), dot(halfDirection,  i.bitangentDir));
	#else
		SpecularDistribution *=  GGXNormalDistribution(roughness, NdotH);
	#endif

	 //Geometric Shadowing term----------------------------------------------------------------------------------
	#ifdef _SMITHGEOSHADOWMODEL_NONE
	 	#ifdef _GEOSHADOWMODEL_ASHIKHMINSHIRLEY
			GeometricShadow *= AshikhminShirleyGeometricShadowingFunction (NdotL, NdotV, LdotH);
	 	#elif _GEOSHADOWMODEL_ASHIKHMINPREMOZE
			GeometricShadow *= AshikhminPremozeGeometricShadowingFunction (NdotL, NdotV);
	 	#elif _GEOSHADOWMODEL_DUER
			GeometricShadow *= DuerGeometricShadowingFunction (lightDirection, viewDirection, normalDirection, NdotL, NdotV);
	 	#elif _GEOSHADOWMODEL_NEUMANN
			GeometricShadow *= NeumannGeometricShadowingFunction (NdotL, NdotV);
	 	#elif _GEOSHADOWMODEL_KELEMAN
			GeometricShadow *= KelemenGeometricShadowingFunction (NdotL, NdotV, LdotH,  VdotH);
	 	#elif _GEOSHADOWMODEL_MODIFIEDKELEMEN
			GeometricShadow *=  ModifiedKelemenGeometricShadowingFunction (NdotV, NdotL, roughness);
	 	#elif _GEOSHADOWMODEL_COOK
			GeometricShadow *= CookTorrenceGeometricShadowingFunction (NdotL, NdotV, VdotH, NdotH);
	 	#elif _GEOSHADOWMODEL_WARD
			GeometricShadow *= WardGeometricShadowingFunction (NdotL, NdotV, VdotH, NdotH);
	 	#elif _GEOSHADOWMODEL_KURT
			GeometricShadow *= KurtGeometricShadowingFunction (NdotL, NdotV, VdotH, roughness);
	 	#else 			
 			GeometricShadow *= ImplicitGeometricShadowingFunction (NdotL, NdotV);
 		#endif
	////SmithModelsBelow
	////Gs = F(NdotL) * F(NdotV);
  	#elif _SMITHGEOSHADOWMODEL_WALTER
		GeometricShadow *= WalterEtAlGeometricShadowingFunction (NdotL, NdotV, roughness);
	#elif _SMITHGEOSHADOWMODEL_BECKMAN
		GeometricShadow *= BeckmanGeometricShadowingFunction (NdotL, NdotV, roughness);
 	#elif _SMITHGEOSHADOWMODEL_GGX
		GeometricShadow *= GGXGeometricShadowingFunction (NdotL, NdotV, roughness);
	#elif _SMITHGEOSHADOWMODEL_SCHLICK
		GeometricShadow *= SchlickGeometricShadowingFunction (NdotL, NdotV, roughness);
 	#elif _SMITHGEOSHADOWMODEL_SCHLICKBECKMAN
		GeometricShadow *= SchlickBeckmanGeometricShadowingFunction (NdotL, NdotV, roughness);
 	#elif _SMITHGEOSHADOWMODEL_SCHLICKGGX
		GeometricShadow *= SchlickGGXGeometricShadowingFunction (NdotL, NdotV, roughness);
	#elif _SMITHGEOSHADOWMODEL_IMPLICIT
		GeometricShadow *= ImplicitGeometricShadowingFunction (NdotL, NdotV);
	#else
		GeometricShadow *= ImplicitGeometricShadowingFunction (NdotL, NdotV);
 	#endif
	 //Fresnel Function-------------------------------------------------------------------------------------------------

	#ifdef _FRESNELMODEL_SCHLICK
		FresnelFunction *=  SchlickFresnelFunction(specColor, LdotH);
	#elif _FRESNELMODEL_SCHLICKIOR
		FresnelFunction *=  SchlickIORFresnelFunction(_Ior, LdotH);
	#elif _FRESNELMODEL_SPHERICALGAUSSIAN
		FresnelFunction *= SphericalGaussianFresnelFunction(LdotH, specColor);
 	#else
		FresnelFunction *=  SchlickIORFresnelFunction(_Ior, LdotH);	
 	#endif


 	#ifdef _ENABLE_NDF_ON
 	 return float4(float3(1,1,1)* SpecularDistribution,1);
    #endif
    #ifdef _ENABLE_G_ON 
 	 return float4(float3(1,1,1) * GeometricShadow,1) ;
    #endif
    #ifdef _ENABLE_F_ON 
 	 return float4(float3(1,1,1)* FresnelFunction,1);
    #endif
	#ifdef _ENABLE_D_ON 
 	 return float4(float3(1,1,1)* diffuseColor,1);
    #endif
	 //PBR
     float3 diffuseColor = _Color.rgb * (1.0 - _Metallic) ;
 	 float f0 = F0(NdotL, NdotV, LdotH, roughness);
	 diffuseColor *= f0;
	 diffuseColor+=indirectDiffuse;
	 float3 specularity = (SpecularDistribution * FresnelFunction * GeometricShadow);
  //   float grazingTerm = saturate(roughness + _Metallic);
	 //float3 unityIndirectSpecularity =  indirectSpecular * FresnelLerp(specColor,grazingTerm,NdotV) * max(0.15,_Metallic) * (1-roughness*roughness* roughness);
     float3 lightingModel = ((diffuseColor) + specularity );//+ (unityIndirectSpecularity *_UnityLightingContribution));
     lightingModel *= NdotL;

     float4 finalDiffuse = float4(lightingModel * attenColor,1);
     UNITY_APPLY_FOG(i.fogCoord, finalDiffuse);
     return finalDiffuse;
}
ENDCG
}
}
FallBack "Legacy Shaders/Diffuse"
}
