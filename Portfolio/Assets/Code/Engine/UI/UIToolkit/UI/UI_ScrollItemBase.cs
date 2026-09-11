#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

public class UI_ScrollItemBase
{
#if UNITY_EDITOR

    protected VisualElement m_VE_ScrollItem;

    ~UI_ScrollItemBase()
    {
        m_VE_ScrollItem         = null;
    }

    public UI_ScrollItemBase( VisualElement veScrollItem )
    {
        m_VE_ScrollItem = veScrollItem;
    }

    public virtual void RefreshUI()
    {
    }

    public VisualElement GetScrollItem()
    {
        return m_VE_ScrollItem;
    }
#endif  // UNITY_EDITOR
}
