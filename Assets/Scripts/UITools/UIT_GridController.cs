using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIT_GridControllerBase<T> where T:class
{
    public Transform transform => m_Pool.transform;
    public ObjectPoolListBase<int, T> m_Pool { get; private set; }
    public int m_Count => m_Pool.Count;
    public UIT_GridControllerBase(ObjectPoolListBase<int, T> pool)
    {
        m_Pool = pool;
    }

    int m_NoneIdentitfiedCount = 0;
    public T AddItem() => AddItem(m_NoneIdentitfiedCount++);
    public virtual T AddItem(int identity)=>m_Pool.AddItem(identity);
    public virtual void RemoveItem(int identity) => m_Pool.RemoveItem(identity);
    public virtual void ClearGrid()
    {
        m_NoneIdentitfiedCount = 0;
        m_Pool.Clear();
    } 
    public bool Contains(int identity) => m_Pool.ContainsItem(identity);
    public T GetItem(int identity) => Contains(identity) ? m_Pool.GetItem(identity) : null;
    public T AddItem(int xIdentity, int yIdentity) => AddItem(GetIdentity(xIdentity, yIdentity));
    public T GetItem(int xIdentity, int yIdentity) => GetItem(GetIdentity(xIdentity, yIdentity));
    int GetIdentity(int xIdentity, int yIdentity) => xIdentity + yIdentity * 1000;
    public T GetOrAddItem(int identity) => Contains(identity) ? GetItem(identity) : AddItem(identity);
    public virtual void Sort(Comparison<KeyValuePair<int, T>> comparison) => m_Pool.Sort(comparison);
}

public class UIT_GridItemClass : CObjectPoolClass<int>
{
    public RectTransform rectTransform { get; private set; }
    public UIT_GridItemClass(Transform transform):base(transform){ rectTransform = transform as RectTransform; }
}

public class UIT_GridControllerClass<T> : UIT_GridControllerBase<T> where T: UIT_GridItemClass
{
    public UIT_GridControllerClass(Transform _transform):base(new ObjectPoolListClass<int, T>(_transform, "GridItem"))
    {
    }
}


public class UIT_GridControllerComponent<T> : UIT_GridControllerBase<T> where T : Component
{
    public UIT_GridControllerComponent(Transform _transform) : base(new ObjectPoolListComponent<int, T>(_transform, "GridItem"))
    {
    }

}



public class UIT_GridControllerGridItem<T>: UIT_GridControllerBase<T> where T:UIT_GridItem
{
    public GridLayoutGroup m_GridLayout { get; private set; }
    public UIT_GridControllerGridItem(Transform _transform) : base(new ObjectPoolListMonobehaviour<int,T>(_transform,"GridItem"))
    {
        m_GridLayout = _transform.GetComponent<GridLayoutGroup>();
    }

    public override T AddItem(int identity)
    {
        T item = m_Pool.AddItem(identity);
        item.transform.SetSiblingIndex(identity);
        return item;
    }

}

public class UIT_GridControllerGridItemScrollView<T> : UIT_GridControllerGridItem<T> where T : UIT_GridItem
{
    ScrollRect m_ScrollRect;
    int m_VisibleCount;
    public UIT_GridControllerGridItemScrollView(Transform _transform,int visibleCount) : base(_transform.Find("Viewport/Content"))
    {
        m_VisibleCount = visibleCount;
        m_ScrollRect = _transform.GetComponent<ScrollRect>();
        m_ScrollRect.onValueChanged.AddListener((Vector2 delta) => OnRectChanged());
    }

    public override void Sort(Comparison<KeyValuePair<int, T>> comparison)
    {
        base.Sort(comparison);
        m_ScrollRect.verticalNormalizedPosition = 1;
        OnRectChanged();
    }

    void OnRectChanged()
    {
        int totalCount = m_Pool.m_ActiveItemDic.Count;
        int current = (int)(Mathf.Clamp01(m_ScrollRect.verticalNormalizedPosition) * totalCount);
        int rangeMin = current - m_VisibleCount;
        int rangeMax = current + m_VisibleCount;

        foreach (int index in m_Pool.m_ActiveItemDic.Keys)
        {
            GetItem(index).SetShowScrollView(rangeMin< totalCount && totalCount < rangeMax);
            totalCount--;
        }
    }
}


public class UIT_GridControlledSingleSelect<T> : UIT_GridControllerGridItem<T> where T : UIT_GridItem
{
    public int m_Selecting { get; private set; } = -1;
    Action<int> OnItemSelect;
    public UIT_GridControlledSingleSelect(Transform _transform,Action<int> _OnItemSelect) : base(_transform)
    {
        OnItemSelect = _OnItemSelect;
    }
    public override T AddItem(int identity)
    {
        T item= base.AddItem(identity);
        item.InitHighlight(OnItemClick);
        item.OnHighlight(false);
        return item;
    }
    public override void RemoveItem(int identity)
    {
        if (identity == m_Selecting)
            m_Selecting = -1;
        base.RemoveItem(identity);
    }
    public void OnItemClick(int index)
    {
        if (m_Selecting != -1)
            GetItem(m_Selecting).OnHighlight(false);
        m_Selecting = index;
        GetItem(m_Selecting).OnHighlight(true);
        OnItemSelect(index);
    }
    public override void ClearGrid()
    {
        base.ClearGrid();
        m_Selecting = -1;
    }
    public void ClearHighlight()
    {
        if (m_Selecting == -1)
            return;
        GetItem(m_Selecting).OnHighlight(false);
        m_Selecting = -1;
    }
}