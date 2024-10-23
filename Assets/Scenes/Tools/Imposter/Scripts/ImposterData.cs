using Runtime.Geometry;
using UnityEngine;

namespace Runtime.Optimize.Imposter
{
    public class ImposterData : ScriptableObject
    {
        [Range(0,1)] public float m_Parallax;

        [Readonly] public bool m_Instanced;
        [Readonly] public ImposterInput m_Input = ImposterInput.kDefault;
        [Readonly] public GSphere m_BoundingSphere;
        [Readonly] public Material m_Material;
        [Readonly] public Mesh m_Mesh;
        
        public static GameObject CreateImposterRenderer(ImposterData imposterData,Transform dropRoot = null)
        {
            var gameObject = new GameObject(imposterData.name);
            var transform = gameObject.transform;
            if (dropRoot != null)
            {
                transform.SetParentAndSyncPositionRotation(dropRoot);
                transform.rotation = Quaternion.identity;   
            }
            
            if (imposterData.m_Instanced)
            {
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = imposterData.m_Material;
                gameObject.AddComponent<MeshFilter>().sharedMesh = imposterData.m_Mesh;
            }
            else
            {
                var renderer = gameObject.AddComponent<ImposterRenderer>();
                renderer.m_Data = imposterData;
                renderer.OnValidate();
            }

            return gameObject;
        }
    }
}