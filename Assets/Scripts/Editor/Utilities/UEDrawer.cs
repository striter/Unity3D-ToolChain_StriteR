using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEditor.Extensions
{
    public class UEDrawer
    {
        private static Dictionary<Type, Type> kCustomDrawerTypes = new();
        private static readonly Type kPropertyDrawerType = typeof(PropertyDrawer);
        public static Type FindCustomDrawerType(Type sourceType)
        {
            if (kCustomDrawerTypes.TryGetValue(sourceType, out var targetDrawerType))
                return targetDrawerType;

            if (sourceType.IsSubclassOf(typeof(Attribute)))
            {
                targetDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor").GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { sourceType });
            }
            else
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!kPropertyDrawerType.IsAssignableFrom(type)) 
                            continue;
                        foreach (var customPropertyDrawerAttribute in type.GetCustomAttributes<CustomPropertyDrawer>())
                        {
                            var customDrawerType = UDebug.GetFieldValue(customPropertyDrawerAttribute, "m_Type") as Type;
                            var useForChildren = ((bool?)UDebug.GetFieldValue(customPropertyDrawerAttribute, "m_UseForChildren")).Value;
                            if (customDrawerType == sourceType || (useForChildren && customDrawerType.IsAssignableFrom(sourceType)))
                                targetDrawerType = type;
                        }
                    }
                }
            }

            kCustomDrawerTypes.Add(sourceType, targetDrawerType);
            return null;
        }

        public static PropertyDrawer CreateDrawer(Type drawerType, FieldInfo fieldInfo, Attribute attribute)
        {
            var drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
            kPropertyDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(drawer, fieldInfo);
            kPropertyDrawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(drawer, attribute);
            return drawer;
        }
    }
}