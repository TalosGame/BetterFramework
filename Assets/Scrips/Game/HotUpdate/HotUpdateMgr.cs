using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 热更新管理器
/// </summary>
public class HotUpdateMgr : DDOLSingleton<HotUpdateMgr>
{
    // 热更新资源完成
    public VoidDelegate DoHotUpdateComplete;

    // 游戏基本配置
    private TableGameConfig tableGameConfig;
    public TableGameConfig TableGameConfig
    {
        get { return tableGameConfig; }
    }

    // 服务器棋牌资源配置
    private TableGameRes serverTableResConfig;

    // 下载管理
    private DownLoadManager downLoadMgr;
    // 下载状态
    private Dictionary<string, object> userInf = new Dictionary<string, object>();

    // cdn服务器url地址
    private string cdnServerURL = string.Empty;
    public string CdnServerURL
    {
        set 
        {
            string target = string.Empty;
#if UNITY_STANDALONE_WIN
            target = ValidBuildTarget.StandaloneWindows.ToString();
#elif UNITY_IOS
            target = ValidBuildTarget.IOS.ToString();
#elif UNITY_ANDROID
            target = ValidBuildTarget.Android.ToString();
#endif
            cdnServerURL = string.Format("{0}/{1}", value, target);
            downLoadMgr.DownLoadURL = cdnServerURL;
            Debug.Log("===DownLoadURL===:" + cdnServerURL);
        }
    }

    protected override void Init()
    {
        downLoadMgr = DownLoadManager.Instance;
    }

    #region 热更新事件处理
    public void RegisterHotUpdateEventHandle()
    {
        NotificationCenter.Instance.AddObserver(this, GameConst.NOTIFY_HANDLE_UI_LOADING_COMPLETE, DoHotUpdateEnd);
    }

    public void RemoveHotUpdateEventHandle()
    {
        NotificationCenter.Instance.RemoveObserver(this);
    }

    public void DoHotUpdateEnd(Notification notify)
    {
        if (DoHotUpdateComplete != null)
        {
            DoHotUpdateComplete();
        }
        else
        {
            Debug.LogError("Need DoHotUpdateComplete call back!");
        }

        Clean();
    }
    
    private void Clean()
    {
        RemoveHotUpdateEventHandle();

        while (DoHotUpdateComplete != null)
        {
            DoHotUpdateComplete -= this.DoHotUpdateComplete;
        }
    }
    #endregion

    #region 大厅热更新逻辑
    // 执行大厅下载
    public void DOLobbyHotUpdate()
    {
        PostLoadingState(LoadingState.checkUpdateRes);
        
        RegisterHotUpdateEventHandle();

        StartCoroutine(DoLobbyHotUpdateCor());
    }

    private IEnumerator DoLobbyHotUpdateCor()
    {
        yield return StartCoroutine(DownloadGameConfigFile());
        if (TableGameConfig == null)
        {
            Debug.LogError("下载版本文件异常！");
            PostLoadingState(LoadingState.downLoadFailed);
            yield break;
        }

        SyncGameSetting();

        if (CheckDownLoadResConfig())
        {
            yield return StartCoroutine(DownLoadResConfig());
        }

        if (!CheckLobbyHotUpdate())
        {
            PostLoadingState(LoadingState.latestVersion, 1);
            yield break;
        }

        // 比对资源配置表
        InitLobbyLoadFiles();

        // 检查是否更新
        if (!CheckHaveUpdateFiles())
        {
            Debug.Log("No files need to down load!");
            PostLoadingState(LoadingState.latestVersion);
            yield break;
        }

        StartCoroutine(StartDownload(false, (_obj) =>
        {
            // 如果更新下载成功
            if (_obj)
            {
                // 同步大厅资源配置数据
                SyncLobbyResConfig();

                PostLoadingState(LoadingState.downLoadSucess, 1);        
            }
        }));
    }

