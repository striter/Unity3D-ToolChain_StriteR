using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UReflection
{
    public static T CreateInstance<T>(Type t,params object[] constructorArgs) => (T)Activator.CreateInstance(t, constructorArgs);
    public static void TraversalAllInheritedClasses<T>(Action<Type> OnEachClass)
    {
        Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
        Type parentType = typeof(T);
        for (int i = 0; i < allTypes.Length; i++)
        {
            if (allTypes[i].IsClass && !allTypes[i].IsAbstract && allTypes[i].IsSubclassOf(parentType))
                OnEachClass(allTypes[i]);
        }
    }
    public static void TraversalAllInheritedClasses<T>(Action<Type, T> OnInstanceCreated, params object[] constructorArgs) => TraversalAllInheritedClasses<T>(type=> OnInstanceCreated(type, CreateInstance<T>(type, constructorArgs)));
    public static void InvokeAllMethod<T>(List<Type> classes,string methodName,T template) where T:class
    {
        foreach (Type t in classes)
        {
            MethodInfo method = t.GetMethod(methodName);
            if (method != null)
                method.Invoke(null,new object[] {template });
            else
                Debug.LogError("Null Method Found From:"+t.ToString()+"."+methodName);
        }
    }
    public static Stack<Type> GetInheritTypes(this Type _type)
    {
        if (_type == null)
            throw new NullReferenceException();

        Stack<Type> inheritStack = new Stack<Type>();
        while (_type.BaseType != null)
        {
            _type = _type.BaseType;
            inheritStack.Push(_type);
        }
        return inheritStack;
    }
    public static object GetValue(this Stack<FieldInfo> _fieldStacks, object _targetObject)
    {
        object dstObject = _targetObject;
        foreach(var field in _fieldStacks)
            dstObject = field.GetValue(dstObject);
        return dstObject;
    }
    public static void SetValue(this Stack<FieldInfo> _fieldStacks, object _targetObject, object _value)
    {
        Stack<object> dstObjects = new Stack<object>();
        object dstObject = _targetObject;
        int totalCount = _fieldStacks.Count;
        int fieldCount = totalCount;
        foreach (var field in _fieldStacks)
        {
            if (--fieldCount == 0)
                break;
            dstObject = field.GetValue(dstObject);
            dstObjects.Push(dstObject);
        }
        dstObject = _value;
        for (int i = totalCount-1; i >=0; i--)
        {
            FieldInfo field = _fieldStacks.ElementAt(i);
            object tarObject = dstObjects.Count==0?_targetObject:dstObjects.Pop();
            field.SetValue(tarObject, dstObject);
            dstObject = tarObject;
        }
    }

    static readonly List<Type> s_SerializeBaseType = new List<Type>() {
        typeof(bool), typeof(string),typeof(char),
        typeof(float),  typeof(double),
        typeof(int),typeof(short),typeof(long),
        typeof(Vector3), typeof(Vector2), typeof(Vector4),
        typeof(RangeInt), typeof(RangeFloat),
        typeof(Texture2D),typeof(Texture3D),typeof(Mesh),typeof(AnimationClip),
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
            bool isBaseType = shallowField.FieldType.IsEnum || shallowField.FieldType.IsArray || shallowField.FieldType.IsGenericType || s_SerializeBaseType.Contains(shallowField.FieldType);
            if (!isBaseType && shallowField.FieldType.IsSerializable)
            {
                totalFields.Push(new Queue<FieldInfo>(shallowField.FieldType.GetAllFields(_flags)));
                continue;
            }
            yield return new KeyValuePair<FieldInfo, Stack<FieldInfo>>(shallowField, new Stack<FieldInfo>(fieldStack));
            fieldStack.Pop();
        }
    }
    public static IEnumerable<FieldInfo> GetAllFields(this Type _type,BindingFlags _flags)
    {
        if (_type == null)
            throw new NullReferenceException();

        foreach (var fieldInfo in _type.GetFields(_flags | BindingFlags.Public|BindingFlags.NonPublic))
            yield return fieldInfo;
        var inheritStack = _type.GetInheritTypes();
        while(inheritStack.Count>0)
        {
            var type = inheritStack.Pop();
            foreach (var fieldInfo in type.GetFields(_flags | BindingFlags.NonPublic))
                if(fieldInfo.IsPrivate)
                    yield return fieldInfo;
        }
    }
    public static IEnumerable<MethodInfo> GetAllMethods(this Type _type, BindingFlags _flags)
    {
        if (_type == null)
            throw new NullReferenceException();

        foreach (var methodInfo in _type.GetMethods(_flags | BindingFlags.Public | BindingFlags.NonPublic))
            yield return methodInfo;
        var inheritStack = _type.GetInheritTypes();
        while (inheritStack.Count > 0)
        {
            var type = inheritStack.Pop();
            foreach (var methodInfo in type.GetMethods(_flags | BindingFlags.NonPublic))
                if (methodInfo.IsPrivate)
                    yield return methodInfo;
        }
    }

    public static class UI
    {
        public class CPropertyFillElement
        {
            public Transform transform { get; private set; }
            public CPropertyFillElement(Transform transform)
            {
                this.transform = transform;
            }
        }

        static readonly Type m_FillElement = typeof(CPropertyFillElement);
        static readonly Dictionary<Type, Func<Transform, object>> m_BaseTypeHelper = new Dictionary<Type, Func<Transform, object>>()
    {
        { typeof(Transform),(Transform transform)=>transform as Transform},
        { typeof(RectTransform),(Transform transform)=>transform as RectTransform},
        { typeof(Button),(Transform transform)=>transform.GetComponent<Button>() },
        { typeof(Text),(Transform transform)=>transform.GetComponent<Text>() },
        { typeof(InputField),(Transform transform)=>transform.GetComponent<InputField>() },
        { typeof(Image),(Transform transform)=>transform.GetComponent<Image>() },
        { typeof(RawImage),(Transform transform)=>transform.GetComponent<RawImage>() },
    };
        static bool FillTypeMatch(Type type) => m_BaseTypeHelper.ContainsKey(type) || type.IsSubclassOf(typeof(CPropertyFillElement));

        public static void UIPropertyFill<T>(T target,Transform parent) 
        {
            try
            {
                var properties = target.GetType().GetProperties(BindingFlags.DeclaredOnly| BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(p=>FillTypeMatch(p.PropertyType));
                var fieldInfos = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(p => FillTypeMatch(p.FieldType));

                object objValue = null;
                foreach (var property in properties)
                {
                    if (GetProperty(target,parent, property.Name, property.PropertyType, out objValue))
                        property.SetValue(target, objValue, null);
                }

                foreach (var filedInfo in fieldInfos)
                {
                    if (GetProperty(target,parent, filedInfo.Name, filedInfo.FieldType, out objValue))
                        filedInfo.SetValue(target, objValue);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error!Property Should Be Named Like: x_Xxxx_xxx! Transfered To Xxxx/xxx \n" + e.Message + "\n" + e.StackTrace);
            }
        } 

        static bool GetProperty<T>(T target,Transform parent, string name, Type type, out object obj)
        {
            obj = null;
            string[] propertySplitPath = name.Split('_');
            string path = "";
            for (int i = 1; i < propertySplitPath.Length; i++)
            {
                path += propertySplitPath[i];
                if (i < propertySplitPath.Length - 1)
                    path += "/";
            }
            Transform targetTrans = parent.Find(path);
            if (targetTrans == null)
                throw new Exception("Folder:" + path);
            if (type.IsSubclassOf(m_FillElement))
                obj =  Activator.CreateInstance(type, (object)targetTrans);
            else
                obj = m_BaseTypeHelper[type](targetTrans);
            return true;
        }
    }
}
