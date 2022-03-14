using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TEditor
{
    internal static class ScriptTemplates
    {
        private const string kTemplateFolder = "Assets/Scripts/Editor/Templates/";
        [MenuItem(itemName:"Assets/Create/Shader/HLSLShader")]
        static void CreateHLSLShaderTemplates()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(kTemplateFolder+"HLSLTemplate.shader.txt","DefaultHLSL.shader");
        }
        
        [MenuItem(itemName:"Assets/Create/Shader/HLSLInclude")]
        static void CreateHLSLIncludeTemplates()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(kTemplateFolder+"HLSLIncludeTemplate.hlsl.txt","DefaultHLSLInclude.hlsl");
        }
    }
    
    
}