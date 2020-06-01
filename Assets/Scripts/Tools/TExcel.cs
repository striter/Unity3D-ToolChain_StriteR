using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Reflection;
using Excel;
using UnityEngine;
using System.Linq;

namespace TExcel
{
    public interface ISExcel
    {
        void InitAfterSet();
    }

    class Properties<T> where T : struct,ISExcel
    {
        static List<T> m_Properties=null;
        public static bool B_Inited => m_Properties != null;
        public static int I_ColumnCount => m_Properties.Count;
        public static List<T> PropertiesList
        {
            get
            {
                if (m_Properties == null)
                {
                    Debug.LogError(typeof(T).ToString() + ",Excel Not Inited,Shoulda Init Property First");
                    return null;
                }
                return m_Properties;
            }
        }
        public static void Init()
        {
            m_Properties =  Tools.GetFieldData<T>(Tools.ReadExcelFirstSheetData(Resources.Load<TextAsset>("Excel/"+typeof(T).Name)));
        }
        public static void Clear()
        {
            m_Properties.Clear();
            m_Properties = null;
        }
    }

    struct SheetProperty<T> where T : struct,ISExcel
    {
        public string m_SheetName { get; private set; }
        public List<T> m_Properties { get; private set; }
        public static SheetProperty<T> Create(string sheetName,List<T> _properties)
        {
            SheetProperty<T> item=new SheetProperty<T>();
            item.m_SheetName = sheetName;
            item.m_Properties = _properties;
            return item;
        }
    }

    class SheetProperties<T> where T : struct, ISExcel
    {
        static Dictionary<int, SheetProperty<T>> m_AllProperties = null;
        public static bool B_Inited => m_AllProperties != null;
        public int I_SheetCount => m_AllProperties.Count;
        public static List<T> GetPropertiesList(int i)
        {
            if (m_AllProperties == null)
            {
                Debug.LogError(typeof(T).ToString() + ",Excel Not Inited,Shoulda Init Property First");
                return null;
            }
            return m_AllProperties[i].m_Properties;
        }
        public static void Init()
        {
            m_AllProperties = new Dictionary<int, SheetProperty<T>>();
            Dictionary<string, List<string[]>> m_AllDatas = Tools.ReadExcelMultipleSheetData(Resources.Load<TextAsset>("Excel/"+typeof(T).Name));
            foreach(string sheetName in m_AllDatas.Keys)
                m_AllProperties.Add(m_AllProperties.Count, SheetProperty<T>.Create(sheetName, Tools.GetFieldData<T>(m_AllDatas[sheetName])));
        }
        
        public static void Clear()
        {
            m_AllProperties.Clear();
            m_AllProperties = null;
        }
    }

    class Tools
    {
        public static List<string[]> ReadExcelFirstSheetData(TextAsset excelAsset) => ReadExcelData(excelAsset,false).First().Value;
        public static Dictionary<string, List<string[]>> ReadExcelMultipleSheetData(TextAsset excelAsset) => ReadExcelData(excelAsset, true);
        static Dictionary<string, List<string[]>> ReadExcelData(TextAsset excelAsset,bool readExtraSheet)
        {
            IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(new MemoryStream(excelAsset.bytes));
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            do
            {
                List<string[]> properties = new List<string[]>();
                while (reader.Read())
                {
                    string[] row = new string[reader.FieldCount];
                    for (int i = 0; i < row.Length; i++)
                    {
                        string data = reader.GetString(i);
                        row[i] = data == null ? "" : data;
                    }
                    if (row[0] != "")
                        properties.Add(row);
                }
                result.Add(reader.Name, properties);
            } while (readExtraSheet&&reader.NextResult());
            return result;
        }

        public static List<T> GetFieldData<T>(List<string[]> data) where T : ISExcel
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.NonPublic|BindingFlags.DeclaredOnly | BindingFlags.Instance);
            List<T> targetData = new List<T>();
            try
            {
                Type type = typeof(T);

                for (int i = 0; i < data.Count; i++)
                {
                    if (i == 0)     //Ignore Row 0 and 1
                        continue;

                    object obj = Activator.CreateInstance(type, true);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        try
                        {
                            Type phraseType = fields[j].FieldType;
                            object value = null;
                            string phraseValue = data[i][j].ToString();
                            if (phraseValue.Length == 0)
                                value = TDataConvert.Default(phraseType);
                            else
                                value = TDataConvert.Convert(phraseType, phraseValue);
                            fields[j].SetValue(obj, value);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Inner Info:|" + data[i + 1][j].ToString() + "|,Field:" + fields[j].Name + "|" + fields[j].FieldType.ToString() + ", Rows/Column:" + (i + 1).ToString() + "/" + (j + 1).ToString() + "    Message:" + e.Message);
                        }
                    }
                    T dataObject = (T)obj;
                    dataObject.InitAfterSet();
                    targetData.Add(dataObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Excel|" + typeof(T).Name.ToString() + " Error:" + e.Message + e.StackTrace);
            }
            return targetData;
        }
    }
}