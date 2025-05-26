using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Process
{
    public class FAnimationProcessClipOptimize : AAnimationProcess
    {
        [Range(1,8)] public int m_FloatingPointPrecision = 3;
        public bool m_RemoveStaticIdentityCurve = true;
        protected override bool PreProcess(ModelImporter _importer)
        {
            _importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            return true;
        }
        protected override bool Postprocess(ModelImporter _importer, AnimationClip _target)
        {
            var precisionRegex = $"f{m_FloatingPointPrecision}";
            var tolerance = 1f / (float)Math.Pow(10, m_FloatingPointPrecision);
            foreach (var theCurveBinding in AnimationUtility.GetCurveBindings(_target))
            {
                var curve = AnimationUtility.GetEditorCurve(_target, theCurveBinding);
                var staticIdentityCurve =
                    curve.keys.Length == 2 && curve.keys.All(k => math.abs(k.value - 1f) < tolerance && math.abs(k.inTangent) < tolerance && math.abs(k.outTangent) < tolerance);
                                          
                if (m_RemoveStaticIdentityCurve && staticIdentityCurve)
                    curve = null;
                else
                {
                    var keyFrames = curve.keys;

                    for( var j = 0; j < keyFrames.Length; ++j )
                    {
                        var key = curve.keys[j];
                        key.value = float.Parse(key.value.ToString(precisionRegex));
                        key.inTangent = float.Parse(key.inTangent.ToString(precisionRegex));
                        key.outTangent = float.Parse(key.outTangent.ToString(precisionRegex));
                        keyFrames[j] = key;
                    }

                    curve.keys = keyFrames;
                }
                AnimationUtility.SetEditorCurve(_target, theCurveBinding, curve);
            }
            return true;
        }
    }
}