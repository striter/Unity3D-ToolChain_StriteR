using System.Collections.Generic;
using System.IO;
using Render.Debug;
using UnityEditor;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static partial class EMenuItem
    {
        #region Hotkeys
        [MenuItem("Work Flow/Hotkeys/Selected Object Sync Scene View &F", false, 101)]
        public static void Hotkey101() => Hotkeys.SyncSelectedToSceneViewCamera();
        
        [MenuItem("Work Flow/Hotkeys/Take Screen Shot _F12", false, 103)]
        static void Hotkey103() => Hotkeys.TakeScreenShot();
        
        [MenuItem("Work Flow/Hotkeys/Output Window Asset Path _&-", false, 110)]
        static void Hotkey110() => Hotkeys.OutputActiveWindowDirectory();
        
        [MenuItem("Work Flow/Hotkeys/Output Select Asset Path _&=", false, 111)]
        static void Hotkey111() => Hotkeys.OutputAssetDirectory();
        
        
        [MenuItem("Work Flow/Hotkeys/SyncSelectedTransform", false, 121)]
        static void Hotkey121() => Hotkeys.SortTransformBySize();
    #endregion

        #region Helpers
        [MenuItem("Work Flow/Helper/Clean Persistent Data",false,200)]
        static void CleanPersistentData() => Helper.CleanPersistentData();
        
        [MenuItem("Work Flow/Helper/Assets Rename",false,200)]
        static void AssetsRenameWindow() => EditorWindow.GetWindow<EAssetsBatchRename>().titleContent=new GUIContent("Assets Rename",EditorGUIUtility.IconContent("FilterByLabel").image);
        
        [MenuItem("Work Flow/Hotkeys/Switch Developer Mode _F11", false, 104)]
        static void SwitchDevelopMode() => Helper.SwitchDevelopMode();
        [MenuItem("Work Flow/Helper/UI/Missing Fonts Replacer", false, 210)]
        static void ShowFontsReplacerWindow() => EditorWindow.GetWindow<UIFontsMissingReplacerWindow>().titleContent=new GUIContent("Missing Fonts Replacer",EditorGUIUtility.IconContent("FilterByLabel").image);
        
        #endregion
    
        #region MenuItems

        [MenuItem("Work Flow/Prefab/Remove All Colliders Child Of Selected",false,304)]
        static void RemoveAllSelectedColliders()
        {
            foreach (Transform transform in Selection.transforms)
                transform.DestroyChildrenComponent<Collider>();
        }


        [MenuItem("Work Flow/Prefab/Extract Materials To New Folder", false, 305)]
        static void ExtractMaterialsToFolder()
        {
            if (Application.isPlaying)
                return;
            
            var selection = Selection.activeTransform;
            if (selection == null)
                return;
            
            if(!UEAsset.SelectDirectory(out var folderPath))
                return;
            
            Dictionary<MeshRenderer, Material[]> materials = new Dictionary<MeshRenderer, Material[]>();
            foreach (var renderer in selection.GetComponentsInChildren<MeshRenderer>(true))
                materials.Add(renderer,renderer.sharedMaterials);

            foreach (var (k, v) in materials)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    var material = v[i];
                    var path = AssetDatabase.GetAssetPath(material);
                    if (path == null)
                        continue;
                    var fileName = Path.GetFileName(path);
                    var newPath = Path.Combine(folderPath, fileName);
                    v[i] = UEAsset.CreateOrReplaceMainAsset(new Material(material){name = fileName.RemoveExtension()},newPath,false);
                }
            }

            foreach (var (k,v) in materials)
                k.sharedMaterials = v;
        }
        #endregion
        
        #region Windows
        //BuiltIn Texture Ref:https://github.com/halak/unity-editor-icons
        //Art
        [MenuItem("Work Flow/Asset/Procedural Mesh Generator", false, 301)]
        static void ShowPlaneGenerator() => EditorWindow.GetWindow(typeof(EProceduralMeshGenerator)).titleContent = new GUIContent("Procedural Mesh Generator", EditorGUIUtility.IconContent("CustomTool").image);

        [MenuItem("Work Flow/Asset/Mesh Smooth Normal Generator", false, 302)]
        static void ShowSmoothNormalGenerator() => EditorWindow.GetWindow(typeof(SmoothNormalGenerator)).titleContent = new GUIContent("Smooth Normal Generator", EditorGUIUtility.IconContent("CustomTool").image);

        [MenuItem("Work Flow/Asset/Mesh Editor", false, 303)]
        static void ShowMeshEditor() => EditorWindow.GetWindow(typeof(MeshEditor)).titleContent = new GUIContent("Mesh Editor", EditorGUIUtility.IconContent("AvatarPivot").image);

        [MenuItem("Work Flow/Asset/Texture Editor", false, 305)]
        static void ShowTextureModifier() => EditorWindow.GetWindow(typeof(TextureEditor.ETextureEditor)).titleContent = new GUIContent("Texture Editor", EditorGUIUtility.IconContent("d_PreTextureMipMapHigh").image);

        [MenuItem("Work Flow/Asset/Noise Texture Generator", false, 304)]
        static void ShowNoiseGenerator() => EditorWindow.GetWindow(typeof(EWNoiseTextureGenerator)).titleContent = new GUIContent("Noise Texture Generator", EditorGUIUtility.IconContent("CustomTool").image);

        [MenuItem("Work Flow/Asset/(Optimize)Animation Clip Optimize", false, 401)]
        static void ShowAssetOptimizeWindow() => EditorWindow.GetWindow(typeof(AnimationClipOptimize)).titleContent = new GUIContent("Asset Optimize", EditorGUIUtility.IconContent("Toolbar Plus More").image);

        [MenuItem("Work Flow/Asset/Scriptable Objects Combiner", false, 601)]
        static void ShowScriptableObjectsCombinerWindow() => EditorWindow.GetWindow(typeof(ScriptableObjectCombiner)).titleContent = new GUIContent("Scriptable Objects Combiner", EditorGUIUtility.IconContent("d_Import").image);

        [MenuItem("Work Flow/Render/OverdrawDebugger", false, 501)]
        static void ShowOverdrawDebugger() => AssetSelectWindow.Select<OverdrawProfilerData>(OverdrawProfiler.Switch);
        #endregion
    }

}