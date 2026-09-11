

#if UNITY_EDITOR
using UnityEngine.UIElements;
using System.Collections;
using Spine.Unity;
using UnityEngine;
#endif  // UNITY_EDITOR

public class Popup_SkeletonData : UTPopup_Base
{
#if UNITY_EDITOR
    private Scene_SpineTool                 m_SceneSpineTool;
    private UI_TextFieldBase                m_TF_Scale;
    private UI_ButtonBase                   m_BTN_Back;
    private UI_ButtonBase                   m_BTN_Confirm;


    protected override void OnDestroy() 
    {
        m_SceneSpineTool                    = null;
        m_TF_Scale                          = null;
        m_BTN_Back                          = null;
        m_BTN_Confirm                       = null;
    }
    protected override void Initialize( object baseData )
    {
        m_SceneSpineTool = (Scene_SpineTool)baseData;
    }

    protected override void CreateElement()
    {
        if(null == m_uiPopupVE)   return;

        VisualElement tmpVE;

        tmpVE = m_uiPopupVE.Q<VisualElement>("TF_Scale");
        m_TF_Scale = new UI_TextFieldBase(tmpVE, Callback_Change_TFScale);

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_Back");
        m_BTN_Back = new UI_ButtonBase(tmpVE, Callback_BTN_Back);

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_Confirm");
        m_BTN_Confirm = new UI_ButtonBase(tmpVE, Callback_BTN_Confirm);
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

    private void Callback_Change_TFScale( string newValue )
    {
        float value;
        if( false == float.TryParse( newValue, out value ) )    return;
        
        SkeletonAnimation skeleton =  Manager_SpineFX.Instance.GetSkeletonAnimation();

        skeleton.transform.localScale = Vector3.one * value;
    }

    private void Callback_BTN_Back( UI_ButtonBase btn )
    {
        this.ClosePopup();
        //m_SceneSpineTool.OpenPopup<Popup_DataMenu>( m_SceneSpineTool );
    }

    private void Callback_BTN_Confirm( UI_ButtonBase btn )
    {
        this.ClosePopup();
    }

#endif  // UNITY_EDITOR
}
