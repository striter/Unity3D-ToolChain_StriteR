using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Process
{
    public class FMeshProcess_SmoothNormal: AModelProcess
    {
        public EVertexAttributeFlags m_OutputAttributeFlags;

        protected override bool Preprocess(ModelImporter _importer)
        {
            if (m_OutputAttributeFlags == EVertexAttributeFlags.Tangent && _importer.importTangents != ModelImporterTangents.None)
            {
                _importer.importTangents = ModelImporterTangents.CalculateLegacy;
                return true;
            }
            return false;
        }

        protected override bool PostProcess(Mesh _mesh)
        {
            var smoothNormals = UModel.GenerateSmoothNormals(_mesh,m_OutputAttributeFlags is not (EVertexAttributeFlags.Normal or EVertexAttributeFlags.Tangent)).Select(p=>p.ToVector4(1f)).ToArray();
            _mesh.SetVertexData(m_OutputAttributeFlags, smoothNormals);
            return true;
        }
    }
}