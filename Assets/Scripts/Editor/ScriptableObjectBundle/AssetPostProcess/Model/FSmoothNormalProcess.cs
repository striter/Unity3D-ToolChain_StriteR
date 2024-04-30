using System;
using System.Linq;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Model
{
    [Serializable]
    public class FMeshProcess_SmoothNormal: AModelProcess
    {
        public override string pathFilter => m_Path;
        [EditorPath] public string m_Path;
        public EVertexAttribute m_OutputAttribute;
        protected override void ProcessMesh(Mesh _mesh)
        {
            var smoothNormals = UModeling.GenerateSmoothNormals(_mesh,m_OutputAttribute is EVertexAttribute.Normal or EVertexAttribute.Tangent).Select(p=>p.ToVector4(1f)).ToArray();
            _mesh.SetVertexData(m_OutputAttribute, smoothNormals);
        }
    }
}