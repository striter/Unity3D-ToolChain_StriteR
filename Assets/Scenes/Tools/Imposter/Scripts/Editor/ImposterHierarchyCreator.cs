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

                var gameObject = new GameObject(imposterData.name);
                
                Undo.RegisterCreatedObjectUndo(gameObject, "Create Imposter");
                var transform = gameObject.transform;
                if (dropRoot != null)
                {
                    transform.SetParentAndSyncPositionRotation(dropRoot.transform);
                    transform.rotation = Quaternion.identity;   
                }
                if (imposterData.m_Instanced)
                {
                    gameObject.AddComponent<MeshRenderer>().sharedMaterial = imposterData.m_Material;
                    gameObject.AddComponent<MeshFilter>().sharedMesh = imposterData.m_Mesh;
                }
                else
                {
                    var renderer = gameObject.AddComponent<ImposterRenderer>();
                    renderer.meshConstructor.m_Data = imposterData;
                    renderer.OnValidate();
                }
                
                objects.Add(gameObject);
            }
            
            Selection.objects = objects.ToArray();
            return DragAndDropVisualMode.None;
        }
        
    }
    
}