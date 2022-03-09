using System.Collections;
using System.Collections.Generic;
using System.IO;
using Rendering;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public static class Helper
    {
        public static void CleanupMaterialProperties()
        {
            //To Be Continued
        }

        public static void ExportLightmapCollection()
        {
            if (Selection.activeTransform==null)
                return;
            
            if (!UEAsset.SaveFilePath(out var filePath, "asset", "LightmapCollection_Default"))
                return;
            
            LightmapParameterCollection collection = Editor.CreateInstance<LightmapParameterCollection>();
            collection.ExportFromScene(Selection.activeTransform);
            UEAsset.CreateOrReplaceMainAsset(collection,UEPath.FileToAssetPath(filePath));
        }
        
        public static void CleanPersistentData()
        {
            if (!Directory.Exists(DRuntime.kDataPersistentPath))
                return;
            foreach (var filePath in Directory.GetFiles(DRuntime.kDataPersistentPath)) 
                File.Delete(filePath);   
            Directory.Delete( DRuntime.kDataPersistentPath);
            Debug.Log("Persistent Data Cleaned");
        }
        
        public static void SwitchDevelopMode()
        {
            bool internalDebug = !EditorPrefs.GetBool("DeveloperMode");
            EditorPrefs.SetBool("DeveloperMode",internalDebug);
            Debug.LogWarning("Editor Developer Mode Switch:"+(internalDebug?"ON":"OFF"));
        }
    }
}