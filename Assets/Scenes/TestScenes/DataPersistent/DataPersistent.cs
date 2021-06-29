using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TDataPersistent;
public class DataPersistent : MonoBehaviour
{
    [Serializable]
    public struct SubDataTest
    {
        public string m_StringValue;
        public int m_IntValue;
        public float[] m_Floats;
    }
    public class DataPersistentTest : CDataSave<DataPersistentTest>
    {
        public string m_MainValue;
        public List<SubDataTest> m_Datas;
        public DataPersistentTest()
        {
            m_MainValue = "?";
            m_Datas = new List<SubDataTest>();
        }
        public override bool DataCrypt() => false;
    }
    public string m_Value;
    public SubDataTest[] m_Values;
    public DataPersistentTest m_PersistentData=new DataPersistentTest();
    void Start()
    {
        m_PersistentData.SavePersistentData();
        m_PersistentData.ReadPersistentData();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            m_PersistentData.m_Datas = m_Values?.ToList();
            m_PersistentData.m_MainValue = m_Value;
            Debug.Log("Save");
            return;
        }
        Debug.Log(m_PersistentData.m_MainValue+" "+(m_PersistentData.m_Datas?.Count??0));
    }
}