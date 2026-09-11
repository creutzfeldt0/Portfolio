using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Spine.Unity;
using Spine.Unity.Editor;
using System.Collections.Generic; 
using System;
using System.IO;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
#endif  // UNITY_EDITOR

/// <summary>
/// 이펙트와 스파인을 같이 보기위한 씬..
/// 스파인 캐릭터 안에 여러개의 애니메이션이 있고.
/// 그 애니메이션 마다 이펙트들을 달아볼 수 있어야 하며.
/// 이펙트 출력시간과 제거시간 및 위치 등등을 조절 할 수 있어야함.
/// 이펙트 시간설정은 랜덤 파티클 이펙트일 경우 유니티에서 지원해주지 않음.
/// FXData와 FXObject를 나눠 데이터 기준으로 오브젝트 안에 이펙트 생성.
/// </summary>
public class Scene_SpineTool : MonoBehaviour 
{
#if UNITY_EDITOR
    [SerializeField] private Transform                  m_Transform_Popups;
    [SerializeField] private Transform[]                m_Transform_FXs;
    [SerializeField] private UI_SpineObject             m_SpineObject;
    [SerializeField] private GameObject[]               m_GameScenes;
    [SerializeField] private Transform                  m_Transform_Export;
    
    private Dictionary<Type, UTPopup_Base>                m_Popups = new Dictionary<Type, UTPopup_Base>();

    private void OnDestroy() 
    {
        Window_FXTool.Instance.Close();

        m_Transform_Popups                  = null;

        m_Transform_FXs                     = null;

        m_SpineObject                       = null;
        m_GameScenes                        = null;

        m_Transform_Export                  = null;

        m_Popups.Clear();
        m_Popups                            = null;
    }

    private void Awake() 
    {
        Manager_SpineFX.Instance.Initialize(this);
        Window_FXTool.Instance.Initialize(this);
    }

    private void Start()
    {
        RefreshUI();
    }

    private void FixedUpdate() 
    {
        if( true == Window_FXTool.IsNullInstance() )    return;

        Window_FXTool.Instance.Repaint();    
    }

    private void RefreshUI()
    {
        VisualElement uiRoot =  Window_FXTool.Instance.GetUIRoot();

        UTPopup_Base[] popups = m_Transform_Popups.GetComponentsInChildren<UTPopup_Base>();

        for(int i=0; i<popups.Length; ++i)
        {
            m_Popups.Add( popups[i].GetType(), popups[i] );

            popups[i].InitVisualElement( uiRoot.Q<VisualElement>($"UXML_{popups[i].GetType()}") );
        }

        OpenPopup<Popup_EditorTabs>( this );
    }

    public Transform GetTransform_FXs(bool isSpineBack = true)
    {
        if( true == isSpineBack )
        {
            return m_Transform_FXs[0];
        }
        else
        {
            return m_Transform_FXs[1];
        }
    }

    public void OpenPopup<T>( object data = null ) where T : UTPopup_Base
    {
        UTPopup_Base popupBase = GetPopup<T>();

        if( null == popupBase ) return;
        
        popupBase.OpenPopup( data );
    }

    public T GetPopup<T>() where T : UTPopup_Base
    {
        UTPopup_Base popupBase = null;

         m_Popups.TryGetValue( typeof(T), out popupBase);

        return (T)popupBase;
    }

    public void OnLoadData_Complete()
    {
        for (int i = 0; i < m_Transform_FXs.Length; i++)
        {
            m_Transform_FXs[i].DestroyChildren();
        }

        OnRefreshSpine(); 
    }

    public void OnExportData()
    {
        //프리팹..
        SkeletonDataAsset dataAsset =  Manager_SpineFX.Instance.GetSkeletonDataAsset(); 

        if(null == dataAsset)   return;

        m_Transform_Export.DestroyChildren();

        var spawnMenuData = new SpineEditorUtilities.DragAndDropInstantiation.SpawnMenuData
        {
            skeletonDataAsset = dataAsset,
            spawnPoint = Vector3.zero,
            parent = m_Transform_Export,
            instantiateDelegate = (data) => EditorInstantiation.InstantiateSkeletonMecanim(data),
            isUI = false
        };

        SpineEditorUtilities.DragAndDropInstantiation.HandleSkeletonComponentDrop( spawnMenuData );

        if( 0 == m_Transform_Export.childCount ) return;

        Transform exportObjectTransform = m_Transform_Export.GetChild(0);
        GameObject exportObject = exportObjectTransform.gameObject;
        exportObject.layer = Manager_SpineFX.Instance.GetSpineLayer();
        SpineController spineController =  exportObject.AddComponent<SpineController>();
        SkeletonMecanim skeletonMecanim = exportObject.GetComponent<SkeletonMecanim>();
        
        spineController.Awake();    //AddComponent했는데 왜 Awake가 안돌아가지..

        exportObject.name = dataAsset.name.Replace("_SkeletonData", "");
        exportObjectTransform.localScale = Vector3.one * Manager_SpineFX.Instance.GetSpineScale();

        MeshRenderer renderer = exportObjectTransform.GetComponent<MeshRenderer>();

        if(null != renderer)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.allowOcclusionWhenDynamic  = false;
            renderer.sortingLayerName = Manager_SpineFX.Instance.GetSpineSortOrderLayerName();
        }

