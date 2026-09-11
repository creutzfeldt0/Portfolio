using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
#endif  // UNITY_EDITOR

public class UI_SliderBase
{
#if UNITY_EDITOR
    private Slider                                  m_Slider;
    private Action<float>                           m_CallbackChange;

    ~UI_SliderBase()
    {
        m_Slider                    = null;
    }

    public UI_SliderBase( VisualElement ve_Slider, Action<float> callbackChanged )
    {
        m_Slider = (Slider)ve_Slider;
        m_CallbackChange = callbackChanged;

        m_Slider.value = 1f;

        foreach( VisualElement ve in ve_Slider.Children() )
        {
            if(ve is Label)
            {
                ve.style.display = DisplayStyle.None; 
            }
        }

        ve_Slider.RegisterCallback< ChangeEvent<float> >( Callback_Changed );
    }
    public Slider GetSlider()
    {
        return m_Slider;
    }

    private void Callback_Changed( ChangeEvent<float> changeEvent )
    {
        m_CallbackChange?.Invoke( changeEvent.newValue );
    }

#endif  // UNITY_EDITOR
}
