using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Explicit;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions;
using UnityEditor.Extensions.TextureEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Examples.Rendering.Imposter
{
    public class Imposter : MonoBehaviour
    {
        public GSphere m_BoundingSphere;
        public RenderTexture m_RenderTexture;
        public int width = 4;
        public int height = 2;
        
        [Button]
        void Construct()
        {
            if (!UEAsset.SaveFilePath(out var filePath, ETextureExportType.PNG.GetExtension(), "Imposter"))
                return;
            
            var meshRenderers = GetComponentsInChildren<MeshRenderer>(false);

            var camera = GetComponentInChildren<Camera>();
            
            List<float3> verticies = new List<float3>();
            foreach (var renderer in meshRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                    continue;
                
                var matrix = renderer.transform.localToWorldMatrix;
                var intersectMesh = meshFilter.sharedMesh;
                verticies.AddRange(intersectMesh.vertices.Select(p=>(float3)matrix.MultiplyPoint(p)));
            }

            if (verticies.Count == 0)
                return;

            m_BoundingSphere = UGeometry.GetBoundingSphere(verticies);
            
            camera.orthographicSize = m_BoundingSphere.radius;
            camera.targetTexture = m_RenderTexture;
            RenderTexture.active = m_RenderTexture;
            GL.Clear(true,true,Color.black);
            
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black.SetA(0f);
            var rectSize = 1f / new float2(width,height);
            for (var j = 0 ; j < height ; j++)
            for(var i = 0 ; i < width ; i++)
            {
                var uv = new float2((i+.5f) / width, (j+.5f) / height);
                
                var direction = USphereExplicit.UV.GetPoint(uv);
                var position =  m_BoundingSphere.center + direction * (m_BoundingSphere.radius + 0.1f);
                
                camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(-direction,Vector3.up));
                camera.rect = new Rect(uv.x - rectSize.x/2,uv.y - rectSize.y/2,rectSize.x,rectSize.y); 
                camera.Render();
            }

            var texture = new Texture2D(m_RenderTexture.width, m_RenderTexture.height,TextureFormat.ARGB32,false);
            texture.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
            texture.Apply();
            
            UTextureEditor.ExportTexture(texture,filePath,ETextureExportType.JPG);
            
            RenderTexture.active = null;
            camera.targetTexture = null;
        }

        private void OnDrawGizmos()
        {
            m_BoundingSphere.DrawGizmos();
        }
    }
    
}
