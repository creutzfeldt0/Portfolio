#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;
#endif  // UNITY_EDITOR

/// <summary>
/// 에디터 FX 표시용 스크롤뷰 아이템들..
/// </summary>
public class UI_Scrollview_FX : UI_ScrollviewBase
{
#if UNITY_EDITOR

    ~UI_Scrollview_FX() 
    {
      
    }

    /// <summary>
    /// 생성할 scrollItem 원본도 미리 넣는다..
    /// </summary>
    /// <param name="sceneSpineTool"></param>
    /// <param name="ve_ScrollView"></param>
    /// <param name="scrollItem"></param>
    /// <returns></returns>
    public UI_Scrollview_FX( Scene_SpineTool sceneSpineTool, VisualElement ve_ScrollView, VisualTreeAsset scrollItem ): base(sceneSpineTool, ve_ScrollView, scrollItem)
    {
    }

    /// <summary>
    /// fxPrefabPath가 null일 경우 프리팹 불러오는 버튼으로 UI생성됨.
    /// </summary>
    /// <param name="fxPrefabPath"></param>
    /// <returns></returns>
    public override UI_ScrollItemBase CreateScrollItem( object data = null ) 
    {
        FXData fxData = (FXData)data;

        //에디터 UI 생성(스크롤뷰 아이템)..
        VisualElement ve_ScrollItem = base.AddScrollItemVE();
        UI_ScrollItem_FX scrollItem = new UI_ScrollItem_FX( this, ve_ScrollItem, fxData );

        m_Items.Add(scrollItem);

        return scrollItem;
    }
    public override void DestroyScrollItem(int index)
    {
        if( 0 > index ) return;

        if(index < m_Items.Count)
        {
            RemoveScrollItemVE( index );
            m_Items.RemoveAt(index);
        }
    }

    public void DestroyAllScrollItem()
    {
        RemoveAllScrollItemVE();

        m_Items.Clear();
        
        Scene_SpineTool scene = Manager_SpineFX.Instance.GetScene();
        Transform m_SpineBackTrans = scene.GetTransform_FXs(true);
        m_SpineBackTrans.DestroyChildren();
        
        m_SpineBackTrans = scene.GetTransform_FXs(false);
        m_SpineBackTrans.DestroyChildren();
    }

    public void RefreshScrollItem()
    {
        DestroyAllScrollItem();

        UI_ScrollItem_FX scrollItem_FX; 
        List<FXData> fXDatas = Manager_SpineFX.Instance.GetFXDatas_All();

        if(null == fXDatas)
        {
            CreateScrollItem();
            return;
        }

        string aniName = Manager_SpineFX.Instance.GetSkeletonAnimation().AnimationName;
        
        if ( null == Manager_SpineFX.Instance.GetSkeletonAnimation().skeleton.Skin )
        {
            Manager_SpineFX.Instance.GetSkeletonAnimation().skeleton.SetSkin( Manager_SpineFX.Instance.GetSkeletonAnimation().skeleton.Data.DefaultSkin );
        }
        
        string skinName = Manager_SpineFX.Instance.GetSkeletonAnimation().skeleton.Skin.Name;

        for(int i = 0; i < fXDatas.Count; i++)
        {
            switch ( (Manager_SpineFX.FXTYPE) fXDatas[i].FXType)
            {
                case Manager_SpineFX.FXTYPE.애니FX:
                case Manager_SpineFX.FXTYPE.안지워짐:
                    if (aniName != fXDatas[i].FXTypeData)
                    {
                        continue;
                    }
                    break;    
                case Manager_SpineFX.FXTYPE.상시스킨:
                    if (skinName != fXDatas[i].FXTypeData)
                    {
                        continue;
                    }
                    break;
                //case Manager_SpineFX.FXTYPE.상시FX:
                //case Manager_SpineFX.FXTYPE.None:
                default:
                    break;
            }
            
            scrollItem_FX = (UI_ScrollItem_FX)CreateScrollItem( fXDatas[i] );

            if(null == scrollItem_FX)   return;

            scrollItem_FX.InitFXPrefab( fXDatas[i].FXPath );
            scrollItem_FX.RefreshUI();
        }

        CreateScrollItem();
    }

    public UI_ScrollItem_FX GetScrollItem(int index)
    {
        UI_ScrollItemBase scrollItemBase = base.GetScrollItemBase(index);

        if( null == scrollItemBase || 
            scrollItemBase is not UI_ScrollItem_FX )  
                return null;

        return (UI_ScrollItem_FX)scrollItemBase;
    }

#endif  // UNITY_EDITOR
}
