

#if UNITY_EDITOR
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEditor;
using Slider = UnityEngine.UIElements.Slider;
#endif  // UNITY_EDITOR

/// <summary>
/// 스파인툴에서 에디터 탭 눌렀을 시 나오는 UI..
/// </summary>
public class Popup_Spine_Editor : UTPopup_Base
{
#if UNITY_EDITOR
    private Scene_SpineTool                         m_SceneSpineTool;

    //씬 환경 관련..        
    private List<UI_ToggleBase>                     m_ToggleScene = new List<UI_ToggleBase>();

    //스파인 데이터 관련..      
    private UI_ButtonBase                           m_BtnLoadDataAsset;
    private UI_TextFieldBase                        m_TF_Scale;

    //애니메이션 관련..     
    private SkeletonAnimation                       m_SkeletonAnimation;
    private UI_ButtonBase                           m_BTN_AniSelect;
    private Label                                   m_LB_AnimationName;
    private UI_ButtonBase                           m_BTN_SkinSelect;
    private Label                                   m_LB_AnimationTotalTime;
    private Label                                   m_LB_AnimationTimeScale;
    private Slider                                  m_SL_AnimationCurTime;
    private TextField                               m_TF_AnimationCurTime;
    private List<UI_ButtonBase>                     m_BTN_AnimationTimeControls = new List<UI_ButtonBase>();

    //FX관련..
    private UI_Scrollview_FX                        m_SV_FX_View;

    [SerializeField] private VisualTreeAsset        m_ScrollItem_FX; 

    protected override void OnDestroy() 
    {
        m_SceneSpineTool                    = null;

        m_ToggleScene.Clear();
        m_ToggleScene                       = null;

        m_BtnLoadDataAsset                  = null;

        m_TF_Scale                          = null;

        m_BTN_AniSelect                     = null;
        m_LB_AnimationName                  = null;
        m_BTN_SkinSelect                    = null;
        m_LB_AnimationTotalTime             = null;

        m_BTN_AnimationTimeControls.Clear();
        m_BTN_AnimationTimeControls         = null;

        m_SV_FX_View                        = null;
        m_ScrollItem_FX                     = null;
    }
    protected override void Initialize( object baseData )
    {
        m_SceneSpineTool = (Scene_SpineTool)baseData;
        m_SkeletonAnimation = Manager_SpineFX.Instance.GetSkeletonAnimation();
    }

    protected override void CreateElement()
    {
        if(null == m_uiPopupVE)   return;

        VisualElement tmpVE;

        //씬 환경 관련..
        int index = -1;
        tmpVE = m_uiPopupVE.Q<VisualElement>("Toggle_SceneEmpty");
        m_ToggleScene.Add( new UI_ToggleBase(tmpVE, ++index, Callback_ToggleSelect) );

        tmpVE = m_uiPopupVE.Q<VisualElement>("Toggle_SceneUI");
        m_ToggleScene.Add( new UI_ToggleBase(tmpVE, ++index, Callback_ToggleSelect) );

        tmpVE = m_uiPopupVE.Q<VisualElement>("Toggle_SceneInGame");
        m_ToggleScene.Add( new UI_ToggleBase(tmpVE, ++index, Callback_ToggleSelect) );

        //스파인 데이터 관련..
        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_LoadDataAsset");
        m_BtnLoadDataAsset = new UI_ButtonBase(tmpVE, Callback_LoadDataAsset);

        tmpVE = m_uiPopupVE.Q<VisualElement>("TF_SpineScale");
        m_TF_Scale = new UI_TextFieldBase(tmpVE, Callback_Change_TFScale);

        //스킨 관련..
        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_SkinSelect");
        m_BTN_SkinSelect = new UI_ButtonBase(tmpVE, Callback_OpenPopupSkinSelect);

        //애니메이션 관련..
        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_AniSelect");
        m_BTN_AniSelect = new UI_ButtonBase(tmpVE, Callback_OpenPopupAniSelect);

        m_LB_AnimationName = m_uiPopupVE.Q<Label>("LB_AniName");
        m_LB_AnimationTotalTime = m_uiPopupVE.Q<Label>("LB_AniTime");
        m_LB_AnimationTimeScale = m_uiPopupVE.Q<Label>("LB_AniTimeScale");

        m_SL_AnimationCurTime = m_uiPopupVE.Q<Slider>("SL_AniTime");
        m_SL_AnimationCurTime.RegisterValueChangedCallback( Callback_Slider_Changed );
        m_TF_AnimationCurTime = m_uiPopupVE.Q<TextField>("TF_AniTime");
        m_TF_AnimationCurTime.RegisterValueChangedCallback( Callback_TF_AniTime_Changed );

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_TimeBack");
        m_BTN_AnimationTimeControls.Add( new UI_Button_Instant(tmpVE, Callback_AniTime_Back) );

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_TimePlay");
        m_BTN_AnimationTimeControls.Add( new UI_Button_Instant(tmpVE, Callback_AniTime_Play) );

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_TimeStop");
        m_BTN_AnimationTimeControls.Add( new UI_Button_Instant(tmpVE, Callback_AniTime_Stop) );

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_TimeFoward");
        m_BTN_AnimationTimeControls.Add( new UI_Button_Instant(tmpVE, Callback_AniTime_Foward) );

        //FX관련..
        tmpVE = m_uiPopupVE.Q<VisualElement>("SV_FXs");
        m_SV_FX_View = new UI_Scrollview_FX(m_SceneSpineTool, tmpVE, m_ScrollItem_FX);
    }

