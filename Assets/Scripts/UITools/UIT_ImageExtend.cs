using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIT_ImageExtend : Image {
    static readonly int ID_SpriteRect = Shader.PropertyToID("_SpriteRect");
    protected override void UpdateGeometry()
    {
        base.UpdateGeometry();
        Debug.Log("Update");
        if (sprite == null||m_Material==null)
            return;
        Vector4 spriteRect = new Vector4(sprite.textureRect.xMin / sprite.texture.width, sprite.textureRect.yMin / sprite.texture.height,
            sprite.textureRect.xMax / sprite.texture.width, sprite.textureRect.yMax / sprite.texture.height);
        Debug.Log(spriteRect);
        m_Material.SetVector(ID_SpriteRect, spriteRect);
    }
}
