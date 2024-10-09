using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEditor.Rendering;
using UnityEngine;

namespace Runtime.Optimize.Imposter
{
    [InitializeOnLoad]
    public class ImposterHierarchyCreator : Editor
    {
        static ImposterHierarchyCreator () {
            DragAndDrop.AddDropHandler(DropHandler);
        }

        
        static DragAndDropVisualMode DropHandler(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            var dropRoot = EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;

            var objects = new List<GameObject>();
            foreach (var objectRef in DragAndDrop.objectReferences)
            {
                if (objectRef is not ImposterData imposterData) 
                    continue;
                
                if (!perform) 
                    return DragAndDropVisualMode.Link;

                var gameObject = ImposterData.CreateImposterRenderer(imposterData,dropRoot != null ? dropRoot.transform : null);
                Undo.RegisterCreatedObjectUndo(gameObject, "Create Imposter");
                objects.Add(gameObject);
            }
            
            Selection.objects = objects.ToArray();
            return DragAndDropVisualMode.None;
        }
        
    }
    
}