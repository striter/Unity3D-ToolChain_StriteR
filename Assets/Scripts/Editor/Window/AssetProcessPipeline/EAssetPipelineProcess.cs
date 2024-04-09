using System;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    public abstract class EAssetPipelineProcess : ScriptableObject
    {
        public abstract void Begin();
        public abstract bool Executing();
        public abstract float process { get; }
        public abstract void Cancel();
        public abstract void End();
        
        public virtual void OnGUI()
        {
            
        }
        
    }
}