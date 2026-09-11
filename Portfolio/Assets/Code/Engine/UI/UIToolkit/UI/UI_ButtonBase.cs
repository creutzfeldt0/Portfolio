using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

public class UI_ButtonBase
{
#if UNITY_EDITOR
    protected VisualElement                             m_VE_Button;
    protected VisualElement                             m_VE_ButtonBase;
    protected Action<UI_ButtonBase>                     m_SelectCallback;
    protected VisualElement                             m_VE_ButtonSelected;
    protected Label                                     m_LB_Button;

    ~UI_ButtonBase()
    {
        m_VE_Button             = null;
        m_SelectCallback        = null;
        m_LB_Button             = null;
    }

    public UI_ButtonBase( VisualElement ve_Btn, Action<UI_ButtonBase> callbackSelect = null )
    {
        m_VE_Button = ve_Btn;
        m_SelectCallback = callbackSelect;

        m_VE_ButtonBase = m_VE_Button.Q<Button>("BTN_Base");
        m_VE_ButtonSelected = m_VE_ButtonBase.Q<VisualElement>("VE_Selected");
        m_LB_Button = m_VE_ButtonBase.Q<Label>("LB_BTN_Text");

        m_VE_ButtonBase.RegisterCallback<ClickEvent>( Callback_ClickButton );
        m_LB_Button.RegisterCallback<MouseEnterEvent>( Callback_Hover );
        m_LB_Button.RegisterCallback<MouseLeaveEvent>( Callback_Hover_End );
    }

    public VisualElement GetVisualElement()
    {
        return m_VE_Button;
    }

    public void SetLabel( string text )
    {
        m_LB_Button.text = text;
    }

    protected void Callback_Hover( MouseEnterEvent enterEvent )
    {
        m_LB_Button.AddToClassList( "LB_BTN_Text_Hover" );
        m_LB_Button.RemoveFromClassList( "LB_BTN_Text" );
    }

    protected void Callback_Hover_End( MouseLeaveEvent leaveEvent )
    {
        m_LB_Button.RemoveFromClassList( "LB_BTN_Text_Hover" );
        m_LB_Button.AddToClassList( "LB_BTN_Text" );
    }

    protected virtual void Callback_ClickButton( ClickEvent clickEvent )
    {
        m_VE_ButtonSelected.AddToClassList( "BTN_Selected_On" );
        m_VE_ButtonSelected.RemoveFromClassList( "BTN_Selected_Off" );

        m_VE_ButtonSelected.RegisterCallback<TransitionEndEvent>( Callback_ClickButton_Half );
    }

    protected virtual void Callback_ClickButton_Half( TransitionEndEvent endEvent )
    {
        m_VE_ButtonSelected.RemoveFromClassList( "BTN_Selected_On" );
        m_VE_ButtonSelected.AddToClassList( "BTN_Selected_Off" );

        m_VE_ButtonSelected.UnregisterCallback<TransitionEndEvent>( Callback_ClickButton_Half );
        m_VE_ButtonSelected.RegisterCallback<TransitionEndEvent>( Callback_ClickButton_End );
    }

    protected virtual void Callback_ClickButton_End( TransitionEndEvent endEvent )
    {
        m_VE_ButtonSelected.UnregisterCallback<TransitionEndEvent>( Callback_ClickButton_End );
        m_SelectCallback?.Invoke( this );
    }
#endif  // UNITY_EDITOR
}
