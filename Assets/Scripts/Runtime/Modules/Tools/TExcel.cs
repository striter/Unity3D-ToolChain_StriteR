using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Reflection;

using UnityEngine;
using System.Linq;
using ExcelDataReader;

namespace TExcel
{
    public interface ISExcel<T> where T:struct
    {
    }

    static class Properties<T, Y> where T : struct where Y : struct, ISExcel<T>
    {
        static Dictionary<T,Y> m_Properties=null;
        public static bool B_Inited => m_Properties != null;
        public static int I_ColumnCount => m_Properties.Count;
        public static Y Get(T key)
        {
            if (m_Properties == null)
              throw new Exception(typeof(Y).ToString() + ",Excel Not Inited,Shoulda Init Property First");

            if (!m_Properties.ContainsKey(key))
                throw new Exception(typeof(T).ToString()+"Excel Not Contains Key:"+key);

            return m_Properties[key];
        }
        public static void Init()
        {
            m_Properties =  Tools.GetFieldData<T,Y>(Tools.ReadExcelFirstSheetData(Resources.Load<TextAsset>("Excel/"+typeof(Y).Name)));
        }
        public static void Clear()
        {
            m_Properties.Clear();
            m_Properties = null;
        }
    }

    struct SheetProperty<T,Y> where T:struct where Y : struct,ISExcel<T>
    {
        public string m_SheetName { get; private set; }
        public Dictionary<T,Y> m_Properties { get; private set; }
        public static SheetProperty<T,Y> Create(string sheetName,Dictionary<T,Y> _properties)
        {
            SheetProperty<T,Y> item=new SheetProperty<T,Y>();
            item.m_SheetName = sheetName;
            item.m_Properties = _properties;
            return item;
        }
    }

    static class SheetProperties<T,Y> where T:struct where Y : struct, ISExcel<T>
    {
        static Dictionary<int, SheetProperty<T,Y>> m_AllProperties = null;
        public static bool B_Inited => m_AllProperties != null;
        public static int I_SheetCount => m_AllProperties.Count;
        public static Dictionary<T,Y> GetPropertiesList(int i)
        {
            if (m_AllProperties == null)
            {
                Debug.LogError(typeof(Y).ToString() + ",Excel Not Inited,Shoulda Init Property First");
                return null;
            }
            return m_AllProperties[i].m_Properties;
        }
        public static void Init()
        {
            m_AllProperties = new Dictionary<int, SheetProperty<T,Y>>();
            Dictionary<string, List<string[]>> m_AllDatas = Tools.ReadExcelMultipleSheetData(Resources.Load<TextAsset>("Excel/"+typeof(Y).Name));
            foreach(string sheetName in m_AllDatas.Keys)
                m_AllProperties.Add(m_AllProperties.Count, SheetProperty<T,Y>.Create(sheetName, Tools.GetFieldData<T,Y>(m_AllDatas[sheetName])));
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
            IExcelDataReader reader = ExcelReaderFactory.CreateReader(new MemoryStream(excelAsset.bytes));
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            do
            {
                List<string[]> properties = new List<string[]>();
                while (reader.Read())
                {
                    string[] row = new string[reader.FieldCount];
                    for (int i = 0; i < row.Length; i++)
                    {
                        row[i] = "";
                        object obj = reader.GetValue(i);
                        if(obj!=null)
                            row[i] = obj.ToString();
                    }
                    if (row[0] != "")
                        properties.Add(row);
                }
                result.Add(reader.Name, properties);
            } while (readExtraSheet&&reader.NextResult());
            reader.Close();
            return result;
        }

        public static Dictionary<T,Y> GetFieldData<T,Y>(List<string[]> data) where T:struct where Y :struct,ISExcel<T>
        {
            Type valueType = typeof(Y);
            FieldInfo[] fields = valueType.GetFields(BindingFlags.NonPublic|BindingFlags.DeclaredOnly | BindingFlags.Instance);
            Dictionary<T,Y> targetData = new Dictionary<T, Y>();
            try
            {
                for (int i = 1; i < data.Count; i++)        //Ignore First Row
                {
                    T key = TDataConvert.Convert<T>(data[i][0]);        //First Column Use As Key
                    object value = Activator.CreateInstance(valueType, true);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        Type phraseType = fields[j].FieldType;
                        object fieldValue = null;
                        string phraseValue = data[i][j + 1].ToString();
                        try
                        {
                            if (phraseValue.Length == 0)
                                fieldValue = TDataConvert.Default(phraseType);
                            else
                                fieldValue = TDataConvert.Convert(phraseType, phraseValue);
                            fields[j].SetValue(value, fieldValue);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Inner Info:|" + data[i][j+1].ToString() + "|,Field:" + fields[j].Name + "|" + fields[j].FieldType.ToString() + ", Rows/Column:" + (i).ToString() + "/" + (j + 1).ToString() + "    Message:" + e.Message);
                        }
                    }
                    targetData.Add(key, (Y)value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Excel|" + typeof(Y).Name.ToString() + " Error:" + e.Message + e.StackTrace);
            }
            return targetData;
        }
    }
}