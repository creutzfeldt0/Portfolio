using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class AssetBundleMgr : Editor
{
    [MenuItem("CustomTool/AssetBundle/BuildAsset", false, 1)]
    //
    //  에셋번들 및 패치정보파일 생성..
    //
    static public void BuildAsset()
    {
#if UNITY_ANDROID
        string rootAssetBundleDirectory = "../AssetBundles/Android";
#elif UNITY_IOS
        string rootAssetBundleDirectory = "../AssetBundles/IOS";
#else
        string rootAssetBundleDirectory = "../AssetBundles/ETC";
#endif
        string version = PlayerSettings.bundleVersion;
        uint uVersion = uint.Parse(version.Replace(".", ""));

        if ( true == Directory.Exists(rootAssetBundleDirectory))
        {
            Directory.Delete(rootAssetBundleDirectory);

            Directory.CreateDirectory(rootAssetBundleDirectory);
        }
        else
        {
            Directory.CreateDirectory(rootAssetBundleDirectory);
        }

        string versionFile = string.Format("{0}/Version.txt", rootAssetBundleDirectory);

        using (StreamWriter hWriter = new StreamWriter(versionFile))
        {
            // 최신버전의 버전 기록..
            hWriter.WriteLine(version);
            hWriter.Close();
            hWriter.Dispose();
        }

        // 이전 패치파일 정보 가져옴..
        string patchFile = string.Format("{0}/{1}/PatchList.txt", rootAssetBundleDirectory, version);
        Dictionary<string, AssetbundlePatchData> patchDatas = null;
        AssetbundlePatchData data = null;

        if (true == File.Exists(patchFile))
        {
            patchDatas = new Dictionary<string, AssetbundlePatchData>();

            using (StreamReader hReader = new StreamReader(patchFile))
            {
                string prevVersion = hReader.ReadLine();
                string[] lineData;

                while (false == hReader.EndOfStream)
                {
                    lineData = hReader.ReadLine().Split('\t');

                    data = new AssetbundlePatchData();

                    data.Name = lineData[0];
                    data.uCRC = uint.Parse(lineData[1]);
                    data.Byte = lineData[2];
                    data.UpVersion = uint.Parse(lineData[3]);
                    data.bChecked = false;

                    patchDatas.Add(data.Name, data);
                }

                hReader.Close();
                hReader.Dispose();
            }
        }

        // Loader 정보들 입력..
        string[] AssetbundleNames = AssetDatabase.GetAllAssetBundleNames();
        string[] AssetPaths;
        GameObject obj;
        GameObject prefab;
        LoaderBase[] loaders;

        for (int i = 0; i < AssetbundleNames.Length; ++i)
        {
            AssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(AssetbundleNames[i]);

            for (int j = 0; j < AssetPaths.Length; ++j)
            {
                if (true == AssetPaths[j].Contains(".prefab"))
                {
                    obj = AssetDatabase.LoadMainAssetAtPath(AssetPaths[j]) as GameObject;

                    prefab = PrefabUtility.InstantiatePrefab(obj as GameObject) as GameObject;

                    loaders = prefab.GetComponentsInChildren<LoaderBase>();

                    for (int k = 0; k < loaders.Length; ++k)
                    {
                        loaders[k].SaveResourcePath();

                        loaders[k].DeleteResource();
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefab, AssetPaths[j]);
                    DestroyImmediate(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();

        // 번들 버전폴더 생성..
        string assetBundleDirectory = string.Format("{0}/{1}", rootAssetBundleDirectory, version);

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // 애셋번들 빌드중 힙메모리 부족이 발생하지 않도록 사용되지 않는 애셋을 Unload하고 GarbageCollecting을 한다.
        EditorUtility.UnloadUnusedAssetsImmediate();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        // 번들 생성..
        AssetBundleManifest bundleMenifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);

        // 패치파일 정보 업데이트..
        string[] assetbundleNames = bundleMenifest.GetAllAssetBundles();

        FileInfo fileInfo;
        string filePath;

        // 첫 패치임..
        if (null == patchDatas)
        {
            patchDatas = new Dictionary<string, AssetbundlePatchData>();

            for (int i = 0; i < assetbundleNames.Length; ++i)
            {
                filePath = string.Format("{0}/{1}", assetBundleDirectory, assetbundleNames[i]);
                fileInfo = new FileInfo(filePath);

                data = new AssetbundlePatchData();
                data.bChecked = true;

                data.Name = assetbundleNames[i];
                data.Byte = fileInfo.Length.ToString();
                BuildPipeline.GetCRCForAssetBundle(filePath, out data.uCRC);
                data.UpVersion = uVersion;

                patchDatas.Add(data.Name, data);
            }
        }
        // 이전 패치파일이 있음..
        else
        {
            uint uCRC = 0;

            for (int i = 0; i < assetbundleNames.Length; ++i)
            {
                filePath = string.Format("{0}/{1}", assetBundleDirectory, assetbundleNames[i]);
                BuildPipeline.GetCRCForAssetBundle(filePath, out uCRC);

                // 이전 패치파일에 정보있음..
                if (true == patchDatas.TryGetValue(assetbundleNames[i], out data))
                {
                    data.bChecked = true;

                    // 현재 그 정보가 변경됨 혹은 삭제되었다가 다시 추가됨..
                    if (data.uCRC != uCRC ||
                         0 == data.UpVersion)
                    {
                        data.uCRC = uCRC;
                        data.UpVersion = uVersion;
                    }
                }
                else// 새로 생성된 패치파일..
                {
                    fileInfo = new FileInfo(filePath);

                    data = new AssetbundlePatchData();
                    data.bChecked = true;

                    data.Name = assetbundleNames[i];
                    data.uCRC = uCRC;
                    data.Byte = fileInfo.Length.ToString();
                    data.UpVersion = uVersion;

                    patchDatas.Add(data.Name, data);
                }
            }
        }

        using (StreamWriter hWriter = new StreamWriter(patchFile))
        {
            // 최신버전의 버전 기록..
            hWriter.WriteLine(version);

            foreach (AssetbundlePatchData temp in patchDatas.Values)
            {
                // 이번 패치에서 삭제된 파일 표시..
                if (false == temp.bChecked)
                {
                    temp.UpVersion = 0;
                }

                hWriter.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", temp.Name, temp.uCRC, temp.Byte, temp.UpVersion));
            }

            hWriter.Close();
            hWriter.Dispose();
        }
    }

    [MenuItem("CustomTool/AssetBundle/Resources/ImportData")]
    //
    //  테스트를 위해 옮겼던 파일들을 리소시즈폴더로 옮김..
    //
    static public void ImportResources()
    {
        string tempResourcesPath = "TempResources/AssetBundle";

        if (false == Directory.Exists(tempResourcesPath))
            return;

        string resourcesPath = "Assets/Resources";

        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        //
        resourcesPath = "Assets/Resources/AssetBundle";

        if (true == Directory.Exists(resourcesPath))
        {
            Directory.Delete(resourcesPath);
        }

        Directory.Move(tempResourcesPath, resourcesPath);

        //
        AssetDatabase.Refresh();
    }

    [MenuItem("CustomTool/AssetBundle/Resources/MoveData")]
    //
    //  테스트를 위해 파일들을 임시폴더로 옮김..
    //
    static public void MoveResources()
    {
        string resourcesPath = "Assets/Resources/AssetBundle";

        if (!Directory.Exists(resourcesPath))
            return;

        string tempResourcesPath = "TempResources";

        if (!Directory.Exists(tempResourcesPath))
        {
            Directory.CreateDirectory(tempResourcesPath);
        }

        tempResourcesPath = "TempResources/AssetBundle";

        if (Directory.Exists(tempResourcesPath))
            return;

        Directory.Move(resourcesPath, tempResourcesPath);

        AssetDatabase.Refresh();
    }

    [MenuItem("CustomTool/AssetBundle/Resources/ResourcePathSetting")]
    static void ResourcePathSetting()
    {
        string[] dirs = Directory.GetDirectories("Assets/Resources");

        string path = "Assets/Resources";

        SetResourcePath(path);

        AssetDatabase.SaveAssets();
    }

    static void SetResourcePath(string path)
    {
        if (!Directory.Exists(path))
            return;

        string[] files = Directory.GetFiles(path);
        string filePath;
        GameObject obj;
        GameObject prefab;
        LoaderBase[] loaders;

        for (int i = 0; i < files.Length; ++i)
        {
            if (true == files[i].Contains(".prefab"))
            {
                if (true == files[i].Contains(".meta")) continue;

                filePath = files[i].Replace("\\", "/");
                filePath = filePath.Replace(".prefab", "");
                filePath = filePath.Replace("Assets/Resources/", "");

                obj = Resources.Load<GameObject>(filePath);
                prefab = PrefabUtility.InstantiatePrefab(obj as GameObject) as GameObject;

                loaders = prefab.GetComponentsInChildren<LoaderBase>();

                for (int k = 0; k < loaders.Length; ++k)
                {
                    loaders[k].SaveResourcePath();

                    loaders[k].DeleteResource();
                }

                PrefabUtility.SaveAsPrefabAsset(prefab, files[i]);
                DestroyImmediate(prefab);
            }
        }

        string[] dirs = Directory.GetDirectories(path);

        for (int i = 0; i < dirs.Length; ++i)
        {
            SetResourcePath(dirs[i]);
        }
    }

    [MenuItem("CustomTool/AssetBundle/RevertAssets", false, 1)]
    static void RevertAssets()
    {
        // Loader 정보들 가져옴..
        string[] AssetbundleNames = AssetDatabase.GetAllAssetBundleNames();
        string[] AssetPaths;
        GameObject obj;
        GameObject prefab;
        LoaderBase[] loaders;

        for (int i = 0; i < AssetbundleNames.Length; ++i)
        {
            AssetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(AssetbundleNames[i]);

            for (int j = 0; j < AssetPaths.Length; ++j)
            {
                if (true == AssetPaths[j].Contains(".prefab"))
                {
                    obj = AssetDatabase.LoadMainAssetAtPath(AssetPaths[j]) as GameObject;

                    prefab = PrefabUtility.InstantiatePrefab(obj as GameObject) as GameObject;

                    loaders = prefab.GetComponentsInChildren<LoaderBase>();

                    for (int k = 0; k < loaders.Length; ++k)
                    {
                        loaders[k].RefreshResource();
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefab, AssetPaths[j]);

                    DestroyImmediate(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    [MenuItem("CustomTool/AssetBundle/CleanCache", false, 1)]
    static void CleanCache()
    {
        bool bSuccess = Caching.ClearCache();

        Debug.Log("Clear Cache :" + bSuccess);
    }
}
