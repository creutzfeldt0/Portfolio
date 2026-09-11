

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Spine.Unity;
#endif //UNITY_EDITOR

public class Manager_SpineFX : Singleton <Manager_SpineFX>
{
#if UNITY_EDITOR
    public enum SceneTypes : byte
    {
        Scene_Empty = 0,
        Scene_Adventure,
        Scene_InGame,
    }
    
    public enum FXTYPE : int
    {
        애니FX = 0,
        안지워짐,
        상시FX,
        상시스킨,
        None,
    }

    private const string                                                            m_SaveFolderPath = "Assets/SpineFXEditor/SaveData";
    private const string                                                            m_SaveFileName = "FXSaveData";

    private Scene_SpineTool                                                         m_Scene_SpineTool;
    private SceneTypes                                                              m_SceneType = SceneTypes.Scene_Empty;
    private SkeletonDataAsset                                                       m_SkeletonDataAsset;
    private float                                                                   m_SpineScale = 1f;
    private SkeletonAnimation                                                       m_SkeletonAnimation;
    private string                                                                  m_AnimationName = string.Empty;
    private Spine.Skeleton                                                          m_Skeleton;
    private List<FXData>                                                            m_FXDatas_All = new List<FXData>();
    private Dictionary<string, List<FXData>>                                        m_FXDatas_Ani = new Dictionary<string, List<FXData>>();
    private List<FXData>                                                            m_FXDatas_Const = new List<FXData>();
    private Dictionary<string, List<FXData>>                                        m_FXDatas_Skin = new Dictionary<string, List<FXData>>();
    private int                                                                     m_SpineLayer = 0;
    private string                                                                  m_SpineSortingLayerName = "Default";


    protected override void OnDestroy()
    {
        m_SkeletonDataAsset             = null;
        m_SkeletonAnimation             = null;
        m_Skeleton                      = null;

        m_FXDatas_All.Clear();
        m_FXDatas_All                   = null;
    }

    protected override void Awake(){}

    public void Initialize( Scene_SpineTool scene )
    {
        m_Scene_SpineTool = scene;
    }

