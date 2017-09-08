using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 已加载的asset bundle
/// 被引用的个数, 方便自动卸载所依赖的资源
/// </summary>
public class LoadedAssetBundle
{
    public string name;
    public bool loadError = false;
    public AssetBundle assetBundle;
    public int referencedCount;

    public LoadedAssetBundle(string name, AssetBundle assetBundle)
    {
        this.name = name;
        this.loadError = false;
        this.assetBundle = assetBundle;
        referencedCount = 1;
    }

    public LoadedAssetBundle(string name, bool loadError, AssetBundle assetBundle)
    {
        this.name = name;
        this.loadError = loadError;
		this.assetBundle = assetBundle;
		referencedCount = 1;
    }
}

public class AssetBundleManager : ResManagerBase
{
	private AssetBundleManifest assetBundleManifest = null;

    private Dictionary<string, LoadedAssetBundle> loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

	public override void Init(ResourceDefine resourceDefine)
	{
		base.Init(resourceDefine);

        LoadABManifest();
	}

    public void LoadABManifest(bool isPermanent = false)
	{
        ResourceInfo bundle = GetResourceInfo(ABConfiger.ASSET_MANIFEST_NAME, ResourceType.RES_ASSETBUNDLE, isPermanent);
        string path = ABConfiger.GetABFilePath(ABConfiger.ASSET_MANIFEST_NAME);
		Debug.Log("load asset bundle url==" + path);

		if (path == string.Empty)
		{
            RemoveResourceInfo(ABConfiger.ASSET_MANIFEST_NAME);
			return;
		}

		AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
		if (assetBundle == null)
		{
            RemoveResourceInfo(ABConfiger.ASSET_MANIFEST_NAME);

            Debug.LogError("Load Local Manifest error!");
			return;
		}

		assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		bundle._assetObj = assetBundle;
	}

    public override ResManagerType ManagerType()
    {
        return ResManagerType.assetBundleMgr;
    }

    protected override ResourceInfo CreateResourceInfo(string name, int type)
    {
		ResourceInfo ret = new ResourceInfo();
        ret.Name = name.ToLower();
		ret.ResType = type;
        ret.Path = resourceDefine.GetResourcePath(type, ret.Name).ToLower();

		return ret;
    }

	#region 仅仅加载Bundle
	protected override void LoadBundle(ResourceInfo info)
	{
		//ResData bundleData = GetBundleRes(info.Name, info.ResType);
		//if (bundleData == null)
		//{
		//    Debug.LogError("Get bundle data error!");
		//    return;
		//}

		AssetBundle assetBundle = GetCacheAssetBundle(info.Name);
		if (assetBundle != null)
		{
			return;
		}

		LoadSyncAssetBundle(info);
	}
	#endregion

	#region asset bundle 同步加载
	protected override UnityEngine.Object Load(ResourceInfo info)
    {
        AssetBundle assetBundle = GetCacheAssetBundle(info.Name);
        if (assetBundle != null)
        {
            return assetBundle.LoadAsset(info.Name);
        }

        assetBundle = LoadSyncAssetBundle(info);
        if(assetBundle == null)
        {
            return null;
        }

        UnityEngine.Object assetObj = assetBundle.LoadAsset(info.Name);
        if (assetObj == null)
        {
            return null;
        }

        info._assetObj = assetObj;
        return assetObj;
    }

    private AssetBundle LoadSyncAssetBundle(ResourceInfo info)
    {
        LoadSyncDependencies(info);
        return LoadAssetBundle(info.Name, info.Path);
    }

    private void LoadSyncDependencies(ResourceInfo info)
    {
        string[] dependencies = assetBundleManifest.GetAllDependencies(info.Path);
        if(dependencies == null || dependencies.Length == 0)
        {
            return;
        }

        foreach (string dependence in dependencies)
        {
            AssetBundle assetBundle = GetCacheAssetBundle(dependence);
            if (assetBundle != null)
            {
                continue;
            }

            LoadAssetBundle(dependence);
        }
    }

    private AssetBundle LoadAssetBundle(string abPath)
    {
        string name = Path.GetFileNameWithoutExtension(abPath);
        return LoadAssetBundle(name, abPath);
    }

    private AssetBundle LoadAssetBundle(string name, string abPath)
    {
        string path = ABConfiger.GetABFilePath(abPath);
        AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
        if (assetbundle == null)
        {
            return null;
        }

        loadedAssetBundles.Add(name, new LoadedAssetBundle(name, assetbundle));
        return assetbundle;
    }
    #endregion

    #region 异步加载资源
    public override IEnumerator LoadAsync(ResourceInfo info, Action<UnityEngine.Object> load, Action<float> progress)
    {
		AssetBundle assetBundle = GetCacheAssetBundle(info.Name);
		if (assetBundle != null)
		{
            if(load != null)
            {
                load(assetBundle.LoadAsset(info.Name));
            }

            if(progress != null)
            {
                progress(1.0f);                
            }

            yield break;
		}

        CoroutineManger.Instance.StartCoroutine(LoadAsyncAssetBundle(info));

        while(true)
        {
            LoadedAssetBundle ab = GetLoadedAssetBundle(info);
            if(ab == null)
            {
                yield return null;
                continue;
            }

            if(ab.loadError)
            {
                Debug.LogError("Load asset bundle error! name:" + info.Name);
                if(load != null)
                {
                    load(null);
                }

                yield break;
            }

            var abRequest = ab.assetBundle.LoadAssetAsync(info.Name);
            while (!abRequest.isDone)
            {
            	if (progress != null)
            	{
            		progress(abRequest.progress);
            	}

                yield return null;
            }

            if (progress != null)
            {
            	progress(1.0f);
            }

            info._assetObj = abRequest.asset;

            if (load != null)
            {
            	load(abRequest.asset);
            }

            yield break;
        }
    }

