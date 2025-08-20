using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;

namespace Rendering.Pipeline.GrabPass
{
    [ExecuteInEditMode]
    public class GrabTextureBehaviour : MonoBehaviour
    {
        public bool m_Static = false;
        public GrabTextureData m_Data = GrabTextureData.kDefault;
        public static List<GrabTextureBehaviour> kActiveComponents = new List<GrabTextureBehaviour>();
        private Dictionary<int,GrabTextureBuffer> m_Buffers = new Dictionary<int, GrabTextureBuffer>();
        public GrabTexturePass m_Pass { get; private set; } = new();
        
        [InspectorButtonFoldout(nameof(m_Static),false)]
        public void Recapture()
        {
            m_Buffers.Values.Traversal(p=>p.Dispose());
            m_Buffers.Clear();
        }
        
        private void OnEnable()
        {
            kActiveComponents.Add(this);
            m_Pass.Setup(this);
        }

        private void OnDisable()
        {
            kActiveComponents.Remove(this);
            Recapture();
        }

        private void OnValidate()
        {
            Recapture();
        }

        public GrabTextureBuffer GetBuffer(Camera _camera,RenderTextureDescriptor _descriptor)
        {
            var instanceID = _camera.GetInstanceID();
            if (!m_Buffers.ContainsKey(instanceID))
                m_Buffers.Add(instanceID,new ());
            return m_Buffers[instanceID].Validate(this.name,_descriptor);
        }
    }

    public class GrabTextureBuffer
    {
        public RenderTexture texture;
        public RenderTextureDescriptor descriptor;
        private bool capture = true;
        
        static int kBufferIndex = 0;
        public GrabTextureBuffer Validate(string _parentName,RenderTextureDescriptor _descriptor)
        {
            if (descriptor.Equals(_descriptor))
                return this;
            
            RenderTexture.ReleaseTemporary(texture);
            texture = RenderTexture.GetTemporary(_descriptor);
            texture.name = $"_GrabTextureBuffer{kBufferIndex++}({_parentName})";
            descriptor = _descriptor;
            return this;
        }

        public bool Capture(GrabTextureBehaviour _data)
        {
            if (!_data.m_Static)
                return true;
            
            if (!capture)
                return false;
            capture = false;
            return true;
        }
        
        public void Dispose()
        {
            RenderTexture.ReleaseTemporary(texture);
        }
    }
}