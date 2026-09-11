#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
#endif  // UNITY_EDITOR

public class UI_ScrollItem_FX : UI_ScrollItemBase
{
#if UNITY_EDITOR
    private UI_Scrollview_FX                m_UIScrollview_FX;

    //FX 추가하기 전 상태..
    private GroupBox                        m_GB_FX_None;
    private UI_ButtonBase                   m_BTN_PrefabLoad;


    //FX 추가 한 후 상태..
    private GroupBox                        m_GB_FX;
    private UI_ButtonBase                   m_BTN_PrefabReplace;
    private Label                           m_LB_PrefabName;        
    private UI_ButtonBase                   m_BTN_DeleteFX;
    private Label                           m_LB_PrefabPath;  
    private UI_ButtonBase                   m_BTN_FXType;
    private UI_ToggleBase                   m_TG_IsSpineBack;     
    private UI_ToggleBase                   m_TG_IsBone;
    private UI_ButtonBase                   m_BTN_BoneSelect;
    private UI_TextFieldBase                m_TF_FX_StartTime;
    private UI_TextFieldBase                m_TF_FX_EndTime;

    //위치 고정된 상태의 FX 상태..
    private GroupBox                        m_GB_Transform;
    private UI_TextFieldBase                m_TF_T_PosX;
    private UI_TextFieldBase                m_TF_T_PosY;
    private UI_TextFieldBase                m_TF_T_PosZ;
    private UI_TextFieldBase                m_TF_T_ScaleX;
    private UI_TextFieldBase                m_TF_T_ScaleY;
    private UI_TextFieldBase                m_TF_T_ScaleZ;

    //본 추적할때 FX 상태..
    private GroupBox                        m_GB_Transform_Bone;
    private Label                           m_LB_BoneName;
    private UI_TextFieldBase                m_TF_T_B_PosX;
    private UI_TextFieldBase                m_TF_T_B_PosY;
    private UI_TextFieldBase                m_TF_T_B_PosZ;
    private UI_TextFieldBase                m_TF_T_B_ScaleX;
    private UI_TextFieldBase                m_TF_T_B_ScaleY;
    private UI_TextFieldBase                m_TF_T_B_ScaleZ;

    private FXData                          m_FxData;
    private FXGameObject                    m_FXGameObject;

    ~UI_ScrollItem_FX()
    {
        m_UIScrollview_FX           = null;

        m_GB_FX_None                = null;
        m_BTN_PrefabLoad            = null;

        m_GB_FX                     = null;
        m_BTN_PrefabReplace         = null;
        m_LB_PrefabName             = null;       
        m_BTN_DeleteFX              = null;
        m_LB_PrefabPath             = null;
        m_BTN_FXType                = null;
        m_TG_IsSpineBack            = null;     
        m_TG_IsBone                 = null;
        m_BTN_BoneSelect            = null;
        m_TF_FX_StartTime           = null;
        m_TF_FX_EndTime             = null;
        m_GB_Transform              = null;
        m_TF_T_PosX                 = null;
        m_TF_T_PosY                 = null;
        m_TF_T_PosZ                 = null;
        m_TF_T_ScaleX               = null;
        m_TF_T_ScaleY               = null;
        m_TF_T_ScaleZ               = null;
        m_GB_Transform_Bone         = null;
        m_LB_BoneName               = null;
        m_TF_T_B_PosX               = null;
        m_TF_T_B_PosY               = null;
        m_TF_T_B_PosZ               = null;
        m_TF_T_B_ScaleX             = null;
        m_TF_T_B_ScaleY             = null;
        m_TF_T_B_ScaleZ             = null;

        m_FxData                    = null;
        if( null != m_FXGameObject )    GameObject.DestroyImmediate( m_FXGameObject.gameObject );
    }

