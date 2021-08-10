using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform m_CameraRoot;
    private TObjectPool_Transform m_TerrainPool;

    private float m_Forward;
    private int m_Index;
    private void Awake()
    {
        m_TerrainPool = new TObjectPool_Transform(transform.Find("TerrainPool"),"Terrain");
        m_CameraRoot = transform.Find("CameraRoot");
        m_Forward = 0;
        m_Index = 0;
        NextTerrain(-1);
        NextTerrain(-1);
        NextTerrain(-1);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        m_Forward += Time.deltaTime*5f;
        if(m_Forward>(m_Index-2)*20f)
            NextTerrain(m_Index-2);
        m_CameraRoot.transform.position = Vector3.Lerp(m_CameraRoot.transform.position,new Vector3(0f,0f,m_Forward),deltaTime*5f);
    }

    void NextTerrain(int index)
    {
        if(index>=0)
            m_TerrainPool.RemoveItem(index);
        m_Index++;
        m_TerrainPool.AddItem(m_Index).transform.position = new Vector3(0,0,20f*(m_Index-1));
    }
}