    public void OnSaveData()
    {
        string saveFileName = m_SaveFileName;

        if (null != m_SkeletonDataAsset)
        {
            saveFileName = m_SkeletonDataAsset.name.Replace("_SkeletonData", "SaveData");
        }
        
        string savePath = string.Format( "{0}/{1}.asset", m_SaveFolderPath, saveFileName );

        if (!Directory.Exists(m_SaveFolderPath))
        {
            Directory.CreateDirectory(m_SaveFolderPath);
        }
        
        var asset = ScriptableObject.CreateInstance( typeof( FXSaveData ) );
        FXSaveData sobj = (FXSaveData)asset;
        FXData_SaveObj fxSaveObj;

        sobj.m_SceneType = (byte)this.GetSceneType();
        sobj.m_SkeletonDataAsset = this.m_SkeletonDataAsset;
        sobj.m_SpineScale = this.m_SpineScale;
        sobj.m_FXObjects.Clear();
        sobj.m_FXDatas_All = string.Empty;

        for (int i = 0; i < m_FXDatas_All.Count; i++)
        {
            fxSaveObj = new FXData_SaveObj(m_FXDatas_All[i]);
            sobj.m_FXDatas_All += JsonUtility.ToJson( fxSaveObj ) + ';';
            sobj.m_FXObjects.Add( m_FXDatas_All[i].FXPrefab );
        }
        
        if( 0 < m_FXDatas_All.Count )
        {
            sobj.m_FXDatas_All = sobj.m_FXDatas_All.Remove( sobj.m_FXDatas_All.Length -1 );
        }
        
        AssetDatabase.CreateAsset(asset, savePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("스크립터블 오브젝트", "스크립터블 오브젝트가 생성 혹은 변경되었습니다.", "확인");
    }

    /// <summary>
    /// 세이브 데이터 로드. 콜백으론 부른 데이터로 객체생성..
    /// </summary>
    /// <param name="callbackLoad"></param>
    public void OnLoadData( Action callbackLoad  )
    {
        string loadPath = EditorUtility.OpenFilePanel("SaveData 파일을 선택해 주세요.",
            "Assets/SpineFXEditor/SaveData",
            "asset");

        if (String.IsNullOrEmpty(loadPath))     return;
        
        string[] assetPaths = loadPath.Split("/Assets/");
        loadPath = "Assets/" + assetPaths[1];
        
        FXSaveData sobj = UnityEditor.AssetDatabase.LoadAssetAtPath<FXSaveData>(loadPath);

        if( null == sobj)
        {
            EditorUtility.DisplayDialog("스크립터블 오브젝트", "스크립터블 오브젝트를 불러오는데 실패했습니다.", "확인");
            return;
        }

        SetSceneType( (SceneTypes)sobj.m_SceneType );

        m_SkeletonDataAsset = sobj.m_SkeletonDataAsset;

        m_SpineScale = sobj.m_SpineScale;

        m_FXDatas_All.Clear();
        m_FXDatas_Ani.Clear();
        m_FXDatas_Const.Clear();
        m_FXDatas_Skin.Clear();

        m_FXDatas_All = new List<FXData>();
        m_FXDatas_Ani = new Dictionary<string, List<FXData>>();
        m_FXDatas_Const = new List<FXData>();
        m_FXDatas_Skin = new Dictionary<string, List<FXData>>();
        
        if( false == string.IsNullOrEmpty( sobj.m_FXDatas_All ) ) 
        {
            string[] datas = sobj.m_FXDatas_All.Split(';');
            List<FXData> fxDatas;
            FXData_SaveObj fxSaveObj;
            FXData  fxData;

            for (int i = 0; i < datas.Length; ++i)
            {
                if ( true == string.IsNullOrEmpty( datas[i] ) ) continue; 
                
                fxSaveObj = JsonUtility.FromJson<FXData_SaveObj>(datas[i]);
                fxData = fxSaveObj.ConvertFXData(sobj.m_FXObjects[i]);
                
                m_FXDatas_All.Add( fxData );

                switch ( (FXTYPE)fxData.FXType )
                {
                    case FXTYPE.애니FX:
                    case FXTYPE.안지워짐:
                        if ( false == m_FXDatas_Ani.TryGetValue(fxData.FXTypeData, out fxDatas) )
                        {
                            fxDatas = new List<FXData>();
                            m_FXDatas_Ani.Add(fxData.FXTypeData, fxDatas);
                        }
                        fxDatas.Add( fxData );
                        break;

                    case FXTYPE.상시스킨:
                        if ( false == m_FXDatas_Skin.TryGetValue(fxData.FXTypeData, out fxDatas) )
                        {
                            fxDatas = new List<FXData>();
                            m_FXDatas_Skin.Add(fxData.FXTypeData, fxDatas);
                        }
                        fxDatas.Add( fxData );
                        break;
                    
                    //case FXTYPE.상시FX:
                    default:
                        m_FXDatas_Const.Add( fxData );
                        break;
                }
            }
        }

        callbackLoad?.Invoke();

        EditorUtility.DisplayDialog("스크립터블 오브젝트", "스크립터블 오브젝트를 불러오는데 성공했습니다.", "확인");
    }

    public void OnNewDataAsset(string path)
    {
        string extention = Path.GetExtension(path);

        if( extention != ".asset"  )    return;

        string[] assetPaths = path.Split("/Assets/");
        string assetPath = "Assets/" + assetPaths[1];

        m_SkeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);

        m_AnimationName = string.Empty;
        m_SkeletonAnimation = null;
        m_Skeleton = null;
        m_FXDatas_All.Clear();
    }

    public SkeletonDataAsset GetSkeletonDataAsset()
    {
        return m_SkeletonDataAsset;
    }

    public void SetSceneType( SceneTypes sceneType )
    {
        m_SceneType = sceneType;
    }

    public SceneTypes GetSceneType()
    {
        return m_SceneType;
    }

    public void SetSpineScale(float scale)
    {
        m_SpineScale = scale;
    }

    public float GetSpineScale()
    {
        return m_SpineScale;
    }

