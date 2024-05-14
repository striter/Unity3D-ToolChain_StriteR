using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [CreateAssetMenu(menuName = "AssetPipeline/Executable", fileName = "AssetPipelineExecutable", order = 0)]
    public class EAssetPipelineExecutable : AScriptableObjectBundle
    {

        public override Type GetBaseType() => typeof(EAssetPipelineProcess);
    }
}