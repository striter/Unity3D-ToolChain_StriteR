using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMesh : MaskableGraphic
{
    public Mesh m_Mesh;
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if(m_Mesh!=null)
            vh.FillMesh(m_Mesh);
    }
}