    public void SetSpineLayer(int layerIndex)
    {
        m_SpineLayer = layerIndex;
    }

    public int GetSpineLayer()
    {
        return m_SpineLayer;
    }

    public void SetSpineSortOrderLayerName( string name )
    {
        m_SpineSortingLayerName = name;
    }

    public string GetSpineSortOrderLayerName()
    {
        return m_SpineSortingLayerName;
    }

    public void SetSkeletonAni( SkeletonAnimation animation )
    {
        m_SkeletonAnimation = animation;

        if( string.IsNullOrEmpty( m_AnimationName ) )
        {
            m_SkeletonAnimation.AnimationName = m_SkeletonAnimation.AnimationState.Data.SkeletonData.Animations.Items[0].Name;
        }
        else
        {
            m_SkeletonAnimation.AnimationName = m_AnimationName;
        }

        m_SkeletonAnimation.timeScale = 0f;
        m_SkeletonAnimation.AnimationState.Data.DefaultMix = 0f;

        m_Skeleton = m_SkeletonAnimation.skeleton;

    }

    public SkeletonAnimation GetSkeletonAnimation()
    {
        return m_SkeletonAnimation; 
    }

    public Spine.Skeleton GetSkeleton()
    {
        return m_Skeleton; 
    }

    /// <summary>
    /// FX UI처음 생성했을 때 데이터도 미리 추가..
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fxPrefab"></param>
    /// <returns></returns>
    public FXData AddFXData( string path, GameObject fxPrefab )
    {
        List<FXData> fXDatas = null;
        FXData fxData = null;

        if( null == GetSkeletonAnimation() )    return null;
        
        fxData = new FXData();
        fxData.Index = m_FXDatas_All.Count;
        fxData.FXName = Path.GetFileNameWithoutExtension(path);
        fxData.FXPath = path;
        fxData.FXPrefab = fxPrefab;
        fxData.FXType = (int) FXTYPE.애니FX;
        fxData.FXTypeData = GetSkeletonAnimation().AnimationName;
        
        m_FXDatas_All.Add(fxData);
        
        if( false == m_FXDatas_Ani.TryGetValue( GetSkeletonAnimation().AnimationName, out fXDatas))
        {
            fXDatas = new List<FXData>();
            m_FXDatas_Ani.Add( GetSkeletonAnimation().AnimationName, fXDatas );
        }
        
        fXDatas.Add( fxData );

        return fxData;
    }

