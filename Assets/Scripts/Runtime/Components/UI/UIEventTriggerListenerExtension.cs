using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventTriggerListenerExtension : EventTrigger
{
    public Action onClick;
    public Action<Vector2> onClickWorld;
    public Action<Vector2> onClickLocal;
    public Action<bool, Vector2> onPress;
    public Action<bool, Vector2> onPressLocal;

    public Action<bool, Vector2> onDragStatus;
    public Action<Vector2> onDrag, onDragDelta;
    public Action onRayCast;
    
    public void Clear()
    {
        onClick = null;
        onClickWorld = null;
        onClickLocal = null;
        onPress = null;
        onPressLocal = null;
        onRayCast = null;
        onDrag = null;
        onDragDelta = null;
        onDragStatus = null;
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        onClick?.Invoke();
        onClickWorld?.Invoke(eventData.position);
        
        if (onClickLocal == null)
            return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.enterEventCamera, out var pos);
        onClickLocal(pos);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        onPress?.Invoke(true, eventData.position);
        if (onPressLocal == null)
            return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle( transform as RectTransform,eventData.position,eventData.enterEventCamera,out var pos);
        onPressLocal(true, pos);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        onPress?.Invoke(false,eventData.position);
        
        if (onPressLocal == null)
            return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle( transform as RectTransform,eventData.position,eventData.enterEventCamera,out var pos);
        onPressLocal(false, pos);
    }

    public bool m_Dragging { get; private set; }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        onDragStatus?.Invoke(true, eventData.position);
        m_Dragging = true;
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        onDrag?.Invoke(eventData.position);
        onDragDelta?.Invoke(eventData.delta);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        onDragStatus?.Invoke(false, eventData.position);
        m_Dragging = false;
    }
    public void OnRaycast()
    {
        onRayCast?.Invoke();
    }

}