    public UI_ScrollItem_FX( UI_Scrollview_FX uiScrollView_FX, VisualElement ve_ScrollItem, FXData fxData = null ) : base( ve_ScrollItem )
    {
        VisualElement tmpVE;
        
        m_UIScrollview_FX = uiScrollView_FX;
        m_FxData = fxData;

        m_GB_FX_None = m_VE_ScrollItem.Q<GroupBox>("GB_FX_None");

        tmpVE = m_GB_FX_None.Q<VisualElement>("BTN_FX_Load");
        m_BTN_PrefabLoad = new UI_ButtonBase( tmpVE, Callback_PrefabLoad );

        m_GB_FX = m_VE_ScrollItem.Q<GroupBox>("GB_FX");
        
        tmpVE = m_GB_FX.Q<VisualElement>("BTN_PrefabReplace");
        m_BTN_PrefabReplace = new UI_ButtonBase( tmpVE, Callback_PrefabReplace  );

        m_LB_PrefabName = m_GB_FX.Q<Label>("LB_Prefab_Name");   
        
        tmpVE = m_GB_FX.Q<VisualElement>("BTN_Delete_FX");
        m_BTN_DeleteFX = new UI_ButtonBase( tmpVE, Callback_PrefabDelete );

        m_LB_PrefabPath = m_GB_FX.Q<Label>("LB_Prefab_Path");   
        
        tmpVE = m_GB_FX.Q<VisualElement>("BTN_FXType");
        m_BTN_FXType = new UI_ButtonBase( tmpVE, Callback_FXType );

        tmpVE = m_GB_FX.Q<VisualElement>("Toggle_IsSpineBack");
        m_TG_IsSpineBack = new UI_ToggleBase( tmpVE, 0, Callback_IsSpineBack );

        tmpVE = m_GB_FX.Q<VisualElement>("Toggle_IsBone");
        m_TG_IsBone = new UI_ToggleBase( tmpVE, 0, Callback_IsBoneChase );

        tmpVE = m_GB_FX.Q<VisualElement>("BTN_BoneSelect");
        m_BTN_BoneSelect = new UI_ButtonBase( tmpVE, Callback_BoneSelect );

        tmpVE = m_GB_FX.Q<TextField>("TF_FX_StartTime");
        m_TF_FX_StartTime = new UI_TextFieldBase(tmpVE, Callback_StartTime);

        tmpVE = m_GB_FX.Q<TextField>("TF_FX_End_Time");
        m_TF_FX_EndTime = new UI_TextFieldBase(tmpVE, Callback_EndTime);

        //FX가 본 추적하지 않을때..
        m_GB_Transform = m_GB_FX.Q<GroupBox>("GB_Transform");
               
        tmpVE = m_GB_Transform.Q<TextField>("TF_Position_X");
        m_TF_T_PosX = new UI_TextFieldBase(tmpVE, Callback_FX_PosX);  
            
        tmpVE = m_GB_Transform.Q<TextField>("TF_Position_Y");
        m_TF_T_PosY = new UI_TextFieldBase(tmpVE, Callback_FX_PosY);

        tmpVE = m_GB_Transform.Q<TextField>("TF_Position_Z");
        m_TF_T_PosZ = new UI_TextFieldBase(tmpVE, Callback_FX_PosZ);

        tmpVE = m_GB_Transform.Q<TextField>("TF_Scale_X");
        m_TF_T_ScaleX = new UI_TextFieldBase(tmpVE, Callback_FX_ScaleX);  
            
        tmpVE = m_GB_Transform.Q<TextField>("TF_Scale_Y");
        m_TF_T_ScaleY = new UI_TextFieldBase(tmpVE, Callback_FX_ScaleY);

        tmpVE = m_GB_Transform.Q<TextField>("TF_Scale_Z");
        m_TF_T_ScaleZ = new UI_TextFieldBase(tmpVE, Callback_FX_ScaleZ);        

        //FX가 본 추적할 때..
        m_GB_Transform_Bone = m_GB_FX.Q<GroupBox>("GB_Transform_Bone");

        m_LB_BoneName = m_GB_Transform_Bone.Q<Label>("LB_BoneName");
           
        tmpVE = m_GB_Transform_Bone.Q<TextField>("TF_Bone_Pos_X");
        m_TF_T_B_PosX = new UI_TextFieldBase(tmpVE, Callback_FX_PosX);  
            
        tmpVE = m_GB_Transform_Bone.Q<TextField>("TF_Bone_Pos_Y");
        m_TF_T_B_PosY = new UI_TextFieldBase(tmpVE, Callback_FX_PosY);

        tmpVE = m_GB_Transform_Bone.Q<TextField>("TF_Bone_Pos_Z");
        m_TF_T_B_PosZ = new UI_TextFieldBase(tmpVE, Callback_FX_PosZ);

        tmpVE = m_GB_Transform_Bone.Q<TextField>("TF_Bone_Scale_X");
        m_TF_T_B_ScaleX = new UI_TextFieldBase(tmpVE, Callback_FX_ScaleX);  
            
        tmpVE = m_GB_Transform_Bone.Q<TextField>("TF_Bone_Scale_Y");
        m_TF_T_B_ScaleY = new UI_TextFieldBase(tmpVE, Callback_FX_ScaleY);

        tmpVE = m_GB_Transform_Bone.Q<TextField>("TF_Bone_Scale_Z");
        m_TF_T_B_ScaleZ = new UI_TextFieldBase(tmpVE, Callback_FX_ScaleZ); 
    }

