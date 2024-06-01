using System;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering.GI.SphericalHarmonics
{
    [Serializable]
    public struct SHL2ShaderConstants
    {
        public float4 shAr;
        public float4 shAg;
        public float4 shAb;
        public float4 shBr;
        public float4 shBg;
        public float4 shBb;
        public float3 shC;
        public static implicit operator SHL2ShaderConstants(SHL2Data _data)
        {
            var l00 = _data.l00;
            var l10 = _data.l10; var l11 = _data.l11; var l12 = _data.l12;
            var l20 = _data.l20; var l21 = _data.l21; var l22 = _data.l22; var l23 = _data.l23; var l24 = _data.l24;
            
            var diffuseConstant = l00 - l22;
            var quadraticDiffuseConstant = l22 * 3;
            return new SHL2ShaderConstants() {
                shAr = new float4(l12.x,l10.x,l11.x,diffuseConstant.x),
                shAg = new float4(l12.y,l10.y,l11.y,diffuseConstant.y),
                shAb = new float4(l12.z,l10.z,l11.z,diffuseConstant.z),
                shBr = new float4(l20.x,l21.x,quadraticDiffuseConstant.x,l23.x),
                shBg = new float4(l20.y,l21.y,quadraticDiffuseConstant.y,l23.y),
                shBb = new float4(l20.z,l21.z,quadraticDiffuseConstant.z,l23.z),
                shC = l24,
            };
        }
    }

    
    public class SHL2ShaderProperties
    {
        public readonly int kSHAr;
        public readonly int kSHAg;
        public readonly int kSHAb;
        public readonly int kSHBr;
        public readonly int kSHBg;
        public readonly int kSHBb;
        public readonly int kSHC;
        public SHL2ShaderProperties(string _prefix = "")
        {
            kSHAr = Shader.PropertyToID(_prefix+"_SHAr");
            kSHAg = Shader.PropertyToID(_prefix+"_SHAg");
            kSHAb = Shader.PropertyToID(_prefix+"_SHAb");
            kSHBr = Shader.PropertyToID(_prefix+"_SHBr");
            kSHBg = Shader.PropertyToID(_prefix+"_SHBg");
            kSHBb = Shader.PropertyToID(_prefix+"_SHBb");
            kSHC = Shader.PropertyToID(_prefix+"_SHC");
        }
        
        public static SHL2ShaderProperties kDefault = new SHL2ShaderProperties();
        public static SHL2ShaderProperties kUnity = new SHL2ShaderProperties("unity");
        
        public void Apply(MaterialPropertyBlock _block,SHL2ShaderConstants _output)
        {
            _block.SetVector(kSHAr, _output.shAr);
            _block.SetVector(kSHAg, _output.shAg);
            _block.SetVector(kSHAb, _output.shAb);
            _block.SetVector(kSHBr, _output.shBr);
            _block.SetVector(kSHBg, _output.shBg);
            _block.SetVector(kSHBb, _output.shBb);
            _block.SetVector(kSHC, _output.shC.to4());
        }

        public void ApplyGlobal(SHL2ShaderConstants _output)
        {
            Shader.SetGlobalVector(kSHAr, _output.shAr);
            Shader.SetGlobalVector(kSHAg, _output.shAg);
            Shader.SetGlobalVector(kSHAb, _output.shAb);
            Shader.SetGlobalVector(kSHBr, _output.shBr);
            Shader.SetGlobalVector(kSHBg, _output.shBg);
            Shader.SetGlobalVector(kSHBb, _output.shBb);
            Shader.SetGlobalVector(kSHC, _output.shC.to4());
        }

        public SHL2ShaderConstants FetchGlobal()
        {
            return new SHL2ShaderConstants()
            {
                shAr = Shader.GetGlobalVector(kSHAr),
                shAg = Shader.GetGlobalVector(kSHAg),
                shAb = Shader.GetGlobalVector(kSHAb),
                shBr = Shader.GetGlobalVector(kSHBr),
                shBg = Shader.GetGlobalVector(kSHBg),
                shBb = Shader.GetGlobalVector(kSHBb),
                shC = ((float4)Shader.GetGlobalVector(kSHC)).to3xyz(),
            };
        }
    }
}