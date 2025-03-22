using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RawImageClip : MaskableGraphic , ISerializationCallbackReceiver
{
    [SerializeField] Texture m_Texture;

    protected RawImageClip()
    {
        useLegacyMeshGeneration = false;
    }

    public override Texture mainTexture
    {
        get
        {
            if (m_Texture == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return m_Texture;
        }
    }

    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }


    [InspectorButton]
    public override void SetNativeSize()
    {
        Texture tex = mainTexture;
        if (tex != null)
        {
            int w = Mathf.RoundToInt(tex.width);
            int h = Mathf.RoundToInt(tex.height);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.sizeDelta = new Vector2(w, h);
        }
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetMaterialDirty();
        SetVerticesDirty();
        SetRaycastDirty();
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Texture tex = mainTexture;
        vh.Clear();
        if (tex != null)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            
            var scaleX = tex.width * tex.texelSize.x;
            var scaleY = tex.height * tex.texelSize.y;

            var pivot = rectTransform.pivot;
            var clipRect = new Rect(r.xMin /  tex.width + pivot.x,r.yMin / tex.height + pivot.y, r.width / tex.width,r.height / tex.height);
            
            {
                var color32 = color;
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(clipRect.xMin * scaleX,clipRect.yMin * scaleY));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(clipRect.xMin * scaleX, clipRect.yMax * scaleY));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(clipRect.xMax * scaleX, clipRect.yMax * scaleY));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(clipRect.xMax * scaleX, clipRect.yMin * scaleY));

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
        }
    }

    public void OnBeforeSerialize(){}

    public void OnAfterDeserialize() => OnDidApplyAnimationProperties();
}
