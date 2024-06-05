using System.IO;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static class Helper
    {
        
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