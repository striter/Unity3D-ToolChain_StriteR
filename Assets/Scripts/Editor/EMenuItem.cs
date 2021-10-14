using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public static class EMenuItem
    {
        #region Hotkeys
        [MenuItem("Work Flow/Hotkeys/Selected Object Sync Scene View &F", false, 101)]
        public static void SyncObjectToSceneView() => Hotkeys.SyncSelectedToSceneViewCamera();
        [MenuItem("Work Flow/Hotkeys/Scene View Camera Sync To Selected _F6", false, 102)]
        public static void SceneViewCameraSyncSelected() => Hotkeys.SceneViewCameraSyncSelected();
        [MenuItem("Work Flow/Hotkeys/Fast Pause _F10", false, 103)]
        static void FastPause() => Hotkeys.SwitchPause();
        [MenuItem("Work Flow/Hotkeys/Take Screen Shot _F12", false, 105)]
        static void TakeScreenShot() => Hotkeys.TakeScreenShot();
        #endregion

        #region Helpers
        [MenuItem("Work Flow/Helper/Clean Persistent Data",false,200)]
        static void CleanPersistentData() => Helper.CleanPersistentData();
        
        [MenuItem("Work Flow/Hotkeys/Switch Developer Mode _F11", false, 104)]
        static void SwitchDevelopMode() => Helper.SwitchDevelopMode();
        [MenuItem("Work Flow/Helper/UI/Missing Fonts Replacer", false, 210)]
        static void ShowFontsReplacerWindow() => EditorWindow.GetWindow<UIFontsMissingReplacerWindow>().titleContent=new GUIContent("Missing Fonts Replacer",EditorGUIUtility.IconContent("FilterByLabel").image);
        #endregion
        
        #region Window
        //BuiltIn Texture Ref:https://unitylist.com/p/5c3/Unity-editor-icons
        //Art
        [MenuItem("Work Flow/Art/Plane Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(PlaneMeshGenerator)).titleContent=new GUIContent("Plane Generator", EditorGUIUtility.IconContent("CustomTool").image);
        [MenuItem("Work Flow/Art/Mesh Smooth Normal Generator", false, 302)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(SmoothNormalGenerator)).titleContent = new GUIContent("Smooth Normal Generator", EditorGUIUtility.IconContent("CustomTool").image);
        [MenuItem("Work Flow/Art/Mesh Editor", false, 303)]
        static void ShowMeshEditor() => EditorWindow.GetWindow(typeof(MeshEditor)).titleContent = new GUIContent("Mesh Editor",EditorGUIUtility.IconContent("AvatarPivot").image);
        [MenuItem("Work Flow/Art/Noise Texture Generator", false, 304)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(NoiseGenerator)).titleContent = new GUIContent("Noise Texture Generator", EditorGUIUtility.IconContent("CustomTool").image);
        [MenuItem("Work Flow/Art/Texture Modifier", false, 305)]
        static void ShowTextureModifier() => EditorWindow.GetWindow(typeof(TextureEditor)).titleContent = new GUIContent("Texture Editor", EditorGUIUtility.IconContent("d_PreTextureMipMapHigh").image);
        [MenuItem("Work Flow/Art/(Optimize)GPU Animation Baker", false, 400)]
        static void ShowOptimizeWindow() => EditorWindow.GetWindow(typeof(GPUAnimationBaker)).titleContent = new GUIContent("GPU Animation Instance Baker", EditorGUIUtility.IconContent("AvatarSelector").image);
        [MenuItem("Work Flow/Art/(Optimize)Animation Clip Optimize", false, 401)]
        static void ShowAssetOptimizeWindow() => EditorWindow.GetWindow(typeof(AnimationClipOptimize)).titleContent = new GUIContent("Asset Optimize", EditorGUIUtility.IconContent("Toolbar Plus More").image);
        #endregion
        
        #region MenuItems

        [MenuItem("Work Flow/Prefab/Remove All Colliders Child Of Selected",false,304)]
        static void RemoveAllSelectedColliders()
        {
            foreach (Transform transform in Selection.transforms)
            {
                foreach (Collider collider in transform.GetComponents<Collider>())
                    Object.DestroyImmediate(collider);
            }
        }
        #endregion
    }

}