        Transform fxTransform = new GameObject("FXs").transform;
        fxTransform.SetParent(exportObjectTransform);
        fxTransform.localPosition = Vector3.zero;
        fxTransform.localScale = Vector3.one; 
        
        Transform spineFront = new GameObject("Front").transform;
        spineFront.SetParent(fxTransform);
        spineFront.localPosition = Vector3.zero;
        spineFront.localScale = Vector3.one;

        Transform spineBack = new GameObject("Back").transform;
        spineBack.SetParent(fxTransform);
        spineBack.localPosition = Vector3.zero;
        spineBack.localScale = Vector3.one;

        var fxDatas_All = Manager_SpineFX.Instance.GetFXDatas_All();
        FXData fxData;
        GameObject fxObject;
        GameObject fxPrefabObj;
        Transform fxObjectTransform;
        SpineFXObjBase spineFXObject;
        
        for (int i = 0; i < fxDatas_All.Count; i++)
        {
            fxData = fxDatas_All[i];

            if( null == fxData.FXPrefab )   continue;
                
            fxObject = new GameObject( $"FX_{i:D3}");
            fxObjectTransform = fxObject.transform;

            if( true == fxData.IsSpineBack )
            {
                fxObjectTransform.SetParent(spineBack);
                fxObjectTransform.localPosition = Vector3.zero;
                fxObjectTransform.localScale = Vector3.one;
                fxPrefabObj = (GameObject)PrefabUtility.InstantiatePrefab(fxData.FXPrefab, fxObjectTransform);
                fxPrefabObj.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
            }
            else
            {
                fxObjectTransform.SetParent(spineFront);
                fxObjectTransform.localPosition = Vector3.zero;
                fxObjectTransform.localScale = Vector3.one;
                fxPrefabObj = (GameObject)PrefabUtility.InstantiatePrefab(fxData.FXPrefab, fxObject.transform);
                fxPrefabObj.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
            }

            fxObjectTransform = fxPrefabObj.transform;

            switch ( (Manager_SpineFX.FXTYPE)fxData.FXType )
            {
                case Manager_SpineFX.FXTYPE.애니FX:
                    spineFXObject = fxObject.AddComponent<SpineFXObj_Ani>();
                    break;
                case Manager_SpineFX.FXTYPE.안지워짐:
                    spineFXObject = fxObject.AddComponent<SpineFXObj_Ani_UnDel>();
                    break;
                case Manager_SpineFX.FXTYPE.상시FX:
                    spineFXObject = fxObject.AddComponent<SpineFXObj_Const>();
                    break;
                case Manager_SpineFX.FXTYPE.상시스킨:
                    spineFXObject = fxObject.AddComponent<SpineFXObj_Const_Skin>();
                    break;
                default:
                    spineFXObject = fxObject.AddComponent<SpineFXObjBase>();
                    break;
            }
            
            spineFXObject.Init(fxData, skeletonMecanim, spineController.GetAnimator(), fxObjectTransform);
                
            spineController.GetFXs().Add(spineFXObject);
        }

        //FX 없으면 제거..
        if( 0 == spineFront.childCount )
        {
            DestroyImmediate( spineFront.gameObject );
        }
        if( 0 == spineBack.childCount )
        {
            DestroyImmediate( spineBack.gameObject );
        }  
        if( 0 == fxTransform.childCount )
        {
            DestroyImmediate( fxTransform.gameObject );
        }   

        string savePath = $"{Manager_SpineFX.Instance.GetSavePath()}/{exportObject.name}.prefab";

        PrefabUtility.SaveAsPrefabAsset( exportObject,  savePath );

        //문서..
        savePath = $"{Manager_SpineFX.Instance.GetSavePath()}/FXExportData.txt";

        StringBuilder exportText = new StringBuilder();

        CreateExportData_Line( ref exportText, "스파인 파일 경로", AssetDatabase.GetAssetPath(Manager_SpineFX.Instance.GetSkeletonDataAsset()) );
        CreateExportData_Line( ref exportText, "스파인 파일 Scale", exportObjectTransform.localScale.ToString() );