    protected override IEnumerator OpenAnimation() 
    { 
        RefreshUI();

        yield return base.OpenAnimation();
    }


    public override void RefreshUI()
    {
        byte sceneTypeIndex = (byte)Manager_SpineFX.Instance.GetSceneType();
        m_ToggleScene[ sceneTypeIndex ].SetSelected(true);
        m_ToggleScene[ sceneTypeIndex ].RefreshUI();
        m_SceneSpineTool.OnTab_Scene();

        m_TF_Scale.GetTextField().value = Manager_SpineFX.Instance.GetSpineScale().ToString();

        if(null != m_SkeletonAnimation && false == string.IsNullOrEmpty(m_SkeletonAnimation.AnimationName) )
        {
            Spine.Animation ani = m_SkeletonAnimation.AnimationState.Data.SkeletonData.FindAnimation(m_SkeletonAnimation.AnimationName);

            if (null == m_SkeletonAnimation.skeleton.Skin)
            {
                m_SkeletonAnimation.skeleton.SetSkin( m_SkeletonAnimation.skeleton.Data.DefaultSkin );
            }

             if( null != ani && 
                 null != m_SkeletonAnimation.skeleton.Skin )     
            {
                Callback_AniSelect( ani );
            }
        }
    }

    private void FixedUpdate() 
    {
        if( null ==  m_SkeletonAnimation || 
            string.IsNullOrEmpty( m_SkeletonAnimation.AnimationName ) ||
            null == m_SL_AnimationCurTime )
                return;
        
        float curTime = m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime;
        if( curTime > m_SkeletonAnimation.AnimationState.Tracks.Items[0].Animation.Duration )
        {
            curTime = curTime % m_SkeletonAnimation.AnimationState.Tracks.Items[0].Animation.Duration;
        }

        m_SL_AnimationCurTime.value = curTime;
        m_TF_AnimationCurTime.value = $"{curTime:F3}";
    }

    protected override IEnumerator CloseAnimation()
    {
        yield return base.CloseAnimation();
    }

    private void Callback_ToggleSelect( ClickEvent clickEvent, UI_ToggleBase selectedToggle )
    {
        for(int i=0; i<m_ToggleScene.Count; ++i)
        {
            if(m_ToggleScene[i] == selectedToggle)
            {
                m_ToggleScene[i].SetSelected( true );
            }
            else
            {
                m_ToggleScene[i].SetSelected( false );
            }

            m_ToggleScene[i].RefreshUI();
        }

        Manager_SpineFX.Instance.SetSceneType( (Manager_SpineFX.SceneTypes)selectedToggle.GetIndex() );

        m_SceneSpineTool.OnTab_Scene();
    }

