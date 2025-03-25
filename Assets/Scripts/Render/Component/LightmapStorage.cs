using Rendering.Lightmap;
using UnityEngine;

namespace Rendering.Pipeline
{
    
    [ExecuteInEditMode]
    public class LightmapStorage : MonoBehaviour
    {
        public GlobalIllumination_LightmapDiffuse m_Diffuse;

        private void Awake()
        {
            if (m_Diffuse == null)
                return;
            
        #if UNITY_EDITOR
            if(UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;
        #endif
            
            m_Diffuse.Apply(transform);
        }

        [InspectorButton(true)]
        public void Export()
        {
            m_Diffuse = GlobalIllumination_LightmapDiffuse.Export(transform);
        }
    }

}