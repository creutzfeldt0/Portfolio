using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AssetbundlePatchData
{
    public string Name = string.Empty;
    public uint uCRC = 0;
    public string Byte = string.Empty;
    public uint UpVersion = 0;
    public bool bChecked = false;
}

public enum BUNDLE_TYPE : int
{
    NON_ASSET = 0,                  // 리소시즈 폴더 그대로 사용,
    ASSET,                          // 에셋번들.
}

public class ResourceMgr : Singleton<ResourceMgr>
{
    private Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();
    private string m_RootAssetBundleDirectory = string.Format("file:///{0}/../../AssetBundles/Android", Application.dataPath);

    private List<AssetbundlePatchData> m_PatchData = new List<AssetbundlePatchData>();
    private string m_PatchfileVersion = string.Empty;
    private string m_CurVersion = string.Empty;
    private uint m_uCurVersion = 0;
    private long m_lDownloadBytes = 0;

    private float m_fProgress = 0f;
    protected override void OnDestroy()
    {
        foreach (AssetBundle bundle in m_AssetBundles.Values)
        {
            bundle.Unload(true);
        }
        m_AssetBundles.Clear();
        m_AssetBundles = null;

        m_PatchData.Clear();
        m_PatchData = null;
    }

    protected override void Awake()
    {
        m_PatchData.Clear();

        string strDefaultVersion = "0.0.1";
        m_CurVersion = PlayerPrefs.GetString( PlayerPrefNames.BundleVersion, strDefaultVersion );

        // 빈값이 있는 경우가 있음
        if (string.IsNullOrEmpty(m_CurVersion) == true)
            m_CurVersion = strDefaultVersion;

        m_uCurVersion = uint.Parse(m_CurVersion.Replace(".", ""));
    }

    #region AssetbundleLoading
    public IEnumerator LoadPatchFile()
    {
        if (0 != m_PatchData.Count)
            yield break;

        string patchFile = string.Format("{0}/PatchList.txt", m_RootAssetBundleDirectory);
        string text = string.Empty;

        yield return new ThreadForWait( () => { text = File.ReadAllText( patchFile ); } );

        if (true == string.IsNullOrEmpty(text))
        {
            // 실패시 재시작 여부 팝업..
            Debug.LogError("[ResourceManager] LoadPatchFile Error" );
            yield break;
        }

        using (StringReader hReader = new StringReader( text ))
        {
            AssetbundlePatchData data;
            m_PatchfileVersion = hReader.ReadLine();
            string line = hReader.ReadLine();
            string[] lineData;

            while (null != line)
            {
                lineData = line.Split('\t');

                data = new AssetbundlePatchData();
                data.Name = lineData[0];
                data.uCRC = uint.Parse(lineData[1]);
                data.Byte = lineData[2];
                data.UpVersion = uint.Parse(lineData[3]);
                data.bChecked = false;

                m_PatchData.Add(data);

                if (m_uCurVersion < data.UpVersion)
                {
                    //다운로드 예상 용량 노티를 위해..
                    m_lDownloadBytes += long.Parse(data.Byte);
                }

                line = hReader.ReadLine();
            }

            hReader.Close();
            hReader.Dispose();
        }
    }

