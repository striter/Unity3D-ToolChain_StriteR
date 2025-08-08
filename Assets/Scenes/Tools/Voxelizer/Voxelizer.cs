
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace Runtime.Optimize.Voxelizer
{
    public class Voxelizer : MonoBehaviour
    {
        public VoxelData m_Data;
        public EResolution m_Resolution = EResolution._64;

        [EditorPath] public string m_AssetPath;
        [Fold(nameof(m_Data), null), EditorPath] public string m_3DTexturePath;
        [InspectorButton]
        void OutputAsset() => m_Data = VoxelData.Construct(m_Resolution, transform.GetComponentsInChildren<Renderer>(false),UEPath.PathRegex(m_AssetPath));
        
        [InspectorButtonFold(nameof(m_Data),null)]
        void OutputTexture3D() => m_Data.OutputTexture3D(UEPath.PathRegex(m_3DTexturePath));

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            m_Data?.DrawGizmos();
        }
    }

}
#endif