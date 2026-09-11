

#if UNITY_EDITOR
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
#endif  // UNITY_EDITOR

/// <summary>
/// 스파인툴에서 상단 탭 UI.
/// </summary>
public class Popup_EditorTabs : UTPopup_Base
{
#if UNITY_EDITOR
    private Scene_SpineTool                 m_SceneSpineTool;
    private List<UI_ToggleBase>             m_TogglePopups = new List<UI_ToggleBase>();

    protected override void OnDestroy() 
    {
        m_SceneSpineTool                    = null;
    }
    protected override void Initialize( object baseData )
    {
        m_SceneSpineTool = (Scene_SpineTool)baseData;
    }

    protected override void CreateElement()
    {
        if(null == m_uiPopupVE)   return;

        VisualElement tmpVE;

        tmpVE = m_uiPopupVE.Q<VisualElement>("Toggle_File");
        m_TogglePopups.Add( new UI_ToggleBase(tmpVE, 0, Callback_ToggleSelect, true) );
        m_TogglePopups[0].RefreshUI();
        m_SceneSpineTool.OnTab_File();

        tmpVE = m_uiPopupVE.Q<VisualElement>("Toggle_Editor");
        m_TogglePopups.Add( new UI_ToggleBase(tmpVE, 1, Callback_ToggleSelect) );

    }

    protected override IEnumerator OpenAnimation() 
    { 
        RefreshUI();

        yield return base.OpenAnimation();
    }


    public override void RefreshUI()
    {
    }

    protected override IEnumerator CloseAnimation()
    {
        yield return base.CloseAnimation();
    }

    private void Callback_ToggleSelect( ClickEvent clickEvent, UI_ToggleBase selectedToggle )
    {
        for(int i=0; i<m_TogglePopups.Count; ++i)
        {
            if(m_TogglePopups[i] == selectedToggle)
            {
                m_TogglePopups[i].SetSelected( true );
            }
            else
            {
                m_TogglePopups[i].SetSelected( false );
            }

            m_TogglePopups[i].RefreshUI();
        }

        switch( selectedToggle.GetIndex() )
        {
            case 1:
                m_SceneSpineTool.OnTab_Editor();
                break;

            //case 0:
            default:
                m_SceneSpineTool.OnTab_File();
                break;
        }
    }
#endif  // UNITY_EDITOR
}