    /// <summary>
    /// 처음엔 프리팹 넣으라는 기본 UI였다가 이 함수 타면 FX정보 보여주는 UI로 변경된다..
    /// </summary>
    /// <param name="fxPrefabPath"></param>
    public void InitFXPrefab( string fxPrefabPath )
    {
        if( true == string.IsNullOrEmpty(fxPrefabPath) )    return;

        string extention = Path.GetExtension(fxPrefabPath);

        if( extention != ".prefab"  )    return;

        string[] assetPaths = fxPrefabPath.Split("/Assets/");
        string assetPath = "Assets/" + assetPaths[1];

        //프리팹 생성..
        GameObject fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        if( fxPrefab == null )    return;

        m_LB_PrefabPath.text = fxPrefabPath;
        m_LB_PrefabName.text = fxPrefab.name;

        // FX 데이터 생성 or 교체..
        if(null == m_FxData)
        {
            m_FxData = Manager_SpineFX.Instance.AddFXData( fxPrefabPath, fxPrefab );
        }
        else
        {
            if(null != m_FXGameObject)
            {
                GameObject.DestroyImmediate( m_FXGameObject.gameObject );
            }

            m_FxData = Manager_SpineFX.Instance.ReplaceFXData( fxPrefabPath, fxPrefab, m_FxData );

            if( null != m_FXGameObject )
            {
                GameObject.DestroyImmediate( m_FXGameObject );
            }
        }

        if(null == m_FxData)    
        {
            return;
        }

        GameObject fxObject = new GameObject( $"FXObject_{m_FxData.Index}" );
        m_FXGameObject = fxObject.AddComponent<FXGameObject>();
        m_FXGameObject.Init( m_FxData );

        if( m_FxData.IsBoneChase )
        {
            Spine.Skeleton skeleton = Manager_SpineFX.Instance.GetSkeleton();

            if(null != skeleton)    m_FxData.ChasedBone = skeleton.Bones.Items[m_FxData.SelectedBoneIndex];
        }
    }

    public override void RefreshUI()
    {
        if( null == m_FxData )
        {
            m_GB_FX_None.style.display = DisplayStyle.Flex; 
            m_GB_FX.style.display = DisplayStyle.None; 
        }
        else
        {
            m_GB_FX_None.style.display = DisplayStyle.None; 
            m_GB_FX.style.display = DisplayStyle.Flex; 

            m_BTN_FXType.SetLabel( ((Manager_SpineFX.FXTYPE)m_FxData.FXType).ToString() );
            m_TG_IsSpineBack.SetSelected( m_FxData.IsSpineBack );

            m_TF_FX_StartTime.GetTextField().value = m_FxData.FXNotiTime.ToString();
            m_TF_FX_EndTime.GetTextField().value = m_FxData.FXDeleteTime.ToString();

            if( true == m_FxData.IsBoneChase )
            {
                m_TG_IsBone.SetSelected(true);
                m_BTN_BoneSelect.GetVisualElement().style.display  = DisplayStyle.Flex;

                m_GB_Transform.style.display  = DisplayStyle.None;
                m_GB_Transform_Bone.style.display  = DisplayStyle.Flex;

                m_TF_T_B_PosX.GetTextField().value = m_FxData.FXPosition.x.ToString();
                m_TF_T_B_PosY.GetTextField().value = m_FxData.FXPosition.y.ToString();
                m_TF_T_B_PosZ.GetTextField().value = m_FxData.FXPosition.z.ToString();

                m_TF_T_B_ScaleX.GetTextField().value = m_FxData.FXScale.x.ToString();
                m_TF_T_B_ScaleY.GetTextField().value = m_FxData.FXScale.y.ToString();
                m_TF_T_B_ScaleZ.GetTextField().value = m_FxData.FXScale.z.ToString();

                if(null != m_FxData.ChasedBone) m_LB_BoneName.text = m_FxData.ChasedBone.ToString();
            }
            else
            {
                m_TG_IsBone.SetSelected(false);
                m_BTN_BoneSelect.GetVisualElement().style.display  = DisplayStyle.None;

                m_GB_Transform.style.display  = DisplayStyle.Flex;
                m_GB_Transform_Bone.style.display  = DisplayStyle.None;

                m_TF_T_PosX.GetTextField().value = m_FxData.FXPosition.x.ToString();
                m_TF_T_PosY.GetTextField().value = m_FxData.FXPosition.y.ToString();
                m_TF_T_PosZ.GetTextField().value = m_FxData.FXPosition.z.ToString();

                m_TF_T_ScaleX.GetTextField().value = m_FxData.FXScale.x.ToString();
                m_TF_T_ScaleY.GetTextField().value = m_FxData.FXScale.y.ToString();
                m_TF_T_ScaleZ.GetTextField().value = m_FxData.FXScale.z.ToString();
            }

            m_TG_IsSpineBack.RefreshUI();
            m_TG_IsBone.RefreshUI();

            if(null != m_FXGameObject)      m_FXGameObject.RefreshFXObject();
        }
    }

