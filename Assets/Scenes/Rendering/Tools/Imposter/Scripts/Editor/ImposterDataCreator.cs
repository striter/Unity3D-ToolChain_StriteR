using System.Linq;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    public static class ImposterDataCreator
    {
        [MenuItem("Assets/Create/Optimize/Imposter/Data", false, 10)]
        public static void CreateWithSelections()
        {
            var selections = Selection.objects.ToArray();
            if (selections.Length == 0)
            {
                Debug.LogWarning("No ImposterData selected");
                return;
            }

            AssetSelectWindow.Select<ImposterConstructor>(p =>
            {
                if (p == null)
                    return;

                var successful = false;
                foreach(var obj in selections)
                {
                    if (!obj.IsPrefab())
                        continue;

                    var path = AssetDatabase.GetAssetPath(obj);
                    var filePath = path.AssetToFilePath();
                    var fileName = path.GetFileName().RemoveExtension();

                    var prefab = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                    p.Construct(prefab.transform,fileName,$"{filePath.GetPathDirectory()}/{fileName}_Imposter.asset");
                    
                    GameObject.DestroyImmediate(prefab);
                    successful = true;
                }
            
                if(!successful)
                    Debug.LogWarning("No ImposterData selected");
            });
            
        }
    }
}