    private IEnumerator LoadAsyncAssetBundle(ResourceInfo info)
	{
        yield return CoroutineManger.Instance.StartCoroutine(LoadAsyncDependencies(info));

        CoroutineManger.Instance.StartCoroutine(LoadAsyncAssetBundle(info.Name, info.Path));
	}

    private IEnumerator LoadAsyncDependencies(ResourceInfo info)
    {
		string[] dependencies = assetBundleManifest.GetAllDependencies(info.Path);
		if (dependencies == null || dependencies.Length == 0)
		{
            yield break;
		}

        int loadCount = 0;
        foreach (string abPath in dependencies)
        {
            string name = Path.GetFileNameWithoutExtension(abPath);
            AssetBundle assetBundle = GetCacheAssetBundle(name);
            if (assetBundle != null)
            {
                loadCount++;
                continue;
            }

			CoroutineManger.Instance.StartCoroutine(LoadAsyncAssetBundle(name, abPath, (_obj) =>
			{
				loadCount++;
            }));
        }

        while(loadCount < dependencies.Length)
        {
            yield return null;
        }
    }

    private IEnumerator LoadAsyncAssetBundle(string name, string abPath, Action<UnityEngine.Object> load = null)
    {
		string path = ABConfiger.GetABFilePath(abPath);
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
        while (!bundleLoadRequest.isDone)
        {
            yield return null;
        }

		var assetBundle = bundleLoadRequest.assetBundle;
		if (assetBundle == null)
		{
            loadedAssetBundles.Add(name, new LoadedAssetBundle(name, true, null));
			yield break;
		}

		loadedAssetBundles.Add(name, new LoadedAssetBundle(name, assetBundle));
        if(load != null)
        {
            load(assetBundle);
        }
    }
    #endregion

    #region 资源释放
    /// <summary>
    /// 只释放自己的资源，依赖不释放
    /// </summary>
    /// <param name="info"></param>
    /// <param name="unloadObject"></param>
    public override void UnloadResource(ResourceInfo info, bool unloadObject = false)
    {
        //Debuger.Log(loadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + assetBundleName);
        UnloadAssetBundleInternal(info, unloadObject);
        //Debuger.Log(loadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + assetBundleName);    
    }

    public override void UnloadAllResource(ResourceInfo info, bool unloadObject = false)
    {
        UnloadAssetBundleInternal(info, unloadObject);
        UnloadDependencies(info, unloadObject);
    }

    private void UnloadAssetBundleInternal(ResourceInfo info, bool unloadObject)
    {
        LoadedAssetBundle bundleRes = GetLoadedAssetBundle(info);
        if (bundleRes == null)
            return;

        UnloadAssetBundle(bundleRes, unloadObject);
    }

    private void UnloadDependencies(ResourceInfo info, bool unloadObject)
    {
        string[] dependencies = assetBundleManifest.GetAllDependencies(info.Path);
        foreach (var abPath in dependencies)
        {
            string name = Path.GetFileNameWithoutExtension(abPath);
            LoadedAssetBundle bundle = null;
            if (!loadedAssetBundles.TryGetValue(name, out bundle))
            {
                continue;
            }

            UnloadAssetBundle(bundle, unloadObject);
        }
    }

    private void UnloadAssetBundle(LoadedAssetBundle bundleRes, bool unloadObject)
    {
        if (--bundleRes.referencedCount == 0)
        {
            bundleRes.assetBundle.Unload(unloadObject);
            bundleRes.assetBundle = null;
            loadedAssetBundles.Remove(bundleRes.name);
            //Debugger.Log("AssetBundle " + assetBundleName + " has been unloaded successfully");
        }
    }
	#endregion

	#region 公用功能模块
    private AssetBundle GetCacheAssetBundle(string name)
    {
        LoadedAssetBundle bundle = null;
        if (loadedAssetBundles.TryGetValue(name, out bundle))
        {
            bundle.referencedCount++;
            return bundle.assetBundle;
        }

        return null;
    }

    private LoadedAssetBundle GetLoadedAssetBundle(ResourceInfo info)
    {
        LoadedAssetBundle bundle = null;
        if (loadedAssetBundles.TryGetValue(info.Name, out bundle))
        {
            return bundle;
        }

        return null;
    }

    /// <summary>
    /// 获取asset bundle 名称
    /// </summary>
    /// <param name="assetName">资源名称</param>
    /// <returns></returns>
    private string GetAssetBundleName(string assetName)
    {
        string assetBundleName = string.Format("{0}.{1}", assetName, ABConfiger.BUNDLE_SUFFIX);
        return assetBundleName;
    }
    #endregion
}

