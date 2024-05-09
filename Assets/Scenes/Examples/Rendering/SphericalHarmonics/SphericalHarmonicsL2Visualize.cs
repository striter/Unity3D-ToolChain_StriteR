using System;
using Runtime;
using TObjectPool;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.SH
{
    //http://www.ppsloan.org/publications/StupidSH36.pdf
    
    [Serializable]
    public class FSphericalHarmonicsL2VisualizeCore : ARuntimeRendererBase
    {
        public int resolution;
        protected override string GetInstanceName() => "SHL2";
        protected override void PopulateMesh(Mesh _mesh, Transform _transform, Transform _viewTransform)
        {
            
            
        }
    }
    
    [ExecuteInEditMode]
    public class SphericalHarmonicsL2Visualize : ARuntimeRendererMonoBehaviour<FSphericalHarmonicsL2VisualizeCore>
    {
    }
}