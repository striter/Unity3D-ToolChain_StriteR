using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    public enum EModelProcess
    {
        SmoothNormal,
    }
    
    public interface IMeshProcess
    {
        void Execute(Mesh _mesh);
    }
    
    
    [Serializable]
    public class MeshProcessRules
    {
        public string m_Directory;
        public EModelProcess m_Process;
        [MFoldout(nameof(m_Process), EModelProcess.SmoothNormal)]public FMeshProcess_SmoothNormal m_SmoothNormalProcess;

        IMeshProcess GetProcess()
        {
            return m_Process switch
            {
                EModelProcess.SmoothNormal => m_SmoothNormalProcess,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public void ProcessMesh(Mesh _mesh) => GetProcess().Execute(_mesh);
    }


    [Serializable]
    public struct FMeshProcess_SmoothNormal: IMeshProcess
    {
        public EVertexAttribute m_OutputAttribute;
        public void Execute(Mesh _mesh)
        {
            var smoothNormals = UModeling.GenerateSmoothNormals(_mesh,m_OutputAttribute is EVertexAttribute.Normal or EVertexAttribute.Tangent).Select(p=>p.ToVector4(1f)).ToArray();
            _mesh.SetVertexData(m_OutputAttribute, smoothNormals);
        }
    }
}