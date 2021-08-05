using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    #region IDs
    private const string KW_Grid = "_EDITGRID";
    private static readonly int ID_GridSize = Shader.PropertyToID("_GridSize");
    private static readonly int ID_GridColor = Shader.PropertyToID("_GridColor");

    private const string KW_CloudShadow = "_CLOUDSHADOW";
    private static readonly int ID_CloudTexture = Shader.PropertyToID("_CloudShadowTexture");
    private static readonly int ID_CloudShape = Shader.PropertyToID("_CloudParam1");
    private static readonly int ID_CloudFlow = Shader.PropertyToID("_CloudParam2");

    private const string KW_HorizonBend = "_HORIZONBEND";
    private static readonly int ID_HorizonBendPosition =  Shader.PropertyToID("_HorizonBendPosition");
    private static readonly int ID_HorizonBendDistances = Shader.PropertyToID("_HorizonBendDistances");
    private static readonly int ID_HorizonBendDirection = Shader.PropertyToID("_HorizonBendDirection");
    #endregion
    [MTitle] public bool m_Grid = false;
    [ColorUsage(true, true)] public Color m_GridColor = Color.white;
    [Range(0.01f, .2f)] public float m_GridSize = .015f;
    [Header("Shadow")]
    [MTitle] public bool m_CloudShadow = false;
    public Texture m_ShadowTexture;
    [Range(0,1)] public float m_ShadowStrength = 1f;
    [Range(1,200)]public float m_ShadowScale;
    [Range(0,100)]public float m_ShadowPlaneDistance;
    [Range(0f,1f)] public float m_StepBegin;
    [Range(0f,.5f)]public float m_StepWidth;
    public Vector2 m_Flow;

    [Header("Horizon Bend")] 
    public bool m_HorizonBendEnable;
    public float m_HorizonBendBegin;
    public float m_HorizonBendWidth;
    public Vector3 m_HorizonBendDirection;
    public Camera m_HorizonBendCamera;
    private void OnValidate()
    {
        URender.EnableGlobalKeyword(KW_Grid,m_Grid);
        Shader.SetGlobalColor(ID_GridColor, m_GridColor);
        Shader.SetGlobalVector(ID_GridSize, new Vector4(0f,0f, 2f, m_GridSize));
        
        URender.EnableGlobalKeyword(KW_CloudShadow,m_CloudShadow);
        Shader.SetGlobalTexture(ID_CloudTexture,m_ShadowTexture);
        Shader.SetGlobalVector(ID_CloudShape,new Vector4(1f-m_ShadowStrength,m_ShadowScale,m_ShadowPlaneDistance));
        Shader.SetGlobalVector(ID_CloudFlow,new Vector4(m_StepBegin,m_StepBegin+m_StepWidth,m_Flow.x,m_Flow.y));

        URender.EnableGlobalKeyword(KW_HorizonBend,m_HorizonBendEnable);
        Shader.SetGlobalVector(ID_HorizonBendDirection,m_HorizonBendDirection);
        Shader.SetGlobalVector(ID_HorizonBendDistances,new Vector4(m_HorizonBendBegin,m_HorizonBendBegin+m_HorizonBendWidth));
        Shader.SetGlobalVector(ID_HorizonBendPosition,m_HorizonBendCamera.transform.position);
    }

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
        Shader.SetGlobalVector(ID_HorizonBendPosition,m_HorizonBendCamera.transform.position);
    }

    void NextTerrain(int index)
    {
        if(index>=0)
            m_TerrainPool.RemoveItem(index);
        m_Index++;
        m_TerrainPool.AddItem(m_Index).transform.position = new Vector3(0,0,20f*(m_Index-1));
    }
}
