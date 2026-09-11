using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;
using Spine.Unity;
#endif  // UNITY_EDITOR

/// <summary>
/// 스파인툴 상단 파일탭 눌렀을 시 나오는 UI..
/// </summary>
public class Popup_File : UTPopup_Base
{
#if UNITY_EDITOR
    private Scene_SpineTool                 m_SceneSpineTool;
    private List<UI_ToggleBase>             m_ToggleScene = new List<UI_ToggleBase>();
    private UI_ButtonBase                   m_BtnLoadWork;
    private UI_ButtonBase                   m_BtnSaveWork;
    private UI_ButtonBase                   m_BtnExportWork;

    protected override void OnDestroy() 
    {
        m_SceneSpineTool                    = null;
        m_BtnLoadWork                       = null;
        m_BtnSaveWork                       = null;
        m_BtnExportWork                     = null;

        m_ToggleScene.Clear();
        m_ToggleScene                       = null;

    }
    protected override void Initialize( object baseData )
    {
        m_SceneSpineTool = (Scene_SpineTool)baseData;
    }

    protected override void CreateElement()
    {
        if(null == m_uiPopupVE)   return;

        VisualElement tmpVE;

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_Load_Work");
        m_BtnLoadWork = new UI_ButtonBase(tmpVE, Callback_LoadWork);

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_Save_Work");
        m_BtnSaveWork = new UI_ButtonBase(tmpVE, Callback_SaveWork);

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_Export_Work");
        m_BtnExportWork = new UI_ButtonBase(tmpVE, Callback_ExportWork);

        
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

    private void Callback_LoadWork( UI_ButtonBase selectedBtn )
    {
        Manager_SpineFX.Instance.OnLoadData( Callback_LoadData_Complete );
    }

    private void Callback_LoadData_Complete()
    {
        m_SceneSpineTool.OnLoadData_Complete();
        //FX Refresh는 UI_ScrollView_FX에서..
    }

    private void Callback_SaveWork( UI_ButtonBase selectedBtn )
    {
        Manager_SpineFX.Instance.OnSaveData();
    }

    private void Callback_ExportWork( UI_ButtonBase selectedBtn )
    {
        m_SceneSpineTool.OnExportData();
    }
#endif  // UNITY_EDITOR
}
