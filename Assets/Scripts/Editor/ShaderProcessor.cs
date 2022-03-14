using System.Collections;
using System.Collections.Generic;
using Rendering.PostProcess;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

public class ShaderProcessor : IPreprocessShaders
{
    public int callbackOrder { get; }
    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        // foreach (var compile in data)
        // {
        //     Debug.Log(compile.shaderKeywordSet.GetShaderKeywords().ToString(','));
        // }
    }
}