        var skeletonAni = Manager_SpineFX.Instance.GetSkeletonAnimation();
        var animations = skeletonAni.AnimationState.Data.SkeletonData.Animations;

        for( int j = 0; j < animations.Count;  ++j )
        {
            exportText.Append("/////////////////////////////////////////////////////////////////////////////////////////////\n");

            CreateExportData_Line( ref exportText, "애니메이션 이름", animations.Items[j].Name );
            CreateExportData_Line( ref exportText, "애니메이션 총 시간",  animations.Items[j].Duration.ToString() );
            CreateExportData_Line( ref exportText, "애니메이션 진행속도", skeletonAni.timeScale.ToString() );
        }
        
        exportText.Append("/////////////////////////////////////////////////////////////////////////////////////////////\n");

        for ( int i = 0; i < fxDatas_All.Count; ++i )
        {
            exportText.Append("================================================================\n");

            exportText.Append( $"FX_{i}\n" );
            CreateExportData_Line( ref exportText, "FX파일 경로", fxDatas_All[i].FXPath );
            CreateExportData_Line( ref exportText, "FX 출력시간", fxDatas_All[i].FXNotiTime.ToString() );
            CreateExportData_Line( ref exportText, "FX 종료시간", fxDatas_All[i].FXDeleteTime.ToString() );
            CreateExportData_Line( ref exportText, "FX 타입", ((Manager_SpineFX.FXTYPE)fxDatas_All[i].FXType).ToString() );
            CreateExportData_Line( ref exportText, "FX 스파인 뒤 출력여부", fxDatas_All[i].IsSpineBack.ToString() );
            CreateExportData_Line( ref exportText, "FX 본 추적여부", fxDatas_All[i].IsBoneChase.ToString() );

            if ( true == fxDatas_All[i].IsBoneChase )
            {
                if ( null == fxDatas_All[i].ChasedBone )      continue;

                CreateExportData_Line( ref exportText, "추적 본 이름", fxDatas_All[i].ChasedBone.ToString() );

                CreateExportData_Line( ref exportText, "FX 본으로부터 위치", fxDatas_All[i].FXPosition.ToString() );
                CreateExportData_Line( ref exportText, "FX Scale", fxDatas_All[i].FXScale.ToString() );

            }
            else
            {
                CreateExportData_Line( ref exportText, "FX Position", fxDatas_All[i].FXPosition.ToString() );
                CreateExportData_Line( ref exportText, "FX Scale", fxDatas_All[i].FXScale.ToString() );
            }
            exportText.Append("================================================================\n");
        }

        exportText.Append("/////////////////////////////////////////////////////////////////////////////////////////////\n");
        
        using (StreamWriter hWriter = new StreamWriter(savePath))
        {
            hWriter.Write( exportText.ToString() );

            hWriter.Close();
            hWriter.Dispose();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("작업내용 추출", "스파인프리팹 및 클라 전달용 문서가 생성 혹은 변경되었습니다.", "확인");
    }

    private void CreateExportData_Line( ref StringBuilder stringBuilder, string title, string data )
    {
        stringBuilder.Append(title);
        stringBuilder.Append(":\t\t");
        stringBuilder.Append(data);
        stringBuilder.Append("\n");
    }

    public void OnRefreshSpine()
    {   
        m_SpineObject.OnRefreshSpine();
    }

    public void OnTab_File()
    {
        Popup_File popupFile = GetPopup<Popup_File>();

        if(null == popupFile)   return;
        if( true == popupFile.IsOpen() )    return;

        popupFile.OpenPopup( this );

        Popup_Spine_Editor popupEditor = GetPopup<Popup_Spine_Editor>();

        if(null == popupEditor)   return;
        if( false == popupEditor.IsOpen() )    return;

        popupEditor.ClosePopup();
    }

    public void OnTab_Editor()
    {
        Popup_Spine_Editor popupEditor = GetPopup<Popup_Spine_Editor>();

        if(null == popupEditor)   return;
        if( true == popupEditor.IsOpen() )    return;

        popupEditor.OpenPopup( this );

        Popup_File popupFile = GetPopup<Popup_File>();

        if(null == popupFile)   return;
        if( false == popupFile.IsOpen() )    return;

        popupFile.ClosePopup();
    }

    public void OnTab_Scene()
    {
        byte sceneIndex = (byte)Manager_SpineFX.Instance.GetSceneType();

        for(int i=0;i<m_GameScenes.Length; ++i)
        {
            m_GameScenes[i].SetActive(false);
        }

        m_GameScenes[sceneIndex].SetActive(true);
    }
#endif //UnityEditor
}

