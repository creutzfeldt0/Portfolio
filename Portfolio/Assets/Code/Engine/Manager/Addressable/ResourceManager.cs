using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

public enum ResourceType 
{
    Common_Remote,
    NeedUnload,
}

public class ResourceManager : MonoBehaviour 
{
    static private ResourceManager _instance;
    static public ResourceManager instance => _instance;

    private void Awake() 
    {
        if (_instance == null)  _instance = this;
    }

    private Dictionary<string, Object> m_CommonObjByPath = new Dictionary<string, Object>();
    private List<AsyncOperationHandle> m_LoadedHandles = new List<AsyncOperationHandle>();
    private List<string> m_CachedKeyList = new List<string>();
    private List<Task> m_LoadTasks = new List<Task>();
    private static readonly ResourceType[] m_ResourceTypes = (ResourceType[])System.Enum.GetValues(typeof(ResourceType));
    private readonly Dictionary<string, Dictionary<System.Type, Dictionary<string, IResourceLocation>>> m_LocationCacheByLabel = new Dictionary<string, Dictionary<System.Type, Dictionary<string, IResourceLocation>>>();

    private const string m_Label_Common_Remote = "Common_Remote";

#region 다운로드 할 사이즈 체크
    public async Task<double> CheckDownloadCapacity()
    {
        try
        {
            long downloadSizeBytes = 0;
            List<Task<long>> sizeTasks = new List<Task<long>>();
            List<AsyncOperationHandle<long>> sizeHandles = new List<AsyncOperationHandle<long>>();

            // 1. 모든 라벨의 사이즈 체크를 병렬(동시)로 요청
            for(int i = 0; i < m_ResourceTypes.Length; i++)
            {
                ResourceType type = m_ResourceTypes[i];

                var sizeHandle = Addressables.GetDownloadSizeAsync(type.ToString());
                sizeHandles.Add(sizeHandle);
                sizeTasks.Add(sizeHandle.Task);
            }

            // 2. 한 번에 대기
            long[] results = await Task.WhenAll(sizeTasks);
            for(int i = 0; i < results.Length; i++)
            {
                downloadSizeBytes += results[i];
                Addressables.Release(sizeHandles[i]); // 대기 완료 후 일괄 해제
            }

            if (downloadSizeBytes > 0)
            {
                double downloadSizeMegabytes = (double)downloadSizeBytes / (1024 * 1024);
                return downloadSizeMegabytes;
            }
            
            Debug.Log("[Addressables] 이미 최신 상태입니다. 바로 에셋을 로드합니다.");
            return 0;
        }
        catch ( System.Exception e )
        {
            Debug.LogError($"[Addressables] 에러 발생: {e.Message}");
            return -1;
        }
    }
#endregion 다운로드 할 사이즈 체크

#region 메모리에 들고 있을 리소스

