using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TEditor
{
    #region Inspector
    [CustomEditor(typeof(UIComponentBase), true), CanEditMultipleObjects]
    public class EUIComponent : Editor
    {
        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying && GUILayout.Button("Test Init"))
                (target as UIComponentBase).SendMessage("Init");
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(UIT_TextExtend)), CanEditMultipleObjects]
    public class EUITextExtend : UnityEditor.UI.TextEditor
    {
        [MenuItem("GameObject/UI/TextExtend")]
        public static void CreateTextExtend()
        {
            GameObject go = Selection.activeGameObject;
            GameObject textExtend = new GameObject("Text_Extended");

            if (go != null)
                textExtend.transform.SetParent(go.transform);
            UIT_TextExtend extend = textExtend.AddComponent<UIT_TextExtend>();
            extend.text = "New Text Extend";
            extend.color = Color.black;
            extend.rectTransform.anchoredPosition = Vector2.zero;
        }

        UIT_TextExtend m_target = null;
        string targetLocalize;
        protected override void OnEnable()
        {
            base.OnEnable();
            m_target = target as UIT_TextExtend;
            TLocalization.SetRegion(enum_Option_LanguageRegion.CN);
            targetLocalize = m_target.m_LocalizeKey;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            //Will Do Localize?
            GUILayout.Label("Auto Localize:", GUILayout.Width(Screen.width / 3 - 20));
            bool autoLocalize = EditorGUILayout.Toggle(m_target.B_AutoLocalize, GUILayout.Width(Screen.width * 2 / 3 - 20)); ;
            if (autoLocalize != m_target.B_AutoLocalize)
            {
                Undo.RecordObject(m_target, "Text Extend Undo");
                m_target.B_AutoLocalize = autoLocalize;
            }
            EditorGUILayout.EndHorizontal();

            // Do Localize
            if (m_target.B_AutoLocalize)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Localize Key:", GUILayout.Width(Screen.width / 3 - 20));
                targetLocalize = EditorGUILayout.TextField(targetLocalize, GUILayout.Width(Screen.width * 2 / 3 - 20));

                string localizeKey = null;
                bool localized = true;
                bool keyLocalized = false;
                if (TLocalization.CheckKeyLocalizable(targetLocalize))
                {
                    localizeKey = targetLocalize;
                    keyLocalized = true;
                }
                else if (TLocalization.CheckValueLocalized(targetLocalize))
                {
                    localizeKey = TLocalization.FindLocalizeKey(targetLocalize);
                    keyLocalized = false;
                }
                else
                {
                    localized = false;
                }

                if (localizeKey != m_target.m_LocalizeKey)
                {
                    Undo.RecordObject(m_target, "Text Extend Undo");
                    m_target.m_LocalizeKey = localizeKey;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (localized)
                {
                    if (keyLocalized)
                    {
                        GUILayout.Label("Localized Value:", GUILayout.Width(Screen.width / 3 - 20));
                        GUILayout.Label(TLocalization.GetLocalizeValue(m_target.m_LocalizeKey), GUILayout.Width(Screen.width * 2 / 3 - 20));
                    }
                    else
                    {
                        GUILayout.Label("Localized Key:", GUILayout.Width(Screen.width / 3 - 20));
                        GUILayout.Label(m_target.m_LocalizeKey);
                    }
                }
                else
                {
                    GUILayout.Label("Unable To Localize");
                }
                GUILayout.EndHorizontal();
            }

            //Character Spacing
            EditorGUILayout.BeginHorizontal();
            int spacing = EditorGUILayout.IntField("Character Spacing:", m_target.m_CharacterSpacing);
            if (spacing != m_target.m_CharacterSpacing)
            {
                Undo.RecordObject(m_target, "Text Extend Undo");
                m_target.m_CharacterSpacing = spacing;
                m_target.SetAllDirty();
            }
            EditorGUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
    #endregion
    #region Window
    public class EUIFontsMissingReplacerWindow : EditorWindow
    {
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
                m_parent = PrefabUtility.GetPrefabInstanceHandle(obj) != null ? obj : null;
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
            m_replaceMissing = EditorGUILayout.Toggle("Replace Missing", m_replaceMissing);
            if (m_Font)
            {
                if (m_Font && GUILayout.Button("Set " + (m_replaceMissing ? "Missing" : "All") + " Texts Font To:" + m_Font.name))
                {
                    ReplaceFonts(m_Font, m_text, m_replaceMissing);
                    EditorUtility.SetDirty(m_parent);
                }
            }
            EditorGUILayout.EndVertical();
        }

        void ReplaceFonts(Font _font, Text[] _texts, bool replaceMissingOnly)
        {
            for (int i = 0; i < _texts.Length; i++)
                if (!replaceMissingOnly || _texts[i].font == null)
                    _texts[i].font = m_Font;
        }
    }
    #endregion
}