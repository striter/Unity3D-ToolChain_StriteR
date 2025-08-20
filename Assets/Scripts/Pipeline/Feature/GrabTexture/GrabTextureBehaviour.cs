using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline.GrabPass
{
    [ExecuteInEditMode]
    public class GrabTextureBehaviour : MonoBehaviour
    {
        public static List<GrabTextureBehaviour> kActiveComponents = new List<GrabTextureBehaviour>();
        public GrabTextureData m_Data = GrabTextureData.kDefault;
        [field:SerializeField] public bool executed { get; private set; } = false;

        private void OnEnable()
        {
            kActiveComponents.Add(this);
            executed = false;
        }

        private void OnDisable()
        {
            kActiveComponents.Remove(this);
            executed = true;
        }
    }
}