    /// <summary>
    /// Common 그룹에 있는 에셋들은 게임 켜질 때 전부 로드해서 메모리에 올려놓는다 //
    /// </summary>
    /// <returns></returns>
    public async Task LoadCommonResource( Action<float> onProgressChanged )
    {
        Debug.Log($"  [LoadCommonResource] 로딩 시작 : {Time.realtimeSinceStartup}");
        
        onProgressChanged?.Invoke(0f);

        var generalLocHandle_R = Addressables.LoadResourceLocationsAsync(m_Label_Common_Remote);
        var spriteLocHandle_R = Addressables.LoadResourceLocationsAsync(m_Label_Common_Remote, typeof(Sprite));

        await Task.WhenAll( generalLocHandle_R.Task, spriteLocHandle_R.Task);

        var generalLocations_R = generalLocHandle_R.Result;
        var spriteLocations_R = spriteLocHandle_R.Result;

        Addressables.Release(generalLocHandle_R);
        Addressables.Release(spriteLocHandle_R);

        Debug.Log($"  [LoadCommonResource] Location Load 완료 : {Time.realtimeSinceStartup}");

        List<(IResourceLocation loc, AsyncOperationHandle handle, bool isSprite)> taskMapping = new List<(IResourceLocation, AsyncOperationHandle, bool)>();

        AddTask(generalLocations_R, spriteLocations_R, ref taskMapping);        

        Debug.Log($"  [LoadCommonResource] 병렬 에셋 로드 시작 (총 {m_LoadTasks.Count}건) : {Time.realtimeSinceStartup}");

        await m_LoadTasks.WhenAllWithProgress(progressPercent =>
        {
            onProgressChanged.Invoke(progressPercent);
        });

        Debug.Log($"  [LoadCommonResource] 실제 로딩 완료 : {Time.realtimeSinceStartup}");

        m_LoadTasks.Clear();
        m_CommonObjByPath.Clear();
        m_CommonObjByPath.EnsureCapacity( m_CommonObjByPath.Count + (m_CommonObjByPath.Count/2) );

        // 결과물 딕셔너리 적재 //
        int mappingCount = taskMapping.Count;
        int spriteCount = 0;

        for (int i = 0; i < mappingCount; i++)
        {
            var item = taskMapping[i];

            if (item.handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LoadCommonResource] 로드 실패: {item.loc.PrimaryKey}");
                Addressables.Release(item.handle);
                continue;
            }

            if (item.isSprite) 
            {
                IList<Sprite> sprites = item.handle.Result as IList<Sprite>;

                if (null == sprites)    continue;

                spriteCount = sprites.Count;

                string primeryKey = item.loc.PrimaryKey;

                if (1 == spriteCount)
                {
                    m_CommonObjByPath[primeryKey] = sprites[0];
                }
                else
                {
                    for (int j = 0; j < spriteCount; j++)
                    {
                        m_CommonObjByPath[$"{primeryKey}[{sprites[j].name}]"] = sprites[j];
                    }
                }
            }
            else 
            {
                m_CommonObjByPath[item.loc.PrimaryKey] = item.handle.Result as Object;
            }
        }

        m_CachedKeyList.Clear();
        Debug.Log($"  [LoadCommonResource] 데이터 저장 완료 : {Time.realtimeSinceStartup}");
    }

    /// <summary>
    /// 에셋 로드 태스크 생성 ///
    /// 속도를 위해 최대한 foreach 없이 for문으로 처리 ///
    /// </summary>
    /// <param name="locations_gen"></param>
    /// <param name="locations_sprite"></param>
    private void AddTask( IList<IResourceLocation> locations_gen, 
                          IList<IResourceLocation> locations_sprite,
                          ref List<(IResourceLocation loc, AsyncOperationHandle handle, bool isSprite)> taskMapping )
    {
        HashSet<string> spritePrimaryKeys = new HashSet<string>();
        int spriteCount = locations_sprite.Count;

        for (int i = 0; i < spriteCount; i++)
        {
            IResourceLocation loc = locations_sprite[i];

            spritePrimaryKeys.Add(loc.PrimaryKey);

            var handle = Addressables.LoadAssetAsync<IList<Sprite>>(loc);
            taskMapping.Add((loc, handle, true));
            m_LoadTasks.Add(handle.Task);
        }

        int generalCount = locations_gen.Count;

        for (int i = 0; i < generalCount; i++)
        {
            IResourceLocation loc = locations_gen[i];
            if (true == spritePrimaryKeys.Contains(loc.PrimaryKey))     continue; 

            var handle = Addressables.LoadAssetAsync<Object>(loc);
            taskMapping.Add((loc, handle, false));
            m_LoadTasks.Add(handle.Task);
        }
    }

    /// <summary>
    /// 처음 게임 켰을 때 로딩하는 리소스들을 부를 때 사용 ///
    /// 게임이 꺼질때까지 언로드 되지 않는다 ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T GetResource<T>(string path, string subSpriteName = "") where T : Object
    {
        if(typeof(T) == typeof(Sprite))
        {
            if (false == string.IsNullOrEmpty(subSpriteName)) 
            {
                path = $"{path}[{subSpriteName}]";
            }
        }

        m_CommonObjByPath.TryGetValue(path, out Object data);
        return data as T;
    }

    public Sprite LoadSprite(string path) 
    {
        if (string.IsNullOrEmpty(path))     return null;

        string spritePath;
        string subSpriteName = "";

        // 멀티스프라이트의 경우 ex) path = "Hero/Hero_1:GamePlay" //
        string[] parts = path.Split(':');

        if (parts.Length == 2)
        {
            spritePath = $"Assets/Data/Common_Remote/{parts[0]}.png";  //"Assets/Data/Common_Remote/Hero/Hero_1.png"//
            subSpriteName = parts[1]; //GamePlay//
        }
        else
        {
            spritePath = $"Assets/Data/Common_Remote/{path}.png";
        }

        return GetResource<Sprite>(spritePath, subSpriteName);
    }

