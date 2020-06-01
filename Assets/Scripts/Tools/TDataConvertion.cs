using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;


[Serializable]
public struct RangeFloat
{
    public float start;
    public float length;
    public float end => start + length;
    public RangeFloat(float _start, float _length)
    {
        start = _start;
        length = _length;
    }
}
[Serializable]
public struct RangeInt
{

    public int start;
    public int length;
    public int end => start + length;
    public RangeInt(int _start, int _length)
    {
        start = _start;
        length = _length;
    }
}

#region DataPhrase
public interface IDataConvert
{
}
public static class TDataConvert
{
    static readonly char[] m_PhraseLiterateBreakPoints = new char[8] { '-', '[', ']', '{', '}', '(', ')', '/' };
    const char m_PhraseBaseBreakPoint = '|';

    public static string Convert(object value) => ConvertToString(value.GetType(), value, -1);
    public static T Convert<T>(string xmlData) => (T)ConvertToObject(typeof(T), xmlData, -1);
    public static object Convert(Type type, string xmlData) => ConvertToObject(type, xmlData, -1);
    public static object Default(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    static string ConvertToString( Type type, object value, int iteration)
    {
        if (type.IsEnum)
            return value.ToString();

        if (m_BaseTypeToXmlData.ContainsKey(type))
            return m_BaseTypeToXmlData[type](value);

        if (CheckIXmlParseType(type))
            return IXmlPhraseToString(type, value, iteration + 1);

        Type genericDefinition = type.GetGenericTypeDefinition();
        if (CheckGenericPhrase(genericDefinition))
            return GenericPhraseToString(type,genericDefinition, value, iteration + 1);

        Debug.LogError("Xml Error Invlid Type:" + type.ToString() + " For Base Type To Phrase");
        return null;
    }
    static object ConvertToObject(Type type, string xmlData, int iteration)
    {
        if (type.IsEnum)
            return Enum.Parse(type, xmlData);

        if (m_BaseTypeToObject.ContainsKey(type))
            return m_BaseTypeToObject[type](xmlData);

        if (CheckIXmlParseType(type))
            return IXmlPraseToData(type, xmlData, iteration + 1);

        Type genericDefinition = type.GetGenericTypeDefinition();
        if (CheckGenericPhrase(genericDefinition))
            return GenericPhraseToData(type,genericDefinition, xmlData, iteration + 1);

        Debug.LogError("Xml Error Invlid Type:" + type.ToString() + " For Xml Data To Phrase");
        return null;
    }

    #region BaseType
    static Dictionary<Type, Func<object, string>> m_BaseTypeToXmlData = new Dictionary<Type, Func<object, string>>() {
        { typeof(int), (object target) => { return target.ToString(); }},
        { typeof(long), (object target) => { return target.ToString(); } },
        { typeof(double), (object target) => { return target.ToString(); }},
        { typeof(float), (object target) => { return target.ToString(); }},
        { typeof(string), (object target) => { return target as string; }},
        {typeof(bool), (object data) => { return (((bool)data ? 1 : 0)).ToString(); }},
        { typeof(RangeInt),(object data) => { return ((RangeInt)data).start.ToString() + m_PhraseBaseBreakPoint + ((RangeInt)data).length.ToString(); } },
        { typeof(RangeFloat), (object data) => { return ((RangeFloat)data).start.ToString() + m_PhraseBaseBreakPoint + ((RangeFloat)data).length.ToString(); }}
    };
    static Dictionary<Type, Func<string, object>> m_BaseTypeToObject = new Dictionary<Type, Func<string, object>>()
    {
        { typeof(int), (string xmlData) => { return int.Parse(xmlData); }},
        { typeof(long), (string xmlData) => { return long.Parse(xmlData); } },
        { typeof(double), (string xmlData) => { return double.Parse(xmlData); }},
        { typeof(float), (string xmlData) => { return float.Parse(xmlData); } },
        { typeof(string), (string xmlData) => { return xmlData; }},
        { typeof(bool), (string xmlData) => { return int.Parse(xmlData) == 1; } },
        { typeof(RangeInt), (string xmlData) => { string[] split = xmlData.Split(m_PhraseBaseBreakPoint); return new RangeInt(int.Parse(split[0]), int.Parse(split[1])); }},
        { typeof(RangeFloat), (string xmlData) => { string[] split = xmlData.Split(m_PhraseBaseBreakPoint); return new RangeFloat(float.Parse(split[0]), float.Parse(split[1])); }},
    };
    #endregion
    #region GenericType
    static Type m_GenericDicType = typeof(Dictionary<,>);
    static Type m_GenericListType = typeof(List<>);
    static bool CheckGenericPhrase(Type genericDefinition)=> genericDefinition ==m_GenericDicType  || genericDefinition == m_GenericListType;
    static string GenericPhraseToString( Type type,Type genericDefinition, object data, int iteration)
    {
        if (iteration >= m_PhraseLiterateBreakPoints.Length)
        {
            Debug.LogError("Iteration Max Reached!");
            return "";
        }
        char dataBreak = m_PhraseLiterateBreakPoints[iteration];
        StringBuilder _convertData = new StringBuilder();
        if (genericDefinition== m_GenericListType)
        {
            Type listType = type.GetGenericArguments()[0];
            foreach (object obj in data as IEnumerable)
            {
                _convertData.Append(ConvertToString(listType, obj, iteration + 1));
                _convertData.Append(dataBreak);
            }
            if (_convertData.Length != 0)
                _convertData.Remove(_convertData.Length - 1, 1);
        }
        else if(genericDefinition==m_GenericDicType)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];
            foreach (DictionaryEntry obj in (IDictionary)data)
            {
                _convertData.Append(ConvertToString(keyType, obj.Key,iteration+1));
                _convertData.Append(dataBreak);
                _convertData.Append(ConvertToString(valueType, obj.Value,iteration+1));
                _convertData.Append(dataBreak);
            }
            if (_convertData.Length != 0)
                _convertData.Remove(_convertData.Length - 1, 1);
        }
        return _convertData.ToString();
    }

    static object GenericPhraseToData(Type type,Type genericDefinition, string xmlData, int iteration)
    {
        if (iteration >= m_PhraseLiterateBreakPoints.Length)
        {
            Debug.LogError("Iteration Max Reached!");
            return null;
        }
        char dataBreak = m_PhraseLiterateBreakPoints[iteration];
        if(genericDefinition==m_GenericListType)
        {
            Type listType = type.GetGenericArguments()[0];
            IList iList_Target = (IList)Activator.CreateInstance(type);
            string[] list_Split = xmlData.Split(dataBreak);
            if (list_Split.Length != 1 || list_Split[0] != "")
                for (int i = 0; i < list_Split.Length; i++)
                    iList_Target.Add(ConvertToObject(listType, list_Split[i], iteration + 1));
            return iList_Target;
        }
        else if(genericDefinition==m_GenericDicType)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];
            IDictionary iDic_Target = (IDictionary)Activator.CreateInstance(type);
            string[] as_split = xmlData.Split(dataBreak);
            if (as_split.Length != 1 || as_split[0] != "")
                for (int i = 0; i < as_split.Length; i+=2)
                {
                    iDic_Target.Add(ConvertToObject(keyType, as_split[i], iteration + 1)
                        , ConvertToObject(valueType, as_split[i+1], iteration + 1));
                }
            return iDic_Target;
        }
        Debug.LogError("Invalid GenericDefinition here!");
        return null;
    }
    #endregion
    #region IXmlConvertType
    static readonly Type m_XmlPhraseType = typeof(IDataConvert);
    static Dictionary<Type, FieldInfo[]> m_XmlConvertFieldInfos = new Dictionary<Type, FieldInfo[]>();
    static bool CheckIXmlParseType(Type type)
    {
        if (!m_XmlPhraseType.IsAssignableFrom(type))
            return false;

        if (!m_XmlConvertFieldInfos.ContainsKey(type))
            m_XmlConvertFieldInfos.Add(type, type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic));
        return true;
    }

    static string IXmlPhraseToString(Type type, object data, int iteration)
    {
        string phrase = "";
        if (iteration >= m_PhraseLiterateBreakPoints.Length)
        {
            Debug.LogError("Iteration Max Reached!");
            return phrase;
        }
        char dataBreak = m_PhraseLiterateBreakPoints[iteration];
        int fieldLength = m_XmlConvertFieldInfos[type].Length;
        for (int i = 0; i < fieldLength; i++)
        {
            FieldInfo field = m_XmlConvertFieldInfos[type][i];
            object fieldValue = field.GetValue(data);
            string fieldString = ConvertToString(field.FieldType, fieldValue, iteration);
            phrase += fieldString;
            if (i != fieldLength - 1)
                phrase += dataBreak;
        }
        return phrase;
    }

    static object IXmlPraseToData(Type type, string data, int iteration)
    {
        object objectData = Activator.CreateInstance(type);
        if (iteration >= m_PhraseLiterateBreakPoints.Length)
        {
            Debug.LogError("Iteration Max Reached!");
            return null;
        }
        char dataBreak = m_PhraseLiterateBreakPoints[iteration];
        int fieldLength = m_XmlConvertFieldInfos[type].Length;
        string[] splitString = data.Split(dataBreak);
        if (splitString.Length != fieldLength)
            throw new Exception("Field Not Match:"+data+"|"+type);
        for (int i = 0; i < fieldLength; i++)
        {
            FieldInfo field = m_XmlConvertFieldInfos[type][i];
            string fieldString = splitString[i];
            object fieldValule = ConvertToObject(field.FieldType, fieldString, iteration);
            field.SetValue(objectData, fieldValule);
        }
        return objectData;
    }
    #endregion
}

public static class TDataCrypt
{
    public static string EasyCryptData(string data, string key)
    {
        byte[] bdata = Encoding.UTF8.GetBytes(data);
        byte[] bkey = Encoding.UTF8.GetBytes(key);
        for (int i = 0; i < bdata.Length; i++)
            bdata[i] ^= bkey[i % key.Length];
        return Encoding.UTF8.GetString(bdata);
    }
}

#endregion