    public void Callback_PrefabLoad( UI_ButtonBase  buttonBase )
    {
        if( null == Manager_SpineFX.Instance.GetSkeletonAnimation() )    return;

        string loadPath = EditorUtility.OpenFilePanel("FX Prefab 파일을 선택해 주세요.",
                                                        "Assets/8_Effect",
                                                        "prefab");

        if( string.IsNullOrEmpty( loadPath ) )  return;

        InitFXPrefab(loadPath);
        RefreshUI();

        //데이터 비어있는 ScrollItem 생성..
        UI_ScrollItemBase scrollItemBase = m_UIScrollview_FX.CreateScrollItem();
    }

    /// <summary>
    /// 로직이 PrefabLoad와 같아져 버렸다, 혹시 모르니 분리..
    /// </summary>
    /// <param name="buttonBase"></param>
    public void Callback_PrefabReplace( UI_ButtonBase  buttonBase )
    {
        if( null == Manager_SpineFX.Instance.GetSkeletonAnimation() )    return;

        string loadPath = EditorUtility.OpenFilePanel("FX Prefab 파일을 선택해 주세요.",
                                                        "Assets/8_Effect",
                                                        "prefab");

        if( string.IsNullOrEmpty( loadPath ) )  return;
        
        InitFXPrefab(loadPath);
        RefreshUI();
    }   

    public void Callback_PrefabDelete( UI_ButtonBase buttonBase )
    {
        GameObject.DestroyImmediate( m_FXGameObject );

        if(null != m_FxData)
        {
            int index = m_FxData.Index;

            Manager_SpineFX.Instance.DeleteFXData( index );
            m_FxData = null;
            m_UIScrollview_FX.DestroyScrollItem( index );
        }

        RefreshUI();
    }

    private void Callback_FXType( UI_ButtonBase buttonBase )
    {
        if(null == m_FxData)    return;

        int typeNumber = m_FxData.FXType;
        typeNumber++;
        
        if ( (int)Manager_SpineFX.FXTYPE.None-1 < typeNumber )
        {
            typeNumber = 0;
        }
        
        buttonBase.SetLabel( ((Manager_SpineFX.FXTYPE)typeNumber).ToString() );

        //데이터만 바꾸고..
        Manager_SpineFX.Instance.ChangeTypeFXData( typeNumber, m_FxData);
        
        //UI 재생성..
        m_UIScrollview_FX.RefreshScrollItem();
    }

    private void Callback_IsSpineBack( ClickEvent clickEvent, UI_ToggleBase toggleBase )
    {
        if(null == m_FxData)    return;

        bool isSelect = ! toggleBase.GetSelected();
        m_FxData.IsSpineBack = isSelect;
        toggleBase.SetSelected(isSelect);
        
        RefreshUI();
        
        if(null != m_FXGameObject)      m_FXGameObject.ChangeSpineIsBack();
    }