#endregion 메모리에 들고 있을 리소스

#region 사용 후 지워 줄 리소스

    /// <summary>
    /// Common을 제외한 다른 어드레서블 그룹은 다운로드만 받고 메모리에 올리지는 않는다//
    /// </summary>
    /// <returns></returns>
    public async Task DownloadOtherLabels()
    {
        Debug.Log($"  [DownloadOtherLabels] 로딩 시작 : {Time.realtimeSinceStartup}");

        List<Task> downloadTasks = new List<Task>();
        List<AsyncOperationHandle> downloadHandles = new List<AsyncOperationHandle>();
        AsyncOperationHandle downloadHandle = default;

        for(int i=0; i<m_ResourceTypes.Length; i++)
        {
            ResourceType type = (ResourceType)m_ResourceTypes.GetValue(i);

            if( type == ResourceType.Common_Remote )    continue;            

            downloadHandle = Addressables.DownloadDependenciesAsync( type.ToString() );

            downloadTasks.Add(downloadHandle.Task);
            downloadHandles.Add(downloadHandle);
        }

        await Task.WhenAll(downloadTasks);

        for(int i=0; i<downloadHandles.Count; i++)
        {
            if( downloadHandles[i].Status != AsyncOperationStatus.Succeeded )
            {
                Debug.LogError($"[DownloadOtherLabels] 다운로드 실패: {downloadHandles[i].DebugName}");
            }
            Addressables.Release(downloadHandles[i]);
        }

        Debug.Log($"  [DownloadOtherLabels] 데이터 다운 완료 : {Time.realtimeSinceStartup}");
    }

    private async Task<IResourceLocation> CacheLocationMapAsync<T>(string labelName, string path) where T : Object
    {
        IResourceLocation cachedLocation = null;

        if ( m_LocationCacheByLabel.ContainsKey(labelName) &&
             m_LocationCacheByLabel[labelName].ContainsKey(typeof(T)) ) 
        {
            m_LocationCacheByLabel[labelName][typeof(T)].TryGetValue(path, out cachedLocation);
            return cachedLocation;
        }

        var locHandle = Addressables.LoadResourceLocationsAsync(labelName, typeof(T));
        var locations = await locHandle.Task;

        if (locations == null || locations.Count == 0)
        {
            Addressables.Release(locHandle);
            return null;
        }

        if (false == m_LocationCacheByLabel.ContainsKey(labelName))
        {
            m_LocationCacheByLabel[labelName] = new Dictionary<Type, Dictionary<string, IResourceLocation>>();
        }

        if (false == m_LocationCacheByLabel[labelName].ContainsKey(typeof(T)))
        {
            m_LocationCacheByLabel[labelName][typeof(T)] = new Dictionary<string, IResourceLocation>( locations.Count );
        }

        var typeMap = m_LocationCacheByLabel[labelName];
        var innerMap = typeMap[typeof(T)];
        List<IResourceLocation> locList = locations as List<IResourceLocation> ?? new List<IResourceLocation>(locations);

        for (int i = 0; i < locList.Count; i++)
        {
            IResourceLocation location = locList[i];
            System.Type resourceType = location.ResourceType;

            // Path.ChangeExtension을 대체하여 가비지 발생 최소화
            string cleanKey = RemoveExtensionFast(location.PrimaryKey);
            innerMap[cleanKey] = location;
        }
        
        Addressables.Release(locHandle);

        innerMap.TryGetValue(path, out cachedLocation);

        return cachedLocation;
    }

    /// <summary>
    /// 문자열 자르기 함수, 가비지 최소화 ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string RemoveExtensionFast(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        int dotIndex = path.LastIndexOf('.');
        int slashIndex = System.Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));

        if (dotIndex > slashIndex)
        {
            return path.AsSpan(0, dotIndex).ToString(); 
        }

        return path;
}
/// <summary>
/// 게임 진행중에 불러오는 리소스들을 부를 때 사용 ///
/// UnloadResourceAll 함수로 언로드 될 때까지 메모리에 남아있다 ///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="type"></param>
/// <param name="path"></param>
/// <returns></returns>
    public async Task<T> LoadResource<T>(ResourceType type, string path) where T : Object
    {
        if( type == ResourceType.Common_Remote )
        {
            return GetResource<T>(path);
        }

        string labelName = type.ToString();
        string subSpriteName = "";

        if( false == path.Contains("Assets/Data/") )
        {
            if( typeof(T) == typeof(Sprite))
            {
                int splitIndex = path.IndexOf(':');
                if (splitIndex != -1)
                {
                    subSpriteName = path.Substring(splitIndex + 1);
                    path = path.Substring(0, splitIndex);
                }
            }

            path = $"Assets/Data/{labelName}/{path}";
        }

        IResourceLocation filteredLocation = null;

        if ( false == m_LocationCacheByLabel.TryGetValue(labelName, out var typeMap) || 
             false == typeMap.TryGetValue(typeof(T), out var innerMap) || 
             false == innerMap.TryGetValue(path, out filteredLocation) )
        {
            // 혹시 사전 캐싱이 안 되어있을 때를 대비한 예외 처리(Fallback)
            filteredLocation = await CacheLocationMapAsync<T>(labelName, path);
        }

        if(null == filteredLocation)
        {
            Debug.LogError($"[LoadResource] {path} Location을 찾을 수 없습니다.");
            return null;
        }

        T loadedAsset = null;

        if(typeof(T) == typeof(Sprite))
        {
            var loadHandle = Addressables.LoadAssetAsync<IList<Sprite>>(filteredLocation);
            var sprites = await loadHandle.Task;

            if(loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LoadResource] {path} Asset Load 실패");
                Addressables.Release(loadHandle);
                return null;
            }

            m_LoadedHandles.Add(loadHandle); 

            int spriteCount = sprites.Count;
            if (1 == spriteCount)
            {
                loadedAsset = sprites[0] as T;
            }
            else
            {
                for (int i = 0; i < spriteCount; i++)
                {
                    if(sprites[i].name.Equals(subSpriteName, System.StringComparison.Ordinal))
                    {
                        loadedAsset = sprites[i] as T;
                        break;
                    }
                }
            }
        }
        else
        {
            var loadHandle = Addressables.LoadAssetAsync<T>(filteredLocation);
            loadedAsset = await loadHandle.Task;

            if(loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LoadResource] {path} Asset Load 실패");
                Addressables.Release(loadHandle);
                return null;
            }

            m_LoadedHandles.Add(loadHandle);
        }
        
        return loadedAsset;
    }

/// <summary>
/// 리소스 사용 (Instantiate등등) 후 모아서 언로드 바로 해주거나 씬 끝날때 SafeCode로 한번 씩 해주면 좋을듯 ///
/// </summary>
    public void UnloadResourceAll()
    {
        for(int i=0; i<m_LoadedHandles.Count; i++)
        {
            Addressables.Release(m_LoadedHandles[i]);
        }
        m_LoadedHandles.Clear();
    }
#endregion 사용 후 지워 줄 리소스
}
