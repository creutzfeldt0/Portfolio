using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
using Spine.Unity;
using Spine;
#endif  // UNITY_EDITOR

public struct Popup_SkinSelect_Data
{
#if UNITY_EDITOR
    public Scene_SpineTool          m_SceneSpineTool;
    public Action<Skin>             m_CallbackSkinSelect;
#endif // UNITY_EDITOR
}

/// <summary>
/// 스파인툴 상단 파일탭 눌렀을 시 나오는 UI..
/// </summary>
public class Popup_SkinSelect : UTPopup_Base
{
#if UNITY_EDITOR
    [SerializeField] private VisualTreeAsset    m_VE_ListItem_String; 

    private Scene_SpineTool                     m_SceneSpineTool;

    private Dictionary<int, VisualElement>      m_VELists = new Dictionary<int, VisualElement>();
    private UI_ButtonBase                       m_BTN_SkinName_Confirm;
    private ListView                            m_LV_SkinNames;
    private Action<Skin>                        m_CallbackSkinSelect;

    private SkeletonAnimation                   m_SkeletonAnimation;
    private ExposedList<Skin>                 m_SkinNames;
    private int                                 m_SelectIndex = 0;

    protected override void OnDestroy() 
    {
        m_VE_ListItem_String                = null;

        m_SceneSpineTool                    = null;

        m_BTN_SkinName_Confirm              = null;
        m_LV_SkinNames                      = null;
        m_CallbackSkinSelect                = null;

        m_VELists.Clear();
        m_VELists                           = null;
    }
    protected override void Initialize( object baseData )
    {
        Popup_SkinSelect_Data skinSelectData = (Popup_SkinSelect_Data)baseData;

        m_SceneSpineTool = skinSelectData.m_SceneSpineTool;
        m_CallbackSkinSelect = skinSelectData.m_CallbackSkinSelect;

        m_SkeletonAnimation = Manager_SpineFX.Instance.GetSkeletonAnimation();
        
        m_SkinNames = m_SkeletonAnimation.skeleton.Data.Skins;
        
        if( null == m_SkeletonAnimation.skeleton.Skin )
        {
            m_SkeletonAnimation.skeleton.SetSkin( m_SkinNames.Items[m_SelectIndex].Name );
        }
        else
        {
            for(int i=0; i<m_SkinNames.Items.Length; ++i)
            {
                if( m_SkeletonAnimation.skeleton.Skin.Name == m_SkinNames.Items[i].Name)
                {
                    m_SelectIndex = i;
                    break;
                }
            }
        }

    }

    protected override void CreateElement()
    {
        if(null == m_uiPopupVE)   return;
        
        VisualElement tmpVE;

        m_LV_SkinNames = m_uiPopupVE.Q<ListView>("LB_Names");

        m_LV_SkinNames.makeItem = MakeItem_SkinName;
        m_LV_SkinNames.bindItem =  BindItem_SkinName;
        m_LV_SkinNames.selectionChanged += OnSelectItem_AniName;
        m_LV_SkinNames.itemsSource = m_SkinNames.Items;

        tmpVE = m_uiPopupVE.Q<VisualElement>("BTN_Name_Confirm");
        m_BTN_SkinName_Confirm = new UI_ButtonBase(tmpVE, Callback_SkinName_Confirm);
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

    private VisualElement MakeItem_SkinName()
    {
        return m_VE_ListItem_String.Instantiate();
    }

    /// <summary>
    /// 처음 설정할때 뿐만 아니라 무한스크롤 돌아서 갱신될때도 들어옴
    /// </summary>
    /// <param name="ve"></param>
    /// <param name="index"></param>
    private void BindItem_SkinName(VisualElement ve, int index)
    {
        Label lb_ListItem = ve.Q<Label>( "LB_ListItem" );

        string skinName = m_SkinNames.Items[index].Name;
        var fxList = Manager_SpineFX.Instance.GetFXDatas_Skin( skinName );

        if(null == fxList || 0 == fxList.Count)
        {
            lb_ListItem.text = skinName;
        }
        else
        {
            lb_ListItem.text = $"{skinName}\t(FX:{fxList.Count})";
        }
        
        VisualElement ve_ListItem_Selected = ve.Q<VisualElement>("VE_Selected");

        if(false == m_VELists.ContainsKey(index))
        {
            m_VELists.Add(index, ve);
        }

        if(null == ve_ListItem_Selected)    return;

        if( m_SelectIndex == index)
        {
            ve_ListItem_Selected.AddToClassList( "List_Selected_On" );
            ve_ListItem_Selected.RemoveFromClassList( "List_Selected_Off" );
        }
        else
        {
            ve_ListItem_Selected.RemoveFromClassList( "List_Selected_On" );
            ve_ListItem_Selected.AddToClassList( "List_Selected_Off" );
        }
    }

    private void OnSelectItem_AniName( IEnumerable<object> lists )
    {
        Skin skin;
        VisualElement ve;
        Label lb;

        //선택된 애니메이션..
        foreach(var item in lists)
        {
            skin = (Skin)item;

            for(int i=0; i<m_SkinNames.Items.Length; ++i)
            {
                if( m_VELists.TryGetValue( i , out ve) )
                {
                    lb = ve.Q<Label>("LB_ListItem");
                    ve = ve.Q<VisualElement>("VE_Selected");

                    if( null == ve || null == lb) continue;

                    if( skin.Name == m_SkinNames.Items[i].Name)
                    {
                        m_SelectIndex = i;

                        ve.AddToClassList( "List_Selected_On" );
                        ve.RemoveFromClassList( "List_Selected_Off" );
                    } 
                    else
                    {
                        ve.RemoveFromClassList( "List_Selected_On" );
                        ve.AddToClassList( "List_Selected_Off" );
                    }
                }
                else
                {
                    if( skin.Name == m_SkinNames.Items[i].Name)
                    {
                        m_SelectIndex = i;
                    }
                }
            }
        }
    }

    private void Callback_SkinName_Confirm( UI_ButtonBase selectBTN )
    {
        var skins = m_SkeletonAnimation.skeleton.Data.Skins;

        m_CallbackSkinSelect?.Invoke( skins.Items[m_SelectIndex] );

        ClosePopup();
    }

#endif  // UNITY_EDITOR
}
