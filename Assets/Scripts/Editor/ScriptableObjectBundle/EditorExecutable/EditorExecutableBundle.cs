using System;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [CreateAssetMenu(menuName = "EditorExecutable/Bundle", fileName = "NewEditorExecutableBundle", order = 0)]
    public class EditorExecutableBundle : AScriptableObjectBundle
    {
        public override Type GetBaseType() => typeof(EditorExecutableProcess);
    }
}