using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static class Event_Extend
    {
        public static Vector2 getRealMousePosition(this Event _event) => Event.current.mousePosition*EditorGUIUtility.pixelsPerPoint;
    }
    
}