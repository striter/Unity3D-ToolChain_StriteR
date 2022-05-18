//Instance
#define INSTANCING_BUFFER_START UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
#define INSTANCING_PROP(type,param) UNITY_DEFINE_INSTANCED_PROP(type,param)
#define INSTANCING_BUFFER_END UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
#define INSTANCE(param) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,param)
#define TRANSFORM_TEX_INSTANCE(uv,tex) TransformTex(uv,INSTANCE(tex##_ST))

#define TRANSFORM_TEX_FLOW_INSTANCE(uv,tex) TransformTex_Flow(uv,INSTANCE(tex##_ST))