    public FXData ReplaceFXData( string path, GameObject fxPrefab, FXData fxData )
    {
        if( null == GetSkeletonAnimation() )    return null;

        if (fxData.Index >= m_FXDatas_All.Count) return null;

        List<FXData> fxDatas;
        int index;
        
        //FX 타입이 달라졌을 때..
        if (m_FXDatas_All[fxData.Index].FXType != fxData.FXType)
        {
            //교체되기 전 이전 데이터 제거..
            switch ( (FXTYPE)m_FXDatas_All[fxData.Index].FXType )
            {
                case FXTYPE.애니FX:
                case FXTYPE.안지워짐:
                    if ( true == m_FXDatas_Ani.TryGetValue(m_FXDatas_All[fxData.Index].FXTypeData, out fxDatas) )
                    {
                        index = fxDatas.IndexOf(m_FXDatas_All[fxData.Index]);

                        if (-1 != index)
                        {
                            fxDatas.RemoveAt(index);
                        }
                    }
                    break;
                
                case FXTYPE.상시스킨:
                    if ( true == m_FXDatas_Skin.TryGetValue(m_FXDatas_All[fxData.Index].FXTypeData, out fxDatas) )
                    {
                        index = fxDatas.IndexOf(m_FXDatas_All[fxData.Index]);

                        if (-1 != index)
                        {
                            fxDatas.RemoveAt(index);
                        }
                    }
                    break;
                
                //case FXTYPE.상시FX:
                default:
                    index = m_FXDatas_Const.IndexOf(m_FXDatas_All[fxData.Index]);
                    if (-1 != index)
                    {
                        m_FXDatas_Const.RemoveAt( index );
                    }
                    break;
            }
            
            //데이터 추가..
            switch ( (FXTYPE)fxData.FXType )
            {
                case FXTYPE.애니FX:
                case FXTYPE.안지워짐:
                    fxData.FXTypeData = m_AnimationName;    //애니메이션 이름으로 기록..
                    
                    if ( false == m_FXDatas_Ani.TryGetValue(fxData.FXTypeData, out fxDatas) )
                    {
                        fxDatas = new List<FXData>();
                        m_FXDatas_Ani.Add( fxData.FXTypeData, fxDatas );
                    }
                    
                    fxDatas.Add(fxData);

                    break;
                
                case FXTYPE.상시스킨:
                    fxData.FXTypeData = m_Skeleton.Skin.Name;   //스킨명으로 기록..
                    
                    if ( false == m_FXDatas_Skin.TryGetValue(fxData.FXTypeData, out fxDatas) )
                    {
                        fxDatas = new List<FXData>();
                        m_FXDatas_Skin.Add( fxData.FXTypeData, fxDatas );
                    }
                    fxDatas.Add(fxData);
                    
                    break;
                
                //case FXTYPE.상시FX:
                default:
                    m_FXDatas_Const.Add( fxData );
                    break;
            }
        }
        else         //FX 타입이 같을 때..
        {
            switch ( (FXTYPE)m_FXDatas_All[fxData.Index].FXType )
            {
                case FXTYPE.애니FX:
                case FXTYPE.안지워짐:
                    if ( true == m_FXDatas_Ani.TryGetValue(m_FXDatas_All[fxData.Index].FXTypeData, out fxDatas) )
                    {
                        index = fxDatas.IndexOf(m_FXDatas_All[fxData.Index]);

                        if (-1 != index)
                        {
                            fxDatas[index] = fxData;
                        }
                    }
                    break;
                
                case FXTYPE.상시스킨:
                    if ( true == m_FXDatas_Skin.TryGetValue(m_FXDatas_All[fxData.Index].FXTypeData, out fxDatas) )
                    {
                        index = fxDatas.IndexOf(m_FXDatas_All[fxData.Index]);

                        if (-1 != index)
                        {
                            fxDatas[index] = fxData;
                        }
                    }
                    break;
                
                //case FXTYPE.상시FX:
                default:
                    index = m_FXDatas_Const.IndexOf(m_FXDatas_All[fxData.Index]);
                    if (-1 != index)
                    {
                        m_FXDatas_Const[index] = fxData;
                    }
                    break;
            }
            
        }
        
        fxData.FXName = Path.GetFileNameWithoutExtension(path);
        fxData.FXPath = path;
        fxData.FXPrefab = fxPrefab;
        m_FXDatas_All[fxData.Index] = fxData;
        
        return fxData;
    }
    
    public void DeleteFXData( int index )
    {
        List<FXData> fxDatas = null;
        int deleteIndex;
        
        switch ( (FXTYPE)m_FXDatas_All[index].FXType )
        {
            case FXTYPE.애니FX:
            case FXTYPE.안지워짐:
                if ( true == m_FXDatas_Ani.TryGetValue(m_FXDatas_All[index].FXTypeData, out fxDatas) )
                {
                    deleteIndex = fxDatas.IndexOf(m_FXDatas_All[index]);

                    if (-1 != deleteIndex)
                    {
                        fxDatas.RemoveAt(deleteIndex);
                    }
                }
                break;
                
            case FXTYPE.상시스킨:
                if ( true == m_FXDatas_Skin.TryGetValue(m_FXDatas_All[index].FXTypeData, out fxDatas) )
                {
                    deleteIndex = fxDatas.IndexOf(m_FXDatas_All[index]);

                    if (-1 != deleteIndex)
                    {
                        fxDatas.RemoveAt(deleteIndex);
                    }
                }
                break;
                
            //case FXTYPE.상시FX:
            default:
                deleteIndex = m_FXDatas_Const.IndexOf(m_FXDatas_All[index]);
                
                if (-1 != deleteIndex)
                {
                    m_FXDatas_Const.RemoveAt(deleteIndex);
                }
                break;
        }
        
        m_FXDatas_All.RemoveAt(index);
        
        for(int i =0; i<m_FXDatas_All.Count; ++i)
        {
            m_FXDatas_All[i].Index = i;
        }
    }