    /// <summary>
    /// 同步游戏设置
    /// </summary>
    private void SyncGameSetting()
    {
        Debug.logger.logEnabled = tableGameConfig.openDebugLog;
        SocketClient.EnableLogging(tableGameConfig.openNetDebugLog);
        Input.multiTouchEnabled = tableGameConfig.multyTouchEnabled;
        Application.targetFrameRate = tableGameConfig.targetFrameRate;

        if (tableGameConfig.keepScreenLight)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }

    /// <summary>
    /// 同步大厅资源配置
    /// </summary>
    private void SyncLobbyResConfig()
    {
        LocalResConfigMgr.Instance.SyncLobbyResConfig(serverTableResConfig.lobbyResConfig);
    }

    private IEnumerator DownloadGameConfigFile()
    {
        string url =  downLoadMgr.FormatUrl(TableGameConfig.FILE_NAME);

        WWW www = new WWW(url);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError("Down load game version file error!" + www.error);

            www.Dispose();
            www = null;
            yield break;
        }
        
        tableGameConfig = XMLSerializer.Read<TableGameConfig>(www.bytes) as TableGameConfig;

        www.Dispose();
        www = null;
    }

    private IEnumerator DownLoadResConfig()
    {
        string url = downLoadMgr.FormatUrl(TableGameRes.FILE_NAME);

        WWW www = new WWW(url);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError("Down load game res config file error!" + www.error);

            www.Dispose();
            www = null;
            yield break;
        }

        serverTableResConfig = XMLSerializer.Read<TableGameRes>(www.bytes) as TableGameRes;

        www.Dispose();
        www = null;
    }

    private void InitLobbyLoadFiles()
    {
        InitLobbyDownLoadFiles();        
        downLoadMgr.DebugDownLoadFiles();
    }

    /// <summary>
    /// 初始大厅下载文件
    /// </summary>
    private void InitLobbyDownLoadFiles()
    {
        ABAssetDataMgr abAssetMgr = ABAssetDataMgr.Instance;

        // 比对lua文件
        LobbyResConfig serverLobbyRes = serverTableResConfig.lobbyResConfig;
        List<ResData> sLuaDatas = serverLobbyRes.scripts.luaFiles;
        foreach (ResData sLuaData in sLuaDatas)
        {
            string abName = sLuaData.name;
            ResData luaData = abAssetMgr.FindResData(abName, ResourceType.RES_LUA);
            if (luaData != null && sLuaData.md5 == luaData.md5)
                continue;

            downLoadMgr.AddDownLoadFile(sLuaData);
        }

        // 比对shader文件
        ResData ssData = serverLobbyRes.shader;
        ResData lsData = abAssetMgr.FindResData(ssData.name);
        if (lsData != null && ssData.md5 != lsData.md5)
        {
            downLoadMgr.AddDownLoadFile(ssData);
        }

        // 比对数据文件
        ResData sData = serverLobbyRes.datas;
        ResData lData = abAssetMgr.FindResData(sData.name);
        if (lData != null && sData.md5 != lData.md5)
        {
            downLoadMgr.AddDownLoadFile(sData);
        }

        // 比对公有资源
        ComparisonFiles(serverLobbyRes.common);
        // 比热更新资源
        ComparisonFiles(serverLobbyRes.hotUpdate);
        // 比对登录资源
        ComparisonFiles(serverLobbyRes.login);
        // 比对大厅资源
        ComparisonFiles(serverLobbyRes.lobby);
        // 比对游戏资源
        ComparisonFiles(serverLobbyRes.game);
    }
    #endregion

    #region 检查资源是否需要更新
    // 检查是否下载服务器资源配置表
    private bool CheckDownLoadResConfig()
    {
        // 检查是否需要热更新
        if (!TableGameConfig.openHotUpdate)
            return false;

        LocalResConfigMgr localResConfigMgr = LocalResConfigMgr.Instance;

        // 比对大厅资源版本
        LobbyResConfig lobbyResConfig = localResConfigMgr.LocalLobbyResConfig;
        if (TableGameConfig.openLobbyHp && CheckVersion(TableGameConfig.version, lobbyResConfig.version) > 0)
        {
            return true;
        }

        // 判断是否游戏是否需要更新
        if (!TableGameConfig.openGameHp)
            return false;

        // 比对游戏资源版本
        GameResConfig gameResConfig = localResConfigMgr.LocalGameResConfig;
        var enumerator = TableGameConfig.gameVersion.GetEnumerator();
        while (enumerator.MoveNext())
        {
            int productId = enumerator.Current.Key;
            string gameVer = enumerator.Current.Value;
            BaseRes gameRes = gameResConfig.FindProductRes(productId);
            if (gameRes == null || CheckVersion(gameVer, gameRes.version) > 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckLobbyHotUpdate()
    {
        if (!TableGameConfig.openHotUpdate || !TableGameConfig.openLobbyHp)
            return false;

        LobbyResConfig lobbyResConfig = LocalResConfigMgr.Instance.LocalLobbyResConfig;
        if (string.IsNullOrEmpty(TableGameConfig.version)
            || string.IsNullOrEmpty(lobbyResConfig.version)
            || CheckVersion(TableGameConfig.version, lobbyResConfig.version) <= 0)
        {
            Debug.Log("Lobby Version code is same!");
            return false;
        }

        return true;
    }

    public bool CheckGameHotUpdate(int productId)
    {
#if STREAM_ASSET
        if (!TableGameConfig.openHotUpdate || !TableGameConfig.openGameHp)
            return false;

        LocalResConfigMgr localResConfigMgr = LocalResConfigMgr.Instance;
        localResConfigMgr.LoadGameResConfig(productId);

        BaseRes gameResConfig = localResConfigMgr.FindGameRes(productId);
        string serverGameVer = TableGameConfig.GetGameVersion(productId);
        if (string.IsNullOrEmpty(serverGameVer) 
            || string.IsNullOrEmpty(gameResConfig.version)
            || CheckVersion(serverGameVer, gameResConfig.version) <= 0)
        {
            Debugger.Log("Game version code is same! product id==" + productId);
            return false;
        }

        return true;
#else
        return false;
#endif
    }
    #endregion

    #region 游戏资源热更新
    public void DOGameHotUpdate(int productId, bool toLua = true)
    {
        PostLoadingState(LoadingState.checkUpdateRes, isPostTolua:toLua);

        RegisterHotUpdateEventHandle();

        // 比对资源配置表
        InitGameLoadFiles(productId);

        // 检查是否更新
        if (!CheckHaveUpdateFiles())
        {
            Debug.Log("No files need to down load!");
            PostLoadingState(LoadingState.checkUpdateResErr, isPostTolua:toLua);
            return;
        }

        StartCoroutine(StartDownload(toLua, (_obj) =>
        {
            // 下载成功
            if (_obj)
            {
                // 同步游戏资源配置数据
                SyncGameResConfig(productId);
                PostLoadingState(LoadingState.downLoadSucess, 1, toLua);
            }
        }));
    }

    /// <summary>
    /// 初始游戏下载文件
    /// </summary>
    private void InitGameLoadFiles(int productId)
    {
        InitGameDownLoadFiles(productId);

        downLoadMgr.DebugDownLoadFiles();
    }

    /// <summary>
    /// 初始游戏下载文件
    /// </summary>
    private void InitGameDownLoadFiles(int productId)
    {
        GameResConfig serverLobbyRes = serverTableResConfig.gameResConfig;
        BaseRes serverGameRes = serverLobbyRes.FindProductRes(productId);

        ComparisonFiles(serverGameRes, productId);        
    }

    /// <summary>
    /// 同步游戏资源配置
    /// </summary>
    private void SyncGameResConfig(int productId)
    {
        LocalResConfigMgr.Instance.SyncGameResConfig(serverTableResConfig.gameResConfig, productId);
    }
    #endregion

    #region 公有逻辑
    /// <summary>
    /// 比对版本号数字,位个数一样
    /// 0  版本号一样
    /// 1  服务器版本号新
    /// -1 服务器版本老
    /// </summary>
    /// <param name="serverVer"></param>
    /// <param name="localVer"></param>
    /// <returns></returns>
    private int CheckVersion(string serverVer, string localVer)
    {
        string[] sVerCode = serverVer.Replace("v", "").Split('.');
        string[] lVerCode = localVer.Replace("v", "").Split('.');

        for (int i = 0; i < sVerCode.Length; i++)
        {
            int sNum = Convert.ToInt32(sVerCode[i]);
            int lNum = Convert.ToInt32(lVerCode[i]);

            if (sNum == lNum)
                continue;

            if (sNum > lNum)
                return 1;

            return -1;
        }

        return 0;
    }

    private IEnumerator StartDownload(bool toLua, Action<bool> callBack)
    {
        // 开始下载资源文件
        downLoadMgr.StartDownloadFiles();
        PostLoadingState(LoadingState.startDownLoading, 0, toLua);

        bool downLoadSucceed = false;
        do
        {
            if (downLoadMgr.CheckDownloadFailed())
            {
                downLoadMgr.StopAll();

                PostLoadingState(LoadingState.downLoadFailed, isPostTolua:toLua);
                
                // 更新下载失败
                callBack(false);
                yield break;
            }

            if (downLoadMgr.CheckDownloadOver())
            {
                downLoadSucceed = true;
            }

            float rateOfProgress = downLoadMgr.ProgressOfBundles();
            Debug.Log("download Progress===" + rateOfProgress);

            PostLoadingState(LoadingState.downLoading, rateOfProgress, toLua);
            yield return null;

        } while (!downLoadSucceed);

        // 下载管理停止
        downLoadMgr.StopAll();

        // 更新下载成功
        callBack(true);
    }

    private void PostLoadingState(LoadingState state, float progress = 0f, bool isPostTolua = false)
    {
        if (isPostTolua)
        {
            PostLuaLoadingState(state, progress);
            return;
        }

        PostCSLoadingState(state, progress);
    }

    private void PostCSLoadingState(LoadingState state, float progress = 0f)
    {
        userInf.Clear();
        userInf["state"] = state;
        userInf["progress"] = progress;

        NotificationCenter.Instance.PostNotification(GameConst.NOTIFY_HANDLE_UI_LOAD_PROCESS, null, userInf);
    }

    // lua 推送loading数据
    private void PostLuaLoadingState(LoadingState state, float progress = 0f)
    {
        LuaHelper.CallLuaDirectFunction(GameConst.LUA_BROCAST_VIEW_FUNC, GameConst.NOTIFY_HANDLE_UI_LOAD_PROCESS, (int)state, progress);
    }

    private void ComparisonFiles(BaseRes baseRes, int productId = -1)
    {
        // 比对图集资源
        ComparisonFiles(baseRes.atlas, productId);
        // 比对纹理资源
        ComparisonFiles(baseRes.textures, productId);
        // 比对声音
        ComparisonFiles(baseRes.audios, productId);
        // 比对字体
        ComparisonFiles(baseRes.fonts, productId);
        // 比对预制
        ComparisonFiles(baseRes.prefabs, productId);
    }

    /// <summary>
    /// 比对资源文件
    /// </summary>
    /// <param name="datas"></param>
    private void ComparisonFiles(List<ResData> datas, int productId)
    {
        ABAssetDataMgr abAssetMgr = ABAssetDataMgr.Instance;
        foreach (ResData sData in datas)
        {
            string abName = sData.name;
            ResData luaData = abAssetMgr.FindResData(abName, productId:productId);
            if (luaData != null && sData.md5 == luaData.md5)
                continue;

            downLoadMgr.AddDownLoadFile(sData);
        }
    }

    /// <summary>
    /// 判断是否有需要更新的文件
    /// </summary>
    /// <returns></returns>
    private bool CheckHaveUpdateFiles()
    {
        if (downLoadMgr.DownLoadFiles.Count <= 0)
        {
            return false;
        }

        return true;
    }
    #endregion
}

