using UnityEditor;
using UnityEngine;
namespace TEditor
{
    using Rendering.PostProcess;
    [CustomEditor(typeof(PostProcess_ColorUpgrade))]
    public class PostProcess_Color : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            if(GUILayout.Button("Sepia Tone Filter"))
            {
                PostProcess_ColorUpgrade colorUpgradeGrading = target as PostProcess_ColorUpgrade;
                colorUpgradeGrading.m_Data = PPData_ColorUpgrade.m_Default;
                colorUpgradeGrading.m_Data.m_MixRed = new Vector3(0.393f, 0.349f, 0.272f)-Vector3.right;
                colorUpgradeGrading.m_Data.m_MixGreen = new Vector3(0.769f, 0.686f, 0.534f)-Vector3.up;
                colorUpgradeGrading.m_Data.m_MixBlue = new Vector3(0.189f, 0.168f, 0.131f)-Vector3.forward;
                colorUpgradeGrading.OnValidate();
            }
            GUILayout.EndVertical();
        }
    }

}
