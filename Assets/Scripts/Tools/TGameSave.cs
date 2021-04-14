using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace TDataPersistent
{
    public class CDataSave<T> where T:class,new()
    {
        public static readonly FieldInfo[] s_FieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly string s_FilePath =  "/" + typeof(T).Name + ".xml";
        public virtual bool DataCrypt() => true;
        public virtual void DataRecorrect() { }
    }
    public static class TDataSave
    {
        public const string m_DataCryptKey = "StriteRTestCrypt";
        public static readonly string s_Directory = Application.persistentDataPath + "/Save";
        static TDataSave()
        {
            if (!Directory.Exists(s_Directory))
                Directory.CreateDirectory(s_Directory);
        }
        static Dictionary<Type, object> m_Datas=new Dictionary<Type, object>();
        static XmlNode m_ParentNode;
        static XmlElement temp_Element;
        static XmlNode temp_SubNode;
        public static XmlDocument m_Doc = new XmlDocument();
        public static string GetFiledPath<T>() where T : CDataSave<T>, new() => s_Directory + CDataSave<T>.s_FilePath;
        static void Init<T>() where T : CDataSave<T>, new()
        {
            if (m_Datas.ContainsKey(typeof(T)))
                return;
            try    //Check If File Complete
            {
                string filedPath = GetFiledPath<T>();
                if (!File.Exists(filedPath))
                    throw new Exception("None Xml Data Found:" + filedPath);

                m_Doc.Load(filedPath);
                m_ParentNode = m_Doc.SelectSingleNode(typeof(T).Name);
                if (m_ParentNode != null)
                {
                    FieldInfo[] fieldInfos = CDataSave<T>.s_FieldInfos;
                    for (int i = 0; i < fieldInfos.Length; i++)
                    {
                        temp_SubNode = m_ParentNode.SelectSingleNode(fieldInfos[i].Name);
                        if (temp_SubNode == null)
                            throw new Exception("Invalid Xml Child:" + fieldInfos[i].Name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Warning:{0},Create Default Data", e.Message);
                SaveData(CreateDefaultDoc<T>());
            }
        }

        public static T ReadData<T>() where T : CDataSave<T>,new ()
        {
            Init<T>();
            Type type = typeof(T);
            if (m_Datas.ContainsKey(type))
                return (T)m_Datas[type];

            T readFile = ReadFile<T>();
            m_Datas.Add(type, readFile);
            return readFile;
        }

        static T ReadFile<T>() where T: CDataSave<T>, new()
        {
            try
            {
                T temp = new T();
                bool dataCrypt = temp.DataCrypt();
                FieldInfo[] fieldInfo = CDataSave<T>.s_FieldInfos;
                for (int i = 0; i < fieldInfo.Length; i++)
                {
                    string readData = m_ParentNode.SelectSingleNode(fieldInfo[i].Name).InnerText;
                    if (dataCrypt) readData = TDataCrypt.EasyCryptData(readData, m_DataCryptKey);
                    fieldInfo[i].SetValue(temp, TDataConvert.Convert(fieldInfo[i].FieldType, readData));
                }
                temp.DataRecorrect();
                return temp;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Xml Read File Error:" + e.Message);
                return CreateDefaultDoc<T>();
            }
        }

        public static void SaveData<T>(this CDataSave<T> data) where T: CDataSave<T>,new ()
        {
            Init<T>();
            bool dataCrypt = data.DataCrypt();
            FieldInfo[] fieldInfos = CDataSave<T>.s_FieldInfos;
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                temp_SubNode = m_ParentNode.SelectSingleNode(fieldInfos[i].Name);
                string saveData = TDataConvert.Convert(fieldInfos[i].FieldType, fieldInfos[i].GetValue(data));
                if (dataCrypt) saveData = TDataCrypt.EasyCryptData(saveData, m_DataCryptKey);
                temp_SubNode.InnerText = saveData;
                m_ParentNode.AppendChild(temp_SubNode);
            }
            m_Doc.Save(GetFiledPath<T>());
        }

        static T CreateDefaultDoc<T>() where T:CDataSave<T>,new()
        {
            Debug.LogWarning("New Default Xml Doc Created.");
            string filePath = GetFiledPath<T>();
            if (File.Exists(filePath))
                File.Delete(filePath);

            T temp = new T();

            m_Doc = new XmlDocument();
            temp_Element = m_Doc.CreateElement(typeof(T).Name);
            m_ParentNode = m_Doc.AppendChild(temp_Element);

            FieldInfo[] fieldInfos = CDataSave<T>.s_FieldInfos;
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                temp_Element = m_Doc.CreateElement(fieldInfos[i].Name);
                temp_Element.InnerText = TDataConvert.Convert(fieldInfos[i].FieldType, fieldInfos[i].GetValue(temp));
                m_ParentNode.AppendChild(temp_Element);
            }

            return temp;
        }
    }

}