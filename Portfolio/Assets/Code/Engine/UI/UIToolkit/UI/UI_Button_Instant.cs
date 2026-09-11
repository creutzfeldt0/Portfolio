using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

public class UI_Button_Instant : UI_ButtonBase
{
#if UNITY_EDITOR

    ~UI_Button_Instant()
    {
    }

    public UI_Button_Instant( VisualElement ve_Btn, Action<UI_ButtonBase> callbackSelect = null ) : base(ve_Btn, callbackSelect)
    {
    }

    protected override void Callback_ClickButton( ClickEvent clickEvent )
    {
        m_VE_ButtonSelected.AddToClassList( "BTN_Selected_On" );
        m_VE_ButtonSelected.RemoveFromClassList( "BTN_Selected_Off" );

        m_VE_ButtonSelected.RegisterCallback<TransitionEndEvent>( Callback_ClickButton_Half );
        m_SelectCallback?.Invoke( this );
    }

    protected override void Callback_ClickButton_End( TransitionEndEvent endEvent )
    {
        m_VE_ButtonSelected.UnregisterCallback<TransitionEndEvent>( Callback_ClickButton_End );
    }
#endif  // UNITY_EDITOR
}