    private void Callback_IsBoneChase( ClickEvent clickEvent, UI_ToggleBase toggleBase )
    {
        if(null == m_FxData)    return;

        bool isSelect = ! toggleBase.GetSelected();
        m_FxData.IsBoneChase = isSelect;

        RefreshUI();
    }

    public void Callback_BoneSelect( UI_ButtonBase buttonBase )
    {
        if(null == m_FxData)    return;

        List<string> boneNames = new List<string>();
        Spine.Skeleton skeleton = Manager_SpineFX.Instance.GetSkeleton();

        if(null == skeleton) return;

        for(int i = 0; i < skeleton.Bones.Items.Length; i++)
        {
            boneNames.Add( skeleton.Bones.Items[i].ToString() );
        }

        if( m_FxData.SelectedBoneIndex >= skeleton.Bones.Items.Length ||
            m_FxData.SelectedBoneIndex < 0 )
        {
            m_FxData.SelectedBoneIndex = 0;
        }

        GUIPopupWindow popupWindow = EditorWindow.GetWindow(typeof(GUIPopupWindow)) as GUIPopupWindow;

        Vector2 pos = Vector2.zero;

        popupWindow.SetData( boneNames.ToArray(), 
                                m_FxData.SelectedBoneIndex, 
                                800, Color.gray, 50, 
                                pos, 800,
                                Callback_BoneSelectComplete);

        popupWindow.ShowPopup();
    }

    public void Callback_BoneSelectComplete(int index)
    {
        m_FxData.SelectedBoneIndex = index;

        Spine.Skeleton skeleton = Manager_SpineFX.Instance.GetSkeleton();
        m_FxData.ChasedBone = skeleton.Bones.Items[m_FxData.SelectedBoneIndex];
        RefreshUI();
    }

    public void Callback_StartTime( string startTime )
    {
        if(null == m_FxData)   return;

        float time;

        if(false == float.TryParse(startTime, out time))   return;

        m_FxData.FXNotiTime = time;

        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }

    public void Callback_EndTime( string endTime )
    {
        if(null == m_FxData)   return;

        float time;

        if(false == float.TryParse(endTime, out time))   return;

        m_FxData.FXDeleteTime = time;

        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }

    public void Callback_FX_PosX( string posX )
    {
        if(null == m_FxData)   return;

        float value;

        if(false == float.TryParse(posX, out value))   return;

        m_FxData.FXPosition = new Vector3( value, m_FxData.FXPosition.y, m_FxData.FXPosition.z );
        
        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }

    public void Callback_FX_PosY( string posY )
    {
        if(null == m_FxData)   return;

        float value;

        if(false == float.TryParse(posY, out value))   return;

        m_FxData.FXPosition = new Vector3( m_FxData.FXPosition.x, value, m_FxData.FXPosition.z );

        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }

    public void Callback_FX_PosZ( string posZ )
    {
        if(null == m_FxData)   return;

        float value;

        if(false == float.TryParse(posZ, out value))   return;

        m_FxData.FXPosition = new Vector3( m_FxData.FXPosition.x, m_FxData.FXPosition.y, value );

        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }
    
    public void Callback_FX_ScaleX( string scaleX )
    {
        if(null == m_FxData)   return;

        float value;

        if(false == float.TryParse(scaleX, out value))   return;

        m_FxData.FXScale = new Vector3( value, m_FxData.FXScale.y, m_FxData.FXScale.z );
        
        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }

    public void Callback_FX_ScaleY( string scaleY )
    {
        if(null == m_FxData)   return;

        float value;

        if(false == float.TryParse(scaleY, out value))   return;

        m_FxData.FXScale = new Vector3( m_FxData.FXScale.x, value, m_FxData.FXScale.z );

        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }

    public void Callback_FX_ScaleZ( string scaleZ )
    {
        if(null == m_FxData)   return;

        float value;

        if(false == float.TryParse(scaleZ, out value))   return;

        m_FxData.FXScale = new Vector3( m_FxData.FXScale.x, m_FxData.FXScale.y, value );

        if( null == m_FXGameObject )    return;

        m_FXGameObject.RefreshFXObject();
    }
#endif  // UNITY_EDITOR
}
