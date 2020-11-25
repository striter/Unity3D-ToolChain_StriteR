using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostEffect_ViewGlobalTexture : MonoBehaviour
{
    public string m_TextureName;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Texture tex = Shader.GetGlobalTexture(m_TextureName);
        if (tex != null)
            Graphics.Blit(tex, destination);
        else
            Graphics.Blit(source, destination);
    }
}
