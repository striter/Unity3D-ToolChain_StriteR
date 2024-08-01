using System;
using System.Linq;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Model
{
    [Serializable]
    public class FMeshProcess_SmoothNormal: AModelProcess
    {
        public EVertexAttribute m_OutputAttribute;
        protected override void ProcessMesh(Mesh _mesh)
        {
            var smoothNormals = UModeling.GenerateSmoothNormals(_mesh,m_OutputAttribute is not (EVertexAttribute.Normal or EVertexAttribute.Tangent)).Select(p=>p.ToVector4(1f)).ToArray();
            _mesh.SetVertexData(m_OutputAttribute, smoothNormals);
        }
    }
}