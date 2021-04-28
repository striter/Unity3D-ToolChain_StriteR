using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TEditor
{
    public static class UECommon
    {
        public static Vector2 GetScreenPoint(this SceneView _sceneView)
        {
            Vector2 screenPoint = Event.current.mousePosition;
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

    }
}
