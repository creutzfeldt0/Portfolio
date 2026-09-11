#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

/// <summary>
/// ScrollView 추상 클래스, 실 이용은 클래스 상속받아서 만들도록 한다.. 
/// </summary>
public abstract class UI_ScrollviewBase
{
#if UNITY_EDITOR

    protected Scene_SpineTool                           m_SceneSpineTool;
    protected VisualElement                             m_VE_Scrollview;
    protected ScrollView                                m_ScrollView;
    protected VisualTreeAsset                           m_ScrollItem;

    protected List<UI_ScrollItemBase>                   m_Items = new List<UI_ScrollItemBase>();

    ~UI_ScrollviewBase() 
    {
        m_SceneSpineTool        = null;
        m_VE_Scrollview         = null;
        m_ScrollView            = null;
        m_ScrollItem            = null;
    }

    public UI_ScrollviewBase( Scene_SpineTool sceneSpineTool, VisualElement ve_ScrollView, VisualTreeAsset scrollItem )
    {
        m_SceneSpineTool = sceneSpineTool;
        m_VE_Scrollview = ve_ScrollView;
        m_ScrollItem = scrollItem;

        m_ScrollView = (ScrollView)m_VE_Scrollview;
    }

    public abstract UI_ScrollItemBase CreateScrollItem( object data = null );
    public abstract void DestroyScrollItem(int index);
    protected UI_ScrollItemBase GetScrollItemBase(int index)
    {
        if(m_Items.Count > index )
        {
            return m_Items[index];
        }

        return null;
    }

    public ScrollView GetScrollView()
    {
        return m_ScrollView;
    }

    protected VisualElement AddScrollItemVE()
    {
        VisualElement tmpVE = m_ScrollItem.Instantiate();
        m_ScrollView.Add( tmpVE );
        return tmpVE;
    }

    protected void RemoveScrollItemVE( int index )
    {
        m_ScrollView.RemoveAt(index);
    }

    protected void RemoveAllScrollItemVE()
    {
        m_ScrollView.Clear();
    }

    protected VisualElement GetScrollItemVE(int index)
    {
        return m_ScrollView.ElementAt(index);
    }

#endif  // UNITY_EDITOR
}
