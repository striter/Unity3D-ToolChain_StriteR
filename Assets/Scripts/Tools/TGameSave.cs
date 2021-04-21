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
        static XmlNode m_ParentNode;
        static XmlNode temp_SubNode;
        public static XmlDocument m_Doc = new XmlDocument();
        public static string GetFiledPath<T>() where T : CDataSave<T>, new() => s_Directory + CDataSave<T>.s_FilePath;
        static void Init<T>() where T : CDataSave<T>, new()
        {
            try    //Check If File Complete
            {
                if (!Directory.Exists(s_Directory))
                    throw new Exception("None Save Path Found:" + s_Directory);

                string filedPath = GetFiledPath<T>();
                if (!File.Exists(filedPath))
                    throw new Exception("None Xml Data Found:" + filedPath);

                m_Doc.Load(filedPath);
                m_ParentNode = m_Doc.SelectSingleNode(typeof(T).Name);
                if (m_ParentNode == null)
                    throw new Exception("None Xml Parent Found:" + typeof(T).Name);

                foreach(var fieldInfo in CDataSave<T>.s_FieldInfos)
                    if (m_ParentNode.SelectSingleNode(fieldInfo.Name) == null)
                        throw new Exception("Invalid Xml Child:" + fieldInfo.Name);
            }
            catch
            {
                CreateDefaultDoc<T>();
            }
        }

        public static void ReadPersistentData<T>(this T _data) where T : CDataSave<T>,new ()
        {
            Init<T>();
            try
            {
                ReadData(_data);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Data Read Error:" + e.Message);
                CreateDefaultDoc<T>();
                ReadData(_data);
            }
        }

        public static void SavePersistentData<T>(this CDataSave<T> data) where T: CDataSave<T>,new ()
        {
            Init<T>();
            try
            {
                SaveData(data);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Data Save Error:" + e.Message);
                CreateDefaultDoc<T>();
            }
        }


        static void ReadData<T>(CDataSave<T> _data) where T:CDataSave<T>,new()
        {
            bool dataCrypt = _data.DataCrypt();
            FieldInfo[] fieldInfo = CDataSave<T>.s_FieldInfos;
            for (int i = 0; i < fieldInfo.Length; i++)
            {
                string readData = m_ParentNode.SelectSingleNode(fieldInfo[i].Name).InnerText;
                if (dataCrypt) readData = TDataCrypt.EasyCryptData(readData, m_DataCryptKey);
                fieldInfo[i].SetValue(_data, TDataConvert.Convert(fieldInfo[i].FieldType, readData));
            }
            _data.DataRecorrect();
        }
        
        static void SaveData<T>(CDataSave<T> _data) where T : CDataSave<T>, new()
        {
            bool dataCrypt = _data.DataCrypt();
            FieldInfo[] fieldInfos = CDataSave<T>.s_FieldInfos;
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                temp_SubNode = m_ParentNode.SelectSingleNode(fieldInfos[i].Name);
                string saveData = TDataConvert.Convert(fieldInfos[i].FieldType, fieldInfos[i].GetValue(_data));
                if (dataCrypt) saveData = TDataCrypt.EasyCryptData(saveData, m_DataCryptKey);
                temp_SubNode.InnerText = saveData;
                m_ParentNode.AppendChild(temp_SubNode);
            }
            m_Doc.Save(GetFiledPath<T>());
        }

        static void CreateDefaultDoc<T>() where T:CDataSave<T>,new()
        {
            Debug.LogWarning("Default Data Created:"+typeof(T).Name);
            if (!Directory.Exists(s_Directory))
                Directory.CreateDirectory(s_Directory);

            string filePath = GetFiledPath<T>();
            if (File.Exists(filePath))
                File.Delete(filePath);

            m_Doc = new XmlDocument();
            m_ParentNode = m_Doc.AppendChild(m_Doc.CreateElement(typeof(T).Name));
            foreach(var fieldInfo in CDataSave<T>.s_FieldInfos)
                m_ParentNode.AppendChild(m_Doc.CreateElement(fieldInfo.Name));
            SaveData(new T());
        }
    }

}