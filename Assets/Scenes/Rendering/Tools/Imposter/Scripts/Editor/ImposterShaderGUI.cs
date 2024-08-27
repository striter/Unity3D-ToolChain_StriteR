using Runtime.Geometry;
using Runtime.Geometry.Explicit.Sphere;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    public class ImposterShaderGUI : ShaderGUIExtension
    {
        [Readonly] public ImposterInput m_Input = ImposterInput.kDefault;
        [Readonly] public GSphere m_BoundingSphere;
        public bool m_DrawGizmos;
        public bool m_DrawInput;
        public override void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void OnDisable() { 
            SceneView.duringSceneGui -= OnSceneGUI; 
        }

        public override void OnMaterialPreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background)
        {
            base.OnMaterialPreviewGUI(materialEditor, r, background);
            // Debug.Log(MaterialEditor.GetMaterialProperty(materialEditor.targets, ImposterShaderProperties.kBounding).vectorValue);
            m_BoundingSphere = new GSphere(MaterialEditor.GetMaterialProperty(materialEditor.targets, ImposterShaderProperties.kBounding).vectorValue);
            m_Input.mapping = (ESphereMapping)MaterialEditor.GetMaterialProperty(materialEditor.targets, ImposterShaderProperties.kMode).floatValue;
            m_Input.count = (EImposterCount)(MaterialEditor.GetMaterialProperty(materialEditor.targets, ImposterShaderProperties.kTexel).vectorValue.x);
            m_Input.Ctor();
        }


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            m_DrawGizmos = EditorGUILayout.Toggle("Draw Gizmos", m_DrawGizmos);
            if(m_DrawGizmos)
                m_DrawInput = EditorGUILayout.Toggle("Draw Input", m_DrawInput);
            
            EditorGUILayout.EndVertical();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (m_Renderer == null || !m_DrawGizmos)
                return;
            
            var _transform = m_Renderer.transform;
            var _viewTransform = sceneView.camera.transform;
            var center = _transform.TransformPoint(m_BoundingSphere.center);

            Handles.matrix = Matrix4x4.TRS(center,Quaternion.identity,_transform.lossyScale * m_BoundingSphere.radius);
            var viewDirection = _transform.worldToLocalMatrix.rotation * (_viewTransform.position - center).normalized;
            if(m_DrawInput)
                m_Input.DrawHandles(viewDirection);
            
            var output = m_Input.GetImposterViews(viewDirection);

            var uv = m_Input.mapping.SphereToUV(viewDirection);
            Handles.color = Color.blue;
            UHandles.DrawString(viewDirection,(uv *(int)m_Input.count).ToString(),0.02f);
            UHandles.DrawWireSphere(output.centroid,.01f);
            Handles.DrawWireCube(m_Input.mapping.UVToSphere(uv), Vector3.one * 0.01f);
            for(var i = 0 ; i < 4 ; i++)
            {
                var weight = output.weights[i];
                var corner = m_Input.GetImposterCorner(output.corners[i]);
                Handles.color =  UColor.IndexToColor(i);
                Handles.DrawWireCube( corner.direction, Vector3.one * 0.02f * weight);
                UHandles.DrawString(corner.direction,corner.cellIndex.ToString(),0.04f);
            }
        }
    }
}