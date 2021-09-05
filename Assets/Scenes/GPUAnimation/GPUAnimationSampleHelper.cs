using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rendering.Optimize;
public class GPUAnimationSampleHelper : MonoBehaviour
{
    public int m_X=32,m_Y=32;
    public GPUAnimationData m_Data;
    public Material m_Material;
    AnimationTicker[] m_Timers;
    Matrix4x4[] m_Matrixs;
    MaterialPropertyBlock m_Blocks;
    float[] curFrames;
    float[] nextFrames;
    float[] interpolates;
    protected void Awake()
    {
        int totalCount = m_Y * m_X;
        m_Matrixs = new Matrix4x4[totalCount];
        m_Timers = new AnimationTicker[totalCount];
        curFrames = new float[totalCount];
        nextFrames = new float[totalCount];
        interpolates = new float[totalCount];
        m_Blocks = new MaterialPropertyBlock();
        for (int i=0;i<m_X;i++)
        {
            for(int j=0;j<m_Y;j++)
            {
                int index = i * m_Y + j;
                m_Matrixs[index] = Matrix4x4.TRS(transform.position+new Vector3(i * 10, 0, j * 10),transform.rotation,Vector3.one*(.8f+URandom.RandomUnit()*.2f));
                m_Timers[index] = new AnimationTicker();
                m_Timers[index].Setup(m_Data.m_AnimationClips);
                m_Timers[index].SetAnimation(m_Data.m_AnimationClips.RandomIndex());
                m_Timers[index].SetNormalizedTime(Random.value);
            }
        }
        m_Data.ApplyMaterial(m_Material);
    }

    private void Update()
    {
        float _deltaTime = Time.deltaTime;
        for (int i = 0; i < m_X; i++)
        {
            for (int j = 0; j < m_Y; j++)
            {
                int index = i * m_Y + j;
                m_Timers[index].Tick(_deltaTime,out var output);
                if (!m_Timers[index].m_Anim.loop && (m_Timers[index].m_TimeElapsed >= m_Timers[index].m_Anim.length))
                    m_Timers[index].SetNormalizedTime(0f);

                curFrames[index] = output.cur;
                nextFrames[index] = output.next;
                interpolates[index] = output.interpolate;
            }
        }
        m_Blocks.SetFloatArray("_AnimFrameBegin",curFrames);
        m_Blocks.SetFloatArray("_AnimFrameEnd",nextFrames);
        m_Blocks.SetFloatArray("_AnimFrameInterpolate",interpolates);
        Graphics.DrawMeshInstanced(m_Data.m_BakedMesh, 0, m_Material, m_Matrixs,m_Matrixs.Length, m_Blocks, UnityEngine.Rendering.ShadowCastingMode.On,true);
    }
}
