using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDataPersistent;
using System;
using UnityEngine.UI;
using Rendering.PostProcess;
public class FogOfWar2D : MonoBehaviour
{
    public Vector3 m_SrcVector = Vector3.one;
    public Vector3 m_DstVector = Vector3.down;
    public float m_RotateAngle;
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawLine(Vector3.zero, m_SrcVector);
        Gizmos.DrawLine(Vector3.zero, m_DstVector);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero,UAlgorithm.UQuaternion.AngleAxisToRotateMatrix(m_RotateAngle,m_DstVector)*m_SrcVector);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, UAlgorithm.UQuaternion.AngleAxisToQuaternion(m_RotateAngle, m_DstVector)*m_SrcVector);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, Quaternion.AngleAxis(m_RotateAngle, m_DstVector)* m_SrcVector);
        //Debug.Log(TVector.SqrMagnitude(m_SrcVector) + " " +  m_SrcVector.sqrMagnitude);
        //Debug.Log(TVector.Dot(m_SrcVector, m_DstVector) + " " + Vector3.Dot(m_SrcVector, m_DstVector));
        //Debug.Log(TVector.Project(m_SrcVector, m_DstVector) + " " + Vector3.Project(m_SrcVector, m_DstVector));
        //Debug.Log(TVector.Cross(m_SrcVector, m_DstVector) + " " + Vector3.Cross(m_SrcVector, m_DstVector));
    }
    public SaveTest m_SaveTest = new SaveTest();
    private void Start()
    {
        UIT_TouchConsole.InitDefaultCommands();
        UIT_TouchConsole.NewPage("Data Persistent");
        UIT_TouchConsole.Command("Read").Button(() => { m_SaveTest.ReadPersistentData(); Debug.Log(m_SaveTest.Test1); });
        UIT_TouchConsole.Command("Save").Slider(0,10,m_SaveTest.Test1, value =>
        {
            m_SaveTest.Test1 = value;
            m_SaveTest.m_Test1.m_Test1 = value * value;
            m_SaveTest.SavePersistentData();
        });

        m_Texture = RenderTexture.GetTemporary(1920, 1080);
        m_Texture.enableRandomWrite = true;
        m_Texture.Create();

        m_KernalHandle = m_ComputeShader.FindKernel("CSMain");
        transform.Find("Image").GetComponent<RawImage>().texture = m_Texture;

        UIT_TouchConsole.NewPage("Compute Shader");
        UIT_TouchConsole.Command("Random Lights", KeyCode.Space).Button(()=> {
            int randomCount = 5+URandom.RandomInt(8);
            lights = new Vector4[randomCount];
            for(int i=0;i<randomCount;i++)
            {
                float randomPixelX = URandom.RandomInt(Screen.width);
                float randomPixelY = URandom.RandomInt(Screen.height);
                float randomRadius = 50f + URandom.Random01()*200f;
                randomRadius *= randomRadius;
                float randomIntenisty = .5f + URandom.Random01()*2f;
                lights[i] = new Vector4(randomPixelX,randomPixelY,randomRadius,randomIntenisty);
            }
        });
    }

    public ComputeShader m_ComputeShader;
    int m_KernalHandle;
    RenderTexture m_Texture;
    Vector4[] lights = new Vector4[] { new Vector4(0, 0, 100 * 100, 2) };
    private void Update()
    {

        Vector4[] curLights = lights;
        
        if (Input.GetMouseButton(0))
            curLights = lights.Add(new Vector4(Input.mousePosition.x, Input.mousePosition.y, 200 * 200, 1));
        m_ComputeShader.SetVectorArray("_Lights", curLights);
        m_ComputeShader.SetInt("_LightCount", curLights.Length);
        m_ComputeShader.SetTexture(m_KernalHandle, "Result", m_Texture);
        m_ComputeShader.SetVector("Result_TexelSize",new Vector4(1f/m_Texture.width,1f/m_Texture.height,m_Texture.width,m_Texture.height));
        m_ComputeShader.SetVector("_TexelCount", new Vector4(m_Texture.width, m_Texture.height));
        m_ComputeShader.Dispatch(m_KernalHandle, m_Texture.width / 8, m_Texture.height / 8, 1);
    }
    private void OnDestroy()
    {
        RenderTexture.ReleaseTemporary(m_Texture);
    }

    [Serializable]
    public class SaveTest : CDataSave<SaveTest>
    {
        public float Test1;
        public string Test2;
        public SaveTest1 m_Test1;
        public override bool DataCrypt() => true;
    }
    [Serializable]
    public struct SaveTest1
    {
        public float m_Test1;
        public Dictionary<int, string> m_Test4;
    }
}

