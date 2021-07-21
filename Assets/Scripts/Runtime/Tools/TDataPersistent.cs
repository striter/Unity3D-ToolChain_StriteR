using System.Xml;
using System.IO;
using UnityEngine;
using System.Reflection;
using System;

namespace TDataPersistent
{
    public class CDataSave<T> where T:class,new()
    {
        public static readonly FieldInfo[] s_FieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly string s_FilePath =  "/" + typeof(T).Name + ".sav";
        public virtual bool DataCrypt() => true;
    }
    public static class TDataSave
    {
        private const string c_DataCryptKey = "StriteRTestCrypt";
        private static readonly string s_persistentPath = Application.persistentDataPath + "/Save";
        private static readonly string s_streamingPath = Application.streamingAssetsPath + "/Save";
        private static XmlDocument m_Doc = new XmlDocument();
        private static string GetStreamingPath<T>() where T : CDataSave<T>, new() => s_streamingPath + CDataSave<T>.s_FilePath;
        public static string GetPersistentPath<T>() where T : CDataSave<T>, new() => s_persistentPath + CDataSave<T>.s_FilePath;
        public static void ReadPersistentData<T>(this T _data) where T : CDataSave<T>,new ()
        {
            try
            {
                Validate<T>(GetPersistentPath<T>(),out var parentNode);
                ReadData(_data,parentNode);
            }
            catch (Exception ePersistent)
            {
                Debug.LogWarning("Data Read Fail,Use Streaming Data:\n" + ePersistent.Message);
                ReadDefaultData(_data);
            }
        }

        public static void SavePersistentData<T>(this CDataSave<T> _data) where T: CDataSave<T>,new ()
        {
            string persistentPath = GetPersistentPath<T>();
            try
            {
                Validate<T>(persistentPath,out var parentNode);
                SaveData(_data,parentNode,persistentPath);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Data Save Error,Use Streaming Data\n" + e.Message);
                ReadDefaultData(_data);
            }
        }

        static void Validate<T>(string _path,out XmlNode _node) where T : CDataSave<T>, new()
        {
            m_Doc.Load(_path);
            _node = m_Doc.SelectSingleNode(typeof(T).Name);
            if (_node == null)
                throw new Exception("None Xml Parent Found:" + typeof(T).Name);

            foreach(var fieldInfo in CDataSave<T>.s_FieldInfos)
                if (_node.SelectSingleNode(fieldInfo.Name) == null)
                    throw new Exception("Invalid Xml Child:" + fieldInfo.Name);
        }

        static void ReadData<T>(CDataSave<T> _data,XmlNode _node) where T:CDataSave<T>,new()
        {
            bool dataCrypt = _data.DataCrypt();
            FieldInfo[] fieldInfo = CDataSave<T>.s_FieldInfos;
            for (int i = 0; i < fieldInfo.Length; i++)
            {
                string readData = _node.SelectSingleNode(fieldInfo[i].Name).InnerText;
                if (dataCrypt) readData = TDataCrypt.EasyCryptData(readData, c_DataCryptKey);
                fieldInfo[i].SetValue(_data, TDataConvert.Convert(fieldInfo[i].FieldType, readData));
            }
        }
        
        static void SaveData<T>(CDataSave<T> _data, XmlNode _node,string _path) where T : CDataSave<T>, new()
        {
            bool dataCrypt = _data.DataCrypt();
            FieldInfo[] fieldInfos = CDataSave<T>.s_FieldInfos;
            foreach (var t in fieldInfos)
            {
                var fieldNode = _node.SelectSingleNode(t.Name);
                string saveData = TDataConvert.Convert(t.FieldType, t.GetValue(_data));
                if (dataCrypt) saveData = TDataCrypt.EasyCryptData(saveData, c_DataCryptKey);
                fieldNode.InnerText = saveData;
                _node.AppendChild(fieldNode);
            }
            m_Doc.Save(_path);
        }

        static void ReadDefaultData<T>(CDataSave<T> _data) where T : CDataSave<T>, new()
        {
            try
            {
                Validate<T>( GetStreamingPath<T>(),out XmlNode node);
                ReadData(_data,node);
                SaveData(_data,node,GetPersistentPath<T>());
            }
            catch(Exception eStreaming)
            {
                Debug.LogError("Streaming Data Invalid,Use BuiltIn-Code:\n"+eStreaming.Message);

                if (!Directory.Exists(s_persistentPath))
                    Directory.CreateDirectory(s_persistentPath);
        
                string filePath = GetPersistentPath<T>();
                if (File.Exists(filePath))
                    File.Delete(filePath);

                m_Doc = new XmlDocument();
                var node = m_Doc.AppendChild(m_Doc.CreateElement(typeof(T).Name));
                foreach(var fieldInfo in CDataSave<T>.s_FieldInfos)
                    node.AppendChild(m_Doc.CreateElement(fieldInfo.Name));
            
                SaveData(new T(),node,GetPersistentPath<T>());
            }
        }
    }

}