using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class TUI
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

    public static void RaycastAll(Vector2 castPos)      //Bind UIT_EventTriggerListener To Items Need To Raycast By EventSystem
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = castPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        for (int i = 0; i < results.Count; i++)
        {
            UIT_EventTriggerListener listener = results[i].gameObject.GetComponent<UIT_EventTriggerListener>();
            if (listener != null)
                listener.OnRaycast();
        }
    }
}