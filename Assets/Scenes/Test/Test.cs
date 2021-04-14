using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDataPersistent;
using System;
public class Test : MonoBehaviour
{
    public Vector3 m_SrcVector = Vector3.one;
    public Vector3 m_DstVector = Vector3.down;
    public SaveTest m_SaveTest;
    public float m_Test;
    private void Awake()
    {
        m_SaveTest = TDataSave.ReadData<SaveTest>();
    }
    private void OnValidate()
    {
        m_SaveTest = TDataSave.ReadData<SaveTest>();
        m_SaveTest.Test1 = m_Test;
        m_SaveTest.m_Test1.m_Test1 = m_Test*m_Test;
        m_SaveTest.SaveData();
    }
    private void Update()
    {
        Debug.Log(m_SaveTest.Test1 + " " + m_SaveTest.m_Test1.m_Test1);
        Debug.Log(TDataSave.GetFiledPath<SaveTest>());
    }
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawLine(Vector3.zero, m_SrcVector);
        Gizmos.DrawLine(Vector3.zero, m_DstVector);

        //Debug.Log(TVector.SqrMagnitude(m_SrcVector) + " " +  m_SrcVector.sqrMagnitude);
        //Debug.Log(TVector.Dot(m_SrcVector, m_DstVector) + " " + Vector3.Dot(m_SrcVector, m_DstVector));
        //Debug.Log(TVector.Project(m_SrcVector, m_DstVector) + " " + Vector3.Project(m_SrcVector, m_DstVector));
        //Debug.Log(TVector.Cross(m_SrcVector, m_DstVector) + " " + Vector3.Cross(m_SrcVector, m_DstVector));
    }
    public class SaveTest:CDataSave<SaveTest>
    {
        public float Test1;
        public string Test2;
        public SaveTest1 m_Test1;
        public override bool DataCrypt() => true;
    }
    public struct SaveTest1:IDataConvert
    {
        public float m_Test1;
        Dictionary<int, string> m_Test4;
    }
}

