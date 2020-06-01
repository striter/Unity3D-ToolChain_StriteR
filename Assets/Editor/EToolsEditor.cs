using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EToolsEditor
{
    public static class TMenuItem
    {
        [MenuItem("Work Flow/Selected Prefabs/Apply All",false,101)]
        static void ApplyAllPrefabs()
        {
            GameObject[] objects = Selection.gameObjects;
            for (int i = 0; i < objects.Length; i++)
            {
                UnityEngine.Object connectedPrefab = PrefabUtility.GetPrefabParent(objects[i]);
                if (connectedPrefab == null)
                    continue;

                PrefabUtility.ReplacePrefab(objects[i], connectedPrefab);
                Debug.Log("Prefab:" + connectedPrefab.name + " Replaced Successful!");
            }
        }
        [MenuItem("Work Flow/Take Screen Shot",false, 102)]
        static void TakeScreenShot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath+"/ScreenShots");
            string path = Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmss")));
            Debug.Log("Sceen Shots At " + path);
            ScreenCapture.CaptureScreenshot(path);
        }
    }

    public static class TUI
    {
        public class FontsHelpWindow : EditorWindow
        {
            [MenuItem("Work Flow/UI Tools/Fonts Help Window", false, 203)]
            public static void ShowWindow()
            {
                FontsHelpWindow window = GetWindow(typeof(FontsHelpWindow)) as FontsHelpWindow;
                window.Show();
            }

            UnityEngine.Object m_parent;
            Font m_Font;
            bool m_replaceMissing;
            private void OnEnable()
            {
                m_Font = null;
                m_parent = null;
                m_replaceMissing = false;
                EditorApplication.update += Update;
            }
            private void OnDisable()
            {
                EditorApplication.update -= Update;
            }
            private void Update()
            {
                if (EditorApplication.isPlaying)
                    return;

                if (m_parent != Selection.activeObject)
                {
                    UnityEngine.Object obj = Selection.activeObject;
                    m_parent = PrefabUtility.GetPrefabObject(obj) != null ? obj : null;
                    Repaint();
                    EditorUtility.SetDirty(this);
                }
            }
            private void OnGUI()
            {
                EditorGUILayout.BeginVertical();
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.TextArea("Available Only In EDITOR Mode!");
                    return;
                }
                if (m_parent == null)
                {
                    EditorGUILayout.TextArea("Please Select Texts Parent Which PREFAB CONNECTED!");
                    return;
                }

                Text[] m_text = m_parent ? (m_parent as GameObject).GetComponentsInChildren<Text>() : null;
                int count = 0;
                for (int i = 0; i < m_text.Length; i++)
                    if (m_text[i].font == null)
                        count++;

                EditorGUILayout.TextArea("Current Selecting:" + m_parent.name + ", Texts Counts:" + m_text.Length);
                EditorGUILayout.TextArea("Current Missing Count:" + count);
                m_Font = (Font)EditorGUILayout.ObjectField("Replace Font", m_Font, typeof(Font), false);
                m_replaceMissing = EditorGUILayout.Toggle("Replace Missing",m_replaceMissing);
                if (m_Font)
                {
                    if (m_Font && GUILayout.Button("Set " + (m_replaceMissing ? "Missing" : "All") + " Texts Font To:" + m_Font.name))
                    {
                        ReplaceFonts(m_Font,m_text,m_replaceMissing);
                        EditorUtility.SetDirty(m_parent);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            void ReplaceFonts(Font _font, Text[] _texts, bool replaceMissingOnly)
            {
                for (int i = 0; i < _texts.Length; i++)
                    if(!replaceMissingOnly||_texts[i].font==null)
                        _texts[i].font = m_Font;
            }
        }
    }

    public static class TEditor
    {
        public const string S_AssetDataBaseResources = "Assets/Resources/";

        public static Transform m_CurrentSelectingTransform => Selection.activeTransform;
    }


    public class EAudio
    {
        static AudioClip curClip;
        //Reflection Target  UnityEditor.AudioUtil;
        public static void AttachClipTo(AudioClip clip)
        {
            curClip = clip;
        }
        public static bool IsAudioPlaying()
        {
            if (curClip != null)
                return (bool)GetClipMethod("IsClipPlaying").Invoke(null, new object[] { curClip });
            return false;
        }
        public static int GetSampleDuration()
        {
            if(curClip!=null)
              return(int)GetClipMethod("GetSampleCount").Invoke(null, new object[] { curClip });
            return -1;
        }
        public static int GetCurSample()
        {
            if (curClip != null)
                return (int)GetClipMethod("GetClipSamplePosition").Invoke(null, new object[] { curClip });
            return -1;
        }
        public static float GetCurTime()
        {
            if (curClip != null)
                return (float)GetClipMethod("GetClipPosition").Invoke(null, new object[] { curClip});
            return -1;
        }
        public static void PlayClip()
        {
            if (curClip != null)
                GetClipMethod("PlayClip").Invoke(null, new object[] { curClip });
        }
        public static void PauseClip()
        {
            if (curClip != null)
                GetClipMethod("PauseClip").Invoke( null,  new object[] { curClip } );
        }
        public static void StopClip()
        {
            if(curClip!=null)
            GetClipMethod("StopClip").Invoke(null,  new object[] { curClip } );
        }
        public static void ResumeClip()
        {
            if (curClip != null)
                GetClipMethod("ResumeClip").Invoke(null, new object[] { curClip });
        }
        public static void SetSamplePosition(int startSample)
        {
            GetMethod<AudioClip, int>("SetClipSamplePosition").Invoke(null, new object[] { curClip, startSample });
        }
        static MethodInfo GetClipMethod(string methodName)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
           return  audioUtilClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
        }
        static MethodInfo GetMethod<T, U>(string methodName)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            return audioUtilClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(T), typeof(U) }, null);

        }
    }

}
