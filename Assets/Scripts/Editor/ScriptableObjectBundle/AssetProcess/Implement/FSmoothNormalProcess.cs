using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Process
{
    public class FMeshProcess_SmoothNormal: AModelProcess
    {
        public EVertexAttribute m_OutputAttributeFlags = EVertexAttribute.Tangent;
        protected override bool PreProcess(ModelImporter _importer)
        {
            if (m_OutputAttributeFlags == EVertexAttribute.Tangent && _importer.importTangents != ModelImporterTangents.None)
            {
                _importer.importTangents = ModelImporterTangents.CalculateLegacy;
                return true;
            }
            return false;
        }

        protected override bool PostProcess(Mesh _mesh)
        {
            var smoothNormals = UModel.GenerateSmoothNormals(_mesh,m_OutputAttributeFlags is not (EVertexAttribute.Normal or EVertexAttribute.Tangent)).Select(p=>p.ToVector4(1f)).ToArray();
            _mesh.SetVertexData((EVertexAttributeFlags)m_OutputAttributeFlags,smoothNormals);
            return true;
        }
    }
}