    //로딩되는 스테이지에서 LoadPatchFile() 완료후 불리도록..
    public IEnumerator DownloadAllAssets()
    {
        string wepAssetbundlePath = string.Empty;

        for (int i = 0; i < m_PatchData.Count; ++i)
        {
            wepAssetbundlePath = string.Format("{0}/{1}/{2}", m_RootAssetBundleDirectory, m_PatchfileVersion, m_PatchData[i].Name);

            //버전이 0일땐 캐시에서 제거한다..(테스트 필요)
            if (0 == m_PatchData[i].UpVersion)
            {
                UnityWebRequest webRequest = UnityWebRequest.Delete(wepAssetbundlePath);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    //실패시 재시작 여부 팝업..
                    Debug.LogError("[ResourceManager] DownloadAllAssets Delete Error:" + webRequest.error);
                    yield break;
                }

                continue;
            }

            using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(wepAssetbundlePath, m_PatchData[i].UpVersion, m_PatchData[i].uCRC))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError )
                {
                    //실패시 재시작 여부 팝업..
                    Debug.LogError("[ResourceManager] DownloadAllAssets Error:" + webRequest.error);
                    yield break;
                }

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
                m_AssetBundles.Add( m_PatchData[i].Name, bundle );
            }

            m_fProgress = (float)(i + 1) / (float)m_PatchData.Count;
        }

        PlayerPrefs.SetString( PlayerPrefNames.BundleVersion, m_PatchfileVersion);
    }

    public IEnumerator LoadInGameAssetBundle(string name)
    {
#if ASSETBUNDLE
        string wepAssetbundlePath = string.Empty;

        string assetName = string.Format("ingame_{0}", name.ToLower());
     
        wepAssetbundlePath = string.Format("{0}/{1}/{2}", m_RootAssetBundleDirectory, m_PatchfileVersion, assetName);

        using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(wepAssetbundlePath))
        {
            yield return webRequest.SendWebRequest();

            if (true == webRequest.isNetworkError ||
            true == webRequest.isHttpError)
            {
                //TODO: 실패시 재시작 여부 팝업..
                Debug.LogError("[ResourceManager] DownloadAllAssets Error:" + webRequest.error);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);

            AssetBundleRequest request = bundle.LoadAllAssetsAsync();

            while (false == request.isDone)
            {
                yield return true;
            }

            Object[] assets = request.allAssets;
            string[] names = bundle.GetAllAssetNames();
            string tag;
            Dictionary<string, Object> objs;

            for (int count = 0; count < assets.Length; ++count)
            {
                tag = Path.GetExtension(names[count]);

                if (false == m_InGameAssets.TryGetValue(tag, out objs))
                {
                    objs = new Dictionary<string, Object>();
                    m_InGameAssets.Add(tag, objs);
                }

                objs.Add(assets[count].name.ToLower(), assets[count]);
            }

            bundle.Unload(false);

            //m_fProgress = (float)(i + 1) / (float)m_PatchData.Count;
        }
#endif //ASSETBUNDLE
        yield return true;
    }

    public IEnumerator UnloadInGameAssetBundle()
    {
#if ASSETBUNDLE
        foreach(Dictionary<string, Object> objs in m_InGameAssets.Values)
        {
            objs.Clear();
        }
        m_InGameAssets.Clear();
#endif //ASSETBUNDLE
        yield return true;
    }

    public IEnumerator LoadStoryAssetBundle()
    {
        yield return LoadPatchFile();

        string wepAssetbundlePath = string.Empty;
    }
    #endregion

    #region GetResource
    private T GetAsset<T>(string path) where T : Object
    {
        string fileName = Path.GetFileName(path).ToLower();

        string[] dirs = Path.GetDirectoryName(path).Split('/');

        if( 1 > dirs.Length ) return null;

        string bundleName = dirs[ dirs.Length-1 ].ToLower();

        AssetBundle bundle = m_AssetBundles[bundleName];

        return bundle.LoadAsset<T>(fileName);
    }

    public T Load<T>(BUNDLE_TYPE type, string path ) where T : Object
    {
#if ASSETBUNDLE
        if (type == BUNDLE_TYPE.ASSET)
        {
            return GetAsset<T>(path);
        }
        else
        { 
            string nonExtPath = Path.ChangeExtension(path, null);
            return Resources.Load<T>(nonExtPath);
        }
#else
        string nonExtPath = Path.ChangeExtension(path, null);
        return Resources.Load<T>(nonExtPath);
#endif
    }
    #endregion

    public float GetPatchProgress()
    {
        return m_fProgress;
    }

    /// <summary>
    ///오브젝트의 메터리얼 및 세이더 세팅.. 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    public void RefreshMaterials(BUNDLE_TYPE type, GameObject obj)
    {
#if ASSETBUNDLE
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        string materialName = string.Empty;
        string textureName = string.Empty;
        string path = string.Empty;
        Material material;

        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; ++i)
            {
                if (renderers[i].materials == null)
                    continue;

                for (int j = 0; j < renderers[i].materials.Length; ++j)
                {
                    path = string.Format("{0}.mat", renderers[i].materials[j].name);
                    path = path.Replace(" (Instance)", "");

                    material = Load<Material>(type, path);
                    material.shader = Shader.Find(material.shader.name);

                    path = string.Format("{0}.png", material.mainTexture.name);
                    material.mainTexture = Load<Texture>(type, path);

                    renderers[i].materials[j] = material;
                }
            }
        }
#endif //ASSETBUNDLE
    }
}
