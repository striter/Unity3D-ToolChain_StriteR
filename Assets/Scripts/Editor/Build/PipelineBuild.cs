using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

class PipelineShaderProcesser : UnityEditor.Build.IPreprocessShaders
{
    public int callbackOrder { get { return 0; } }
    private static readonly string kValidateShader = "Game/Lit/Vegetation";

    public void OnProcessShader(Shader _shader, ShaderSnippetData _snippet, IList<ShaderCompilerData> _data)
    {
        if (_shader.name != kValidateShader)
            return;
        Debug.LogWarning(_shader.name +" "+_snippet.passType + " " +_snippet.shaderType  );
        for (int i = _data.Count - 1; i >= 0; --i)
        {
            var data = _data[i];
            var keywords = data.shaderKeywordSet.GetShaderKeywords();
            if(keywords.Length==0)
                continue;
            
            Debug.Log( keywords.ToString(',', p =>
            {
                string name = ShaderKeyword.GetGlobalKeywordName(p);
                if (string.IsNullOrEmpty(name))
                    name = ShaderKeyword.GetKeywordName(_shader,p);
                return name;
            }));
        }
    }
}