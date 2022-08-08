using UnityEngine;
using UnityEngine.UI;

namespace ExampleScenes.Rendering.FogOfWar2D
{
    public class FogOfWar2D : MonoBehaviour
    {
        private void Start()
        {
            TouchConsole.InitDefaultCommands();
            
            m_Texture = RenderTexture.GetTemporary(1920, 1080);
            m_Texture.enableRandomWrite = true;
            m_Texture.Create();

            m_KernalHandle = m_ComputeShader.FindKernel("CSMain");
            transform.Find("Image").GetComponent<RawImage>().texture = m_Texture;

            TouchConsole.NewPage("Compute Shader");
            TouchConsole.Command("Random Lights", KeyCode.Space).Button(()=> {
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
    }
}

