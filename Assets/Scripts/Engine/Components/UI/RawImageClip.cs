using System;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RawImageClip : MaskableGraphic , ICanvasRaycastFilter
{
    
    [SerializeField] Texture m_Texture;
    
    public RectTransform m_ClipRect;
    public Rect m_UVRect = new Rect(0.2f, 0.2f, 0.6f, 0.6f);

    protected RawImageClip()
    {
        useLegacyMeshGeneration = false;
    }

    private void OnValidate()
    {
        Update();
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

    private void Update()
    {
        if (m_ClipRect == null)
            return;
        var newRect = (Rect)m_ClipRect.GetLocalBoundsNormalized(rectTransform);
        newRect = G2Box.Clamp(G2Box.kOne,newRect);
        
        if (m_UVRect == newRect)
            return;
        m_UVRect = newRect;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Texture tex = mainTexture;
        vh.Clear();
        if (tex == null)
            return;

        var clipRect = m_UVRect;
        var r = GetPixelAdjustedRect();
        var v = new float4(r.x, r.y, r.x + r.width, r.y + r.height);
        var v2 = new float4(r.x + r.width * clipRect.min.x, r.y + r.height * clipRect.min.y, r.x + r.width * clipRect.max.x, r.y + r.height * clipRect.max.y);
        
        var color32 = color;
            
        vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0,0));
        vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0,1));
        vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1,1));
        vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1,0));
        vh.AddVert(new Vector3(v2.x, v2.y), color32, new Vector2(clipRect.min.x , clipRect.min.y));
        vh.AddVert(new Vector3(v2.x, v2.w), color32, new Vector2(clipRect.min.x , clipRect.max.y));
        vh.AddVert(new Vector3(v2.z, v2.w), color32, new Vector2(clipRect.max.x , clipRect.max.y));
        vh.AddVert(new Vector3(v2.z, v2.y), color32, new Vector2(clipRect.max.x , clipRect.min.y));

        vh.AddTriangle(0,1,4); vh.AddTriangle(5,4,1);
        vh.AddTriangle(1,2,5); vh.AddTriangle(2,6,5);
        vh.AddTriangle(2,3,6); vh.AddTriangle(3,7,6);
        vh.AddTriangle(3,4,7); vh.AddTriangle(3,0,4);
    }
    
    private Vector3[] worldCorners = new Vector3[4];
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        rectTransform.GetWorldCorners(worldCorners);
        var clipMin = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[0]);
        var clipMax = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[2]);
        var screenRect = G2Box.Normalize(G2Box.Minmax(clipMin, clipMax), m_UVRect);
        return !screenRect.Contains(screenPoint);
    }
}
