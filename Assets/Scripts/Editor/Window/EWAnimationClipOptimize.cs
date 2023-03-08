using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace UnityEditor.Extensions
{
    public class AnimationClipOptimize : EditorWindow
    {
        AnimationClip m_OptimizeAsset;
        int m_OptimizePresicion=3;
        bool m_OptimizeScale=true;
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            ProceedGUI();
            EditorGUILayout.EndVertical();
        }

        void ProceedGUI()
        {
            EditorGUILayout.LabelField("Select Optimize AnimationClip Asset");
            m_OptimizeAsset = (AnimationClip)EditorGUILayout.ObjectField(m_OptimizeAsset, typeof(AnimationClip), false);

            if (!m_OptimizeAsset)
                return;

            m_OptimizeScale = EditorGUILayout.Toggle("Wipe Scale:",m_OptimizeScale);
            m_OptimizePresicion = EditorGUILayout.IntSlider("Float Presicion",m_OptimizePresicion, 2, 8);
            if (GUILayout.Button("Optimize"))
            {
                if (UEAsset.SaveFilePath(out string filePath, "anim", UEPath.RemoveExtension(UEPath.GetPathName(AssetDatabase.GetAssetPath(m_OptimizeAsset))) + "_O"))
                {
                    AnimationClip clip = OptimizeAnimation(m_OptimizeAsset, m_OptimizePresicion, m_OptimizeScale);
                    string assetPath = UEPath.FileToAssetPath(filePath);
                    AssetDatabase.CreateAsset(clip, assetPath);
                }
            }
        }
        public static AnimationClip OptimizeAnimation(AnimationClip _srcClip,int _floatPrecision,bool _clearScale)
        {
            AnimationClip _dstClip = _srcClip.Copy();
            _dstClip.ClearCurves();
            foreach(var binding in AnimationUtility.GetCurveBindings(_srcClip))
            {
                if(_clearScale&& binding.propertyName.Contains("Scale"))
                    continue;
                AnimationCurve curve= AnimationUtility.GetEditorCurve(_srcClip,binding);
                Keyframe[] keyframes = curve.keys;
                for(int i=0;i<keyframes.Length;i++)
                {
                    Keyframe keyframe = keyframes[i];
                    keyframe.value = OptimizeFloat(keyframe.value,_floatPrecision);
                    keyframe.time = OptimizeFloat(keyframe.time, _floatPrecision);
                    keyframe.inTangent = OptimizeFloat(keyframe.inTangent, _floatPrecision);
                    keyframe.outTangent = OptimizeFloat(keyframe.outTangent, _floatPrecision);
                    keyframe.inWeight = OptimizeFloat(keyframe.inWeight, _floatPrecision);
                    keyframe.outWeight = OptimizeFloat(keyframe.outWeight, _floatPrecision);
                    keyframes[i] = keyframe;
                }
                curve.keys = keyframes;
                _dstClip.SetCurve(binding.path, binding.type, binding.propertyName,curve);
            }
            return _dstClip;
        }
        static float OptimizeFloat(float _srcFloat, int _optimize)
        {
            float optimize = umath.pow(10, _optimize);
            return Mathf.Floor(_srcFloat *optimize)/optimize ;
        }
    }
}
