using System;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [CreateAssetMenu(menuName = "EditorProcessPipeline/Executable", fileName = "EditorProcessPipelineExecutable", order = 0)]
    public class EditorExecutableBundle : AScriptableObjectBundle
    {
        public override Type GetBaseType() => typeof(EditorExecutableProcess);
    }
}