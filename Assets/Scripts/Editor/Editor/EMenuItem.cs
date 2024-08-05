using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static partial class EMenuItem
    {
        //BuiltIn Texture Ref:https://unitylist.com/p/5c3/Unity-editor-icons
        //Art
        [MenuItem("Work Flow/Rendering/Procedural Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(EProceduralMeshGenerator)).titleContent =
            new GUIContent("Procedural Mesh Generator", EditorGUIUtility.IconContent("CustomTool").image);

        [MenuItem("Work Flow/Rendering/Mesh Smooth Normal Generator", false, 302)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(SmoothNormalGenerator)).titleContent =
            new GUIContent("Smooth Normal Generator", EditorGUIUtility.IconContent("CustomTool").image);

        [MenuItem("Work Flow/Rendering/Mesh Editor", false, 303)]
        static void ShowMeshEditor() => EditorWindow.GetWindow(typeof(MeshEditor)).titleContent =
            new GUIContent("Mesh Editor", EditorGUIUtility.IconContent("AvatarPivot").image);

        [MenuItem("Work Flow/Rendering/Texture Editor", false, 305)]
        static void ShowTextureModifier() => EditorWindow.GetWindow(typeof(TextureEditor.ETextureEditor)).titleContent =
            new GUIContent("Texture Editor", EditorGUIUtility.IconContent("d_PreTextureMipMapHigh").image);

        [MenuItem("Work Flow/Rendering/Noise Texture Generator", false, 304)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(EWNoiseTextureGenerator)).titleContent =
            new GUIContent("Noise Texture Generator", EditorGUIUtility.IconContent("CustomTool").image);

        [MenuItem("Work Flow/Rendering/(Optimize)Animation Clip Optimize", false, 401)]
        static void ShowAssetOptimizeWindow() => EditorWindow.GetWindow(typeof(AnimationClipOptimize)).titleContent =
            new GUIContent("Asset Optimize", EditorGUIUtility.IconContent("Toolbar Plus More").image);

        [MenuItem("Work Flow/Rendering_Test/Scriptable Objects Combiner", false, 601)]
        static void ShowScriptableObjectsCombinerWindow() =>
            EditorWindow.GetWindow(typeof(ScriptableObjectCombiner)).titleContent =
                new GUIContent("Scriptable Objects Combiner", EditorGUIUtility.IconContent("d_Import").image);
    }
}
