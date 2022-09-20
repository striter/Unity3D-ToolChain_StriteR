using UnityEditor;
using UnityEngine;
using Rendering.PostProcess;
[CustomEditor(typeof(PostProcess_ColorUpgrade))]
public class EPostProcess_ColorUpgrade : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.BeginVertical();
        if(GUILayout.Button("Sepia Tone Filter"))
        {
            PostProcess_ColorUpgrade colorUpgrade = target as PostProcess_ColorUpgrade;
            colorUpgrade.m_Data = PPData_ColorUpgrade.kDefault;
            colorUpgrade.m_Data.m_MixRed = new Vector3(0.393f, 0.349f, 0.272f)-Vector3.right;
            colorUpgrade.m_Data.m_MixGreen = new Vector3(0.769f, 0.686f, 0.534f)-Vector3.up;
            colorUpgrade.m_Data.m_MixBlue = new Vector3(0.189f, 0.168f, 0.131f)-Vector3.forward;
            colorUpgrade.ValidateParameters();
        }
        GUILayout.EndVertical();
    }
}