    private void Callback_LoadDataAsset( UI_ButtonBase selectedBtn )
    {
        string loadPath = EditorUtility.OpenFilePanel("스파인 SkeletonData 파일을 선택해 주세요.",
                                                        "Assets/5_OutResource/Spine",
                                                        "asset");

        if( string.IsNullOrEmpty( loadPath ) )  return;
        
        Manager_SpineFX.Instance.OnNewDataAsset( loadPath );

        m_SceneSpineTool.OnRefreshSpine();

        m_SkeletonAnimation = Manager_SpineFX.Instance.GetSkeletonAnimation();

        if(null == m_SkeletonAnimation )    return;

        m_LB_AnimationTimeScale.text = $"{m_SkeletonAnimation.timeScale}";

        RefreshUI();
    }

    private void Callback_Change_TFScale( string newValue )
    {
        float value;
        if( false == float.TryParse( newValue, out value ) )    return;
        
        Manager_SpineFX.Instance.SetSpineScale(value);
        
        if(null == m_SkeletonAnimation )
        {
            m_SkeletonAnimation = Manager_SpineFX.Instance.GetSkeletonAnimation();
        }  

        if(null == m_SkeletonAnimation )    return;

        m_SkeletonAnimation.transform.localScale = Vector3.one * value;

    }

    private void Callback_OpenPopupAniSelect( UI_ButtonBase selectedBtn )
    {
        if( null == m_SkeletonAnimation )   return;
        
        var aniSelectData = new Popup_AniSelect_Data()
                                {
                                    m_SceneSpineTool = this.m_SceneSpineTool,
                                    m_CallbackAniSelect = Callback_AniSelect,
                                };

        m_SceneSpineTool.OpenPopup<Popup_AniSelect>( aniSelectData );
    }

    private void Callback_AniSelect( Spine.Animation selectedAni )
    {
        m_SkeletonAnimation.AnimationName = selectedAni.Name;

        m_LB_AnimationName.text = selectedAni.Name;
        m_LB_AnimationTotalTime.text = $"{selectedAni.Duration}";

        m_SkeletonAnimation.Update( 0f );

        m_SL_AnimationCurTime.lowValue = 0f;
        m_SL_AnimationCurTime.highValue = selectedAni.Duration;
        m_SL_AnimationCurTime.value = 0f;

        m_SV_FX_View.RefreshScrollItem();
    }

    private void Callback_OpenPopupSkinSelect( UI_ButtonBase selectedBtn )
    {
        if( null == m_SkeletonAnimation )   return;
        
        var skinSelectData = new Popup_SkinSelect_Data()
                                {
                                    m_SceneSpineTool = this.m_SceneSpineTool,
                                    m_CallbackSkinSelect = Callback_SkinSelect,
                                };

        m_SceneSpineTool.OpenPopup<Popup_SkinSelect>( skinSelectData );
    }
    
    private void Callback_SkinSelect( Skin selectedSkin )
    {
        m_SkeletonAnimation.skeleton.Skin = selectedSkin;

        m_SkeletonAnimation.skeleton.SetupPoseSlots();

        m_SkeletonAnimation.Update( 0f );
        
        m_SV_FX_View.RefreshScrollItem();
    }

    private void Callback_Slider_Changed( ChangeEvent<float> changedValue )
    {
        m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime = changedValue.newValue;
    }

    private void Callback_TF_AniTime_Changed( ChangeEvent<string> changedValue )
    {
        float curTime;
        if ( float.TryParse( changedValue.newValue, out curTime ) )
        {
            m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime = curTime;
        }
    }

    private void Callback_AniTime_Back( UI_ButtonBase selectedBtn )
    {
        m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime -= 0.01f;
        m_SL_AnimationCurTime.value = m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime;
    }

    private void  Callback_AniTime_Play( UI_ButtonBase selectedBtn )
    {
        m_SkeletonAnimation.timeScale = 1f;
        m_LB_AnimationTimeScale.text = $"{m_SkeletonAnimation.timeScale}";
    }

    private void  Callback_AniTime_Stop( UI_ButtonBase selectedBtn )
    {
        m_SkeletonAnimation.timeScale = 0f;
        m_LB_AnimationTimeScale.text = $"{m_SkeletonAnimation.timeScale}";
    }

    private void  Callback_AniTime_Foward( UI_ButtonBase selectedBtn )
    {
        m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime += 0.01f;
        m_SL_AnimationCurTime.value = m_SkeletonAnimation.AnimationState.Tracks.Items[0].TrackTime;
    }

#endif  // UNITY_EDITOR
}
