using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TEditor
{
    public static class Helper
    {
        public static void CleanupMaterialProperties()
        {
        
    
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
    }
}