using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using System.Linq.Extensions;
using System.Reflection;

namespace CameraController.Inputs
{
    public abstract class AControllerInput
    {
        public bool Available => Camera !=null && Camera.enabled  && Camera.gameObject.activeInHierarchy
                                 && Anchor != null && Anchor.gameObject.activeInHierarchy;
        
        private static readonly string kAnchor = "Anchor";
        private static readonly string kEuler = "Euler";
        private static readonly string kPitch = "Pitch";
        private static readonly string kYaw = "Yaw";
        private static readonly string kRoll = "Roll";
        private static readonly string kViewPortX = "ViewPortX";
        private static readonly string kViewPortY = "ViewPortY";
        private static readonly string kDistance = "Distance";
        private static readonly string kFOV = "FOV";
        
        private Dictionary<string, Type> kPropertyTypes = new() {
            { kAnchor, typeof(float3) },
            { kEuler, typeof(float3) }, { kPitch, typeof(float) }, {kYaw, typeof(float) },{kRoll,typeof(float)},
            { kViewPortX,typeof(float) }, { kViewPortY,typeof(float)},
            { kDistance,typeof(float) }, { kFOV ,typeof(float)},
        };
        
        private readonly MethodInfo[] m_ClearMethods;
        private Dictionary<string, List<PropertyInfo>> m_ValueProperties = new();
        private IEnumerable<T> GetProperty<T>(string _key) where T : struct {
            var properties = m_ValueProperties[_key];
            foreach (var property in properties)
                yield return (T)property.GetValue(this, null);
        }
        
        protected AControllerInput() {
            var interfaces = GetType().GetInterfaces();
            m_ClearMethods = interfaces.Select(p => p.GetMethods().Find(m=>m.Name.Contains("Clear"))).Collect(p=>p!=null).ToArray();

            foreach (var type in kPropertyTypes.Keys)
               m_ValueProperties.Add(type,new List<PropertyInfo>()); 
            
            foreach (var type in interfaces)
            {
                foreach (var property in type.GetProperties())
                {
                    var anyMatches = kPropertyTypes.Keys.Find(p => property.Name.Contains(p));
                    if (anyMatches == null) continue;
                    var propertyType = kPropertyTypes[anyMatches];
                    if(property.PropertyType != propertyType)
                        throw new Exception($"{type.Name} Property type mismatch: {property.Name} | {property.PropertyType} != {propertyType}");
                    m_ValueProperties[anyMatches].Add(property);
                }
            }
        }
        
        public void Clear()
        {
            foreach (var clear in m_ClearMethods) 
                clear.Invoke(this, null);
        }
        
        public abstract Camera Camera { get; }
        public abstract Transform Anchor { get; }
        public abstract Transform Target { get;}

        public float3 InputEuler => new float3(GetProperty<float>(kPitch).Sum(), GetProperty<float>(kYaw).Sum(),GetProperty<float>(kRoll).Sum())  
                                    + GetProperty<float3>(kEuler).Sum().value;
        public float InputPinch => (this is IPlayerInput touchMixin) ? touchMixin.Pinch : 0;
        public float InputFOV => GetProperty<float>(kFOV).Sum();
        public float3 InputAnchorOffset => GetProperty<float3>(kAnchor).Sum().value;
        public float InputDistance => GetProperty<float>(kDistance).Sum();
        public float2 InputViewPort => new float2(GetProperty<float>(kViewPortX).Sum(), GetProperty<float>(kViewPortY).Sum());
    }

    public static class IControllerInput_Extension
    {
        public static void DrawGizmos(this AControllerInput _input)
        {
            if (!_input.Available) return;
            Gizmos.matrix = _input.Anchor.transform.localToWorldMatrix;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, Vector3.right);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }
    }
}