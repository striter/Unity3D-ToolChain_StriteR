using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace Examples.Rendering.Imposter
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
                
                var renderer = new GameObject(imposterData.name).AddComponent<ImposterRenderer>();
                Undo.RegisterCreatedObjectUndo(renderer.gameObject, "Create Imposter");
                var transform = renderer.transform;
                if (dropRoot != null)
                    transform.SetParentAndSyncPositionRotation(dropRoot.transform);
                    
                renderer.meshConstructor.m_Data = imposterData;
                renderer.OnValidate();
                
                objects.Add(renderer.gameObject);
            }
            
            Selection.objects = objects.ToArray();
            return DragAndDropVisualMode.None;
        }
        
    }
    
}