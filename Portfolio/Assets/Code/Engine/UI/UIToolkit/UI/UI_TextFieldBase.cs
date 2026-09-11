using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

public class UI_TextFieldBase
{
#if UNITY_EDITOR
    private TextField                               m_TextField;
    private Action<string>                          m_CallbackChange;

    ~UI_TextFieldBase()
    {
        m_TextField                    = null;
    }

    public UI_TextFieldBase( VisualElement ve_TextField, Action<string> callbackChanged )
    {
        m_TextField = (TextField)ve_TextField;
        m_CallbackChange = callbackChanged;

        m_TextField.RegisterCallback< ChangeEvent<string> >( Callback_Changed );
    }
    public TextField GetTextField()
    {
        return m_TextField;
    }

    private void Callback_Changed( ChangeEvent<string> changeEvent )
    {
        m_CallbackChange?.Invoke( changeEvent.newValue );
    }

#endif  // UNITY_EDITOR
}
