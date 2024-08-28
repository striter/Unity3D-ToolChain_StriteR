using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace Runtime.Optimize.Imposter
{
    public static class ImposterDataCreator
    {
        [MenuItem("Assets/Create/Optimize/Imposter/Data", false, 10)]
        public static void CreateWithSelections()
        {
            var selections = Selection.objects.ToArray();
            if (selections.Length == 0)
            {
                Debug.LogWarning("Nothing selected");
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

        
        [MenuItem("GameObject/Optimize/Imposter/Data", false, 11)]
        public static void CreateWithSelection()
        {
            var selections = Selection.objects.ToArray();
            if (selections.Length != 1)
            {
                Debug.LogWarning("Nothing selected");
                return;
            }

            var selection = selections.First();
            
            AssetSelectWindow.Select<ImposterConstructor>(p =>
            {
                if (p == null)
                    return;

                if (!selection.IsSceneObject())
                {
                    Debug.LogWarning($"{selection} is not SceneObject");
                    return;
                }
        
                var name = UEPath.PathRegex($"{selection.name}_Imposter");
                p.Construct((selection as GameObject).transform,name,$"{GetDirectory()}/{name}.asset");
            });
        }
        
        private static string GetDirectory() => UEAsset.MakeSureDirectory(UEPath.PathRegex("<#activeScenePath>/Imposter"));
        [MenuItem("GameObject/Optimize/Imposter/ActiveSceneData", false, 11)]
        public static void CreateSceneSelections()
        {
            var selections = Selection.objects.ToArray();
            if (selections.Length == 0)
            {
                Debug.LogWarning("Nothing selected");
                return;
            }

            var directory = GetDirectory();
            var index = 0;
            AssetSelectWindow.Select<ImposterConstructor>(p => {
                UEAsset.BeginAssetDirty();
                foreach (var obj in selections)
                {
                    if (!obj.IsSceneObject())
                    {
                        Debug.LogWarning($"{obj} is not SceneObject");
                        continue;
                    }
                
                    var name = UEPath.PathRegex($"<#activeSceneName>_Imposter{index++}_{obj.name}");
                    p.Construct((obj as GameObject).transform,name,$"{directory}/{name}.asset");
                }
                
                UEAsset.DeleteAllAssetAtPath(directory.FileToAssetPath(),p=>!UEAsset.IsAssetDirty(p));
            });
        }

    }
}