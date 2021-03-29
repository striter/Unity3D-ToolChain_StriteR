using System.Collections;
using System.Collections.Generic;
using Rendering.ImageEffect;
using UnityEditor;
using UnityEngine;
namespace TEditor
{
    [CustomEditor(typeof(PostEffect_ColorGrading))]
    public class EPostEffect_ColorGrading : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            GUILayout.Label("Extra",UEGUIStyle_Window.m_TitleLabel);
            if(GUILayout.Button("Sepia Tone Filter"))
            {
                PostEffect_ColorGrading colorGrading = target as PostEffect_ColorGrading;
                colorGrading.m_EffectData = ImageEffectParam_ColorGrading.m_Default;
                colorGrading.m_EffectData.m_MixRed = new Vector3(0.393f, 0.349f, 0.272f)-Vector3.right;
                colorGrading.m_EffectData.m_MixGreen = new Vector3(0.769f, 0.686f, 0.534f)-Vector3.up;
                colorGrading.m_EffectData.m_MixBlue = new Vector3(0.189f, 0.168f, 0.131f)-Vector3.forward;
                colorGrading.OnValidate();
            }
            GUILayout.EndVertical();
        }
    }

}
