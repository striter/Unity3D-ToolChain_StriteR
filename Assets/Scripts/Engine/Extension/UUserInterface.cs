using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;


public class AtlasLoader
{
    protected Dictionary<string, Sprite> m_SpriteDic { get; private set; } = new Dictionary<string, Sprite>();
    public bool Contains(string name) => m_SpriteDic.ContainsKey(name);
    public string m_AtlasName { get; private set; }
    public Sprite this[string name]
    {
        get
        {
            if (!m_SpriteDic.ContainsKey(name))
            {
                Debug.LogWarning("Null Sprites Found |" + name + "|" + m_AtlasName);
                return m_SpriteDic.Values.First();
            }
            return m_SpriteDic[name];
        }
    }
    public AtlasLoader(SpriteAtlas atlas)
    {
        m_AtlasName = atlas.name;
        Sprite[] allsprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(allsprites);
        foreach (Sprite sprite in allsprites)
        {
            string name = sprite.name.Replace("(Clone)", ""); 
            m_SpriteDic.Add(name, sprite); 
        }
    }
}

public class AtlasAnim : AtlasLoader
{
    int animIndex = 0;
    List<Sprite> m_Anims;
    public AtlasAnim(SpriteAtlas atlas) : base(atlas)
    {
        m_Anims = m_SpriteDic.Values.ToList();
        m_Anims.Sort((a, b) =>
        {
            int index1 = int.Parse(System.Text.RegularExpressions.Regex.Replace(a.name, @"[^0-9]+", ""));
            int index2 = int.Parse(System.Text.RegularExpressions.Regex.Replace(b.name, @"[^0-9]+", ""));
            return index1 - index2;
        });
    }

    public Sprite Reset()
    {
        animIndex = 0;
        return m_Anims[animIndex];
    }

    public Sprite Tick()
    {
        animIndex++;
        if (animIndex == m_Anims.Count)
            animIndex = 0;
        return m_Anims[animIndex];
    }
}


public static class UUserInterface
{
    public static void SetAnchor(this RectTransform rect, Vector2 anchor)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
    }
    public static void ReAnchorFillX(this RectTransform rect, Vector2 anchorX)
    {
        rect.anchorMin = new Vector2(anchorX.x, rect.anchorMin.y);
        rect.anchorMax = new Vector2(anchorX.y, rect.anchorMax.y);
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
    }
    public static void ReAnchorReposX(this RectTransform rect, float x)
    {
        rect.anchorMin = new Vector2(x, rect.anchorMin.y);
        rect.anchorMax = new Vector2(x, rect.anchorMax.y);
        rect.anchoredPosition = Vector2.zero;
    }

    public static void SetWorldViewPortAnchor(this RectTransform rect, Vector3 worldPos, Camera camera, float lerpParam = 1f)
    {
        Vector2 viewPortAnchor = camera.WorldToViewportPoint(worldPos);
        rect.anchorMin = Vector2.Lerp(rect.anchorMin, viewPortAnchor, lerpParam);
        rect.anchorMax = Vector2.Lerp(rect.anchorMin, viewPortAnchor, lerpParam);
    }

    public static void ReparentRestretchUI(this RectTransform rect, Transform targetTrans)
    {
        rect.SetParent(targetTrans);
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
    }

    public static G2Box GetLocalBounds(this RectTransform _rectTrans,RectTransform _rootTransform)
    {
        var min = _rectTrans.anchorMin * _rootTransform.rect.size;
        min += _rectTrans.offsetMin;
        var max = _rectTrans.anchorMax * _rootTransform.rect.size;
        max += _rectTrans.offsetMax;
        
        int iteration = 0;
        var parent = _rectTrans;
        while (true)
        {
            if (iteration++ > 1024)
                throw new IndexOutOfRangeException();
            
            parent = (parent.parent as RectTransform);
            if (parent == _rootTransform)
                break;

            min += parent.anchoredPosition;
            max += parent.anchoredPosition;
        }
        return G2Box.Minmax(min, max);
    }
 
    public static G2Box GetLocalBoundsNormalized(this RectTransform _rectTrans,RectTransform _rootTransform)
    {
        // var root = (_rectTrans.parent as RectTransform);
        return GetLocalBounds(_rectTrans,_rootTransform) / _rootTransform.rect.size;
    }

    public static bool GetPointInsideImage(this Image _image, float2 _positionNormalized, RectTransform _root)
    {
        var bound = _image.rectTransform.GetLocalBoundsNormalized(_root);
        if (!bound.Contains(_positionNormalized))
            return false;

        // return true;
        var texture = _image.sprite.texture;
        if(!texture.isReadable) return true;
            
        var uv = (_positionNormalized - bound.min) / bound.size;
        var x = (int)math.floor( uv.x * texture.width);
        var y = (int)math.floor( uv.y * texture.height);
        var pixel = texture.GetPixel(x,y);
            
        return pixel.a > 0;
    }

    public static void RaycastAll(Vector2 castPos)      //Bind UIT_EventTriggerListener To Items Need To Raycast By EventSystem
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = castPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        for (int i = 0; i < results.Count; i++)
        {
            UIEventTriggerListenerExtension listener = results[i].gameObject.GetComponent<UIEventTriggerListenerExtension>();
            if (listener != null)
                listener.OnRaycast();
        }
    }
}