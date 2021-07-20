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
    #endregion
    [MTitle] public bool m_Grid = false;
    [ColorUsage(true, true)] public Color m_GridColor = Color.white;
    [Range(0.01f, .2f)] public float m_GridSize = .015f;
    [MTitle] public bool m_CloudShadow = false;
    [Header("Shape")]
    public Texture m_ShadowTexture;
    [Range(0,1)] public float m_ShadowStrength = 1f;
    [Range(1,200)]public float m_ShadowScale;
    [Range(0,100)]public float m_ShadowPlaneDistance;
    [Range(0f,1f)] public float m_StepBegin;
    [Range(0f,.5f)]public float m_StepWidth;
    public Vector2 m_Flow;
    private void OnValidate()
    {
        URender.EnableGlobalKeyword(KW_Grid,m_Grid);
        Shader.SetGlobalColor(ID_GridColor, m_GridColor);
        Shader.SetGlobalVector(ID_GridSize, new Vector4(0f,0f, 2f, m_GridSize));
        
        URender.EnableGlobalKeyword(KW_CloudShadow,m_CloudShadow);
        Shader.SetGlobalTexture(ID_CloudTexture,m_ShadowTexture);
        Shader.SetGlobalVector(ID_CloudShape,new Vector4(1f-m_ShadowStrength,m_ShadowScale,m_ShadowPlaneDistance));
        Shader.SetGlobalVector(ID_CloudFlow,new Vector4(m_StepBegin,m_StepBegin+m_StepWidth,m_Flow.x,m_Flow.y));
    }
}
