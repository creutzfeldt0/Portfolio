using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

public class UI_ToggleBase
{
#if UNITY_EDITOR
    private VisualElement                           m_VE_Toggle;
    private VisualElement                           m_VE_ToggleBase;
    private VisualElement                           m_VE_ToggleSelected;
    private VisualElement                           m_LB_Toggle;

    private bool                                    m_IsSelected = false;
    private int                                     m_Index = -1;
    private Action<ClickEvent, UI_ToggleBase>       m_SelectCallback;

    ~UI_ToggleBase()
    {
        m_VE_Toggle                 = null;
        m_VE_ToggleBase             = null;
        m_VE_ToggleSelected         = null;
        m_LB_Toggle                 = null;
    }

    public UI_ToggleBase( VisualElement ve_Toggle, int index, Action<ClickEvent,UI_ToggleBase> callBackSelect = null, bool bSelected = false )
    {
        m_VE_Toggle = ve_Toggle;
        m_Index = index;
        m_SelectCallback = callBackSelect;
        m_IsSelected = bSelected;

        m_VE_ToggleBase = m_VE_Toggle.Q<Button>("Toggle_Base");
        m_VE_ToggleSelected = m_VE_ToggleBase.Q<VisualElement>("VE_Selected");
        m_LB_Toggle = m_VE_ToggleBase.Q<Label>("LB_Toggle_Text");

        m_VE_ToggleBase.RegisterCallback<ClickEvent>( Callback_SelectToggle );

        m_LB_Toggle.RegisterCallback<MouseEnterEvent>( Callback_Hover );
        m_LB_Toggle.RegisterCallback<MouseLeaveEvent>( Callback_Hover_End );
    }

    public void RefreshUI()
    {
        if( true == m_IsSelected )
        {
             m_VE_ToggleSelected.AddToClassList( "Toggle_Selected_On" );
             m_VE_ToggleSelected.RemoveFromClassList( "Toggle_Selected_Off" );
        }
        else
        {
             m_VE_ToggleSelected.RemoveFromClassList( "Toggle_Selected_On" );
             m_VE_ToggleSelected.AddToClassList( "Toggle_Selected_Off" );
        }
    }

    private void Callback_Hover( MouseEnterEvent enterEvent )
    {
        m_LB_Toggle.AddToClassList( "LB_Toggle_Text_Hover" );
        m_LB_Toggle.RemoveFromClassList( "LB_Toggle_Text" );
    }

    private void Callback_Hover_End( MouseLeaveEvent leaveEvent )
    {
        m_LB_Toggle.RemoveFromClassList( "LB_Toggle_Text_Hover" );
        m_LB_Toggle.AddToClassList( "LB_Toggle_Text" );
    }

    private void Callback_SelectToggle( ClickEvent clickEvent)
    {
        m_SelectCallback?.Invoke( clickEvent, this );
    }

    public VisualElement GetVisualElement()
    {
        return m_VE_Toggle;
    }

    public int GetIndex()
    {
        return m_Index;
    }

    public void SetSelected( bool selected )
    {
        m_IsSelected = selected;
    }

    public bool GetSelected()
    {
        return m_IsSelected;
    }
#endif  // UNITY_EDITOR
}
