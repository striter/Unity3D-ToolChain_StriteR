using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline.GrabPass
{
    [ExecuteInEditMode]
    public class GrabTexture : MonoBehaviour
    {
        public static List<GrabTexture> kActiveComponents = new List<GrabTexture>();
        public GrabTextureData m_Data = GrabTextureData.kDefault;
        private void OnEnable() => kActiveComponents.Add(this);
        private void OnDisable() => kActiveComponents.Remove(this);
    }
}