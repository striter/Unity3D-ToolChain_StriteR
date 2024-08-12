using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace UnityEditor.Extensions
{
    public static class UECommon
    {
        public static Vector2 GetScreenPoint(this SceneView _sceneView)
        {
            Vector2 screenPoint = Event.current.getRealMousePosition();
            screenPoint.y = _sceneView.camera.pixelHeight - screenPoint.y;
            return screenPoint;
        }
        public static Dictionary<int,string> GetAllLayers(bool emptyInclusive)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!emptyInclusive&&layerName == string.Empty)
                    continue;
                dic.Add(i, layerName);
            }
            return dic;
        }

        public static void UndoDestroyChildren(this Transform _transform)
        {
            int count = _transform.childCount;
            if (count <= 0)
                return;
            Transform[] transforms = new Transform[count];
            for (int i = 0; i < count; i++)
                transforms[i] = _transform.GetChild(i);
        
            foreach (var transform in transforms)
            {
                if(transform==_transform)
                    continue;
                Undo.DestroyObjectImmediate(transform.gameObject);;
            }
        }
        public static void DestroyChildrenComponent<T>(this Transform _transform) where T:Component
        {
            foreach (var component in _transform.GetComponentsInChildren<T>())
                Object.DestroyImmediate(component);
        }
        
        public static bool IsSceneObject(this Object obj)
        {
            if (obj == null)
                return false;

            var isSceneType = obj is GameObject or Component;
            if (!isSceneType)
                return false;

            return !PrefabUtility.IsPartOfPrefabAsset(obj);
        }

        public static bool IsPrefab(this Object obj)
        {
            return obj != null && PrefabUtility.IsPartOfPrefabAsset(obj);
        }
        public static bool IsCalledFromEditor()
        {
            foreach (var frame in new StackTrace().GetFrames()!)
            {
                if (frame.GetMethod().DeclaringType == typeof(UnityEditor.Editor))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
}
