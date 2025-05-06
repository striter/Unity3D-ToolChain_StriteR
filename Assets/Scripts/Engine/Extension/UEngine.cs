using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class UEngineIntergration
{
    static readonly List<Type> kSerializeBaseType = new() {
        typeof(bool), typeof(string),typeof(char),
        typeof(float), typeof(double),
        typeof(int), typeof(short), typeof(long),
        typeof(Vector3), typeof(Vector2), typeof(Vector4),
        typeof(RangeInt), typeof(RangeFloat),
        typeof(Texture2D), typeof(Texture3D), typeof(Mesh), typeof(AnimationClip),
        typeof(IntPtr),
    };
    
    public static IEnumerable<KeyValuePair<FieldInfo, Stack<FieldInfo>>> GetBaseTypeFieldStacks(this Type _type, BindingFlags _flags)
    {
        Stack<Queue<FieldInfo>> totalFields = new Stack<Queue<FieldInfo>>();
        Stack<FieldInfo> fieldStack = new Stack<FieldInfo>();

        foreach (var field in _type.GetAllFields(_flags))
            totalFields.Push(new Queue<FieldInfo>(new FieldInfo[] { field }));
        while (totalFields.Count > 0)
        {
            var curFieldStack = totalFields.Peek();
            if (curFieldStack.Count == 0)
            {
                totalFields.Pop();
                if (fieldStack.Count > 0)
                    fieldStack.Pop();
                continue;
            }

            FieldInfo shallowField = curFieldStack.Dequeue();
            fieldStack.Push(shallowField);
            bool isBaseType = shallowField.FieldType.IsEnum || shallowField.FieldType.IsArray || shallowField.FieldType.IsGenericType || kSerializeBaseType.Contains(shallowField.FieldType);
            if (!isBaseType && shallowField.FieldType.IsSerializable)
            {
                totalFields.Push(new Queue<FieldInfo>(shallowField.FieldType.GetAllFields(_flags)));
                continue;
            }
            yield return new KeyValuePair<FieldInfo, Stack<FieldInfo>>(shallowField, new Stack<FieldInfo>(fieldStack));
            fieldStack.Pop();
        }
    }
}
