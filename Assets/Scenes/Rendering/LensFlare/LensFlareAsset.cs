using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.Rendering.LensFlare
{
    [Serializable]
    public struct LensFlarePreBakeData
    {
        public Texture2D texture;
        public LensFlareData data ;

        public static readonly LensFlarePreBakeData kDefault = new()
        {
            texture = null,
            data = LensFlareData.kDefault,
        };
    }
    
    [Serializable]
    public struct LensFlareData
    {
        public float size;
        [ColorUsage(false,true)]public Color color;
        [Range(-.5f,1.5f)]public float offset;

        public static readonly LensFlareData kDefault = new() {
            size = 1f,
            color = Color.white,
            offset = .5f,
        };
    }

    [Serializable]
    public struct LensFlareBakedData
    {
        public float4 bakedST;
        public LensFlareData data;
        public static LensFlareBakedData kDefault = new() { bakedST = new float4(0, 0, 1, 1), data = LensFlareData.kDefault };
    }
    
    [CreateAssetMenu(fileName = "LensFlare_Default", menuName = "Light/LensFlareAsset")]
    public class LensFlareAsset : ScriptableObject
    {
        public List<LensFlarePreBakeData> m_CellsData = new () { LensFlarePreBakeData.kDefault };
        
        [HideInInspector] public bool m_Baked;
        [Foldout(nameof(m_Baked),true),Readonly]public Texture2D m_BakeTexture;
        [Foldout(nameof(m_Baked),true),Readonly]public List<LensFlareBakedData> m_BakedData = new();
        private void OnValidate()
        {
            m_Baked = false;
        }

        [InspectorButtonFoldout(nameof(m_Baked),false)]
        void Bake()
        {
            m_Baked = true;
        }
        
    }
    
}