    public void ChangeTypeFXData( int targetTypeNum, FXData fxData )
    {
        if (targetTypeNum == fxData.FXType) return;

        if ( ( targetTypeNum == (int)FXTYPE.애니FX || targetTypeNum == (int)FXTYPE.안지워짐 ) &&
             ( fxData.FXType == (int)FXTYPE.애니FX || fxData.FXType == (int)FXTYPE.안지워짐 ) )
        {
            fxData.FXType = targetTypeNum;
            return;
        }
        
        FXTYPE targetType = (FXTYPE) targetTypeNum;
        List<FXData> fxDatas;
        int index;
        
        switch ( (FXTYPE)fxData.FXType )
        {
            case FXTYPE.애니FX:
            case FXTYPE.안지워짐:
                if ( true == m_FXDatas_Ani.TryGetValue(fxData.FXTypeData, out fxDatas) )
                {
                    index = fxDatas.IndexOf(fxData);

                    if (-1 != index)
                    {
                        fxDatas.RemoveAt(index);
                    }
                }
                break;
                
            case FXTYPE.상시스킨:
                if ( true == m_FXDatas_Skin.TryGetValue(m_FXDatas_All[fxData.Index].FXTypeData, out fxDatas) )
                {
                    index = fxDatas.IndexOf(fxData);

                    if (-1 != index)
                    {
                        fxDatas.RemoveAt(index);
                    }
                }
                break;
                
            //case FXTYPE.상시FX:
            default:
                index = m_FXDatas_Const.IndexOf(fxData);
                if (-1 != index)
                {
                    m_FXDatas_Const.RemoveAt( index );
                }
                break;
        }
        
        //데이터 추가..
        switch ( targetType )
        {
            case FXTYPE.애니FX:
            case FXTYPE.안지워짐:
                fxData.FXTypeData = m_SkeletonAnimation.AnimationName;  //애니명으로 기록..
                
                if ( false == m_FXDatas_Ani.TryGetValue(fxData.FXTypeData, out fxDatas) )
                {
                    fxDatas = new List<FXData>();
                    m_FXDatas_Ani.Add( fxData.FXTypeData, fxDatas );
                }
                fxDatas.Add(fxData);
                break;
            
            case FXTYPE.상시스킨:
                fxData.FXTypeData = m_Skeleton.Skin.Name;   //스킨명으로 기록..
                
                if ( false == m_FXDatas_Skin.TryGetValue(fxData.FXTypeData, out fxDatas) )
                {
                    fxDatas = new List<FXData>();
                    m_FXDatas_Skin.Add( fxData.FXTypeData, fxDatas );
                }
                fxDatas.Add(fxData);
                break;
            
            //case FXTYPE.상시FX:
            default:
                m_FXDatas_Const.Add( fxData );
                break;
        }

        fxData.FXType = targetTypeNum;
    }

    public List<FXData> GetFXDatas_All()
    {
        return m_FXDatas_All;
    }
    
    public List<FXData> GetFXDatas_Ani( string aniName )
    {
        List<FXData> fxDatas;

        m_FXDatas_Ani.TryGetValue(aniName, out fxDatas);
        
        return fxDatas;
    }
    
    public List<FXData> GetFXDatas_Skin( string skinName )
    {
        List<FXData> fxDatas;

        m_FXDatas_Skin.TryGetValue(skinName, out fxDatas);
        
        return fxDatas;
    }

    public FXData GetFXData( int index )
    {
        if( index < m_FXDatas_All.Count )
        {
            return m_FXDatas_All[index];
        }
        else
        {
            return null;
        }
    }

    public Scene_SpineTool GetScene()
    {
        return m_Scene_SpineTool;
    }

    public string GetSavePath()
    {
        return m_SaveFolderPath;
    }
#else
    protected override void OnDestroy()
    {
    }
#endif //UNITY_EDITOR
}
