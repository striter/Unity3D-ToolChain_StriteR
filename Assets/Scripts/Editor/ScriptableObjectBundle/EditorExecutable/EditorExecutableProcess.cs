
using System;
using UnityEngine;

namespace UnityEditor.Extensions.EditorExecutable
{
    public interface IEditorExecutableProcessContinuous
    {
        public bool Executing();
        public float process { get; }
        public void Cancel();
        public void End();
        public void OnGUI();
    }
    
    public abstract class EditorExecutableProcess : AScriptableObjectBundleElement
    {
        public abstract bool Execute();
    }
}