using System.IO;
using System.Collections;
using UnityEngine;

/// <summary>
/// 本地游戏资源配置数据管理
/// </summary>
public class LocalResConfigMgr : SingletonBase<LocalResConfigMgr>
{
    // 本地棋牌资源配置表
    private TableGameRes localTableResConfig;
    public TableGameRes LocalTableResConfig
    {
        get { return localTableResConfig; }
    }

    // 当前产品ID
    private int productId = -1;
    public int ProductId
    {
        get { return productId; }
        set { productId = value; }
    }

    private ABAssetData abAssetData;

    protected override void Init()
    {
        ABAssetDataBase abDataBase = ABAssetDataMgr.Instance.AbAssetData;
        if(abDataBase == null)
        {
            Debug.LogError("AB Asset data is null!");
            return;
        }

        abAssetData = abDataBase as ABAssetData;
    }

    #region 读取本地资源配置
    /// <summary>
    /// 读取本地资源配置数据
    /// </summary>
    public void LoadLocalGameRes()
    {
        // 首先读取沙盒目录下资源配置文件
        string xmlPath = PathConfiger.GetSandboxFilePath(TableGameRes.FILE_NAME);
        Debug.Log("Sandbox xml file path====" + xmlPath);
        if (MLFileUtil.CheckFileExits(xmlPath))
        {
            localTableResConfig = XMLSerializer.Read<TableGameRes>(xmlPath) as TableGameRes;
            LoadResConfigComplete();
            return;
        }

        CoroutineManger.Instance.StartCoroutine(LoadLocalGameResAsync());
    }

    private IEnumerator LoadLocalGameResAsync()
    {
        string xmlPath = string.Format("{0}{1}", PathConfiger.PACKAGE_STREAMING_DATA_PATH, TableGameRes.FILE_NAME);
        Debug.Log("streaming xml file path==" + xmlPath);
        WWW xmlWWW = new WWW(xmlPath);
        yield return xmlWWW;

        byte[] datas = xmlWWW.bytes;
        if (datas == null || datas.Length <= 0)
        {
            Debug.LogError("Load resConfig xml in streaming path error!");
            yield break;
        }

        // copy file to sandbox
        string sandBoxPath = PathConfiger.GetSandboxFilePath(TableGameRes.FILE_NAME);
        File.WriteAllBytes(sandBoxPath, datas);

        xmlWWW.Dispose();
        xmlWWW = null;

        // read file in sandbox
        localTableResConfig = XMLSerializer.Read<TableGameRes>(datas) as TableGameRes;

        LoadResConfigComplete();
    }

    private void LoadResConfigComplete()
    {
        if (LocalTableResConfig == null)
        {
            Debug.LogError("加载本地资源配置数据异常!");
            return;
        }

        // 加载大厅AB文件信息
        abAssetData.LoadLobbyAssetInfo(LocalTableResConfig);
    }

    /// <summary>
    /// 同步大厅资源配置数据
    /// </summary>
    /// <param name="lobbyResConfig"></param>
    public void SyncLobbyResConfig(LobbyResConfig lobbyResConfig)
    {
        LocalTableResConfig.lobbyResConfig = lobbyResConfig;

        abAssetData.SyncLobbyAssetInfo(LocalTableResConfig);

        string xmlPath = PathConfiger.GetSandboxFilePath(TableGameRes.FILE_NAME);
        XMLSerializer.Save<TableGameRes>(xmlPath, LocalTableResConfig);
    }

    public void LoadGameResConfig(int productId)
    {
        abAssetData.LoadGameAssetsInfo(LocalTableResConfig, productId);
    }

    /// <summary>
    /// 同步具体产品游戏资源配置数据
    /// </summary>
    public void SyncGameResConfig(GameResConfig gameResConfig, int productId)
    {
        BaseRes newGameRes = gameResConfig.FindProductRes(productId);
        if (newGameRes == null)
            return;

        LocalTableResConfig.gameResConfig.RebuildGameRes(newGameRes, productId);

        abAssetData.SyncGameAssetInfo(LocalTableResConfig, productId);

        string xmlPath = PathConfiger.GetSandboxFilePath(TableGameRes.FILE_NAME);
        XMLSerializer.Save<TableGameRes>(xmlPath, LocalTableResConfig);
    }
    #endregion

    #region 查询资源配置
    public GameResConfig LocalGameResConfig
    {
        get 
        {
            if (localTableResConfig == null)
            {
                Debug.LogError("本地游戏资源配置数据异常!");
                return null;
            }

            return localTableResConfig.gameResConfig; 
        }
    }

    public LobbyResConfig LocalLobbyResConfig
    {
        get
        {
            if (localTableResConfig == null)
            {
                Debug.LogError("本地游戏资源配置数据异常!");
                return null;
            }

            return localTableResConfig.lobbyResConfig;
        }
    }

    /// <summary>
    /// 查找游戏资源
    /// </summary>
    /// <returns></returns>
    public BaseRes FindGameRes()
    {
        return FindGameRes(this.productId);
    }

    public BaseRes FindGameRes(int productId)
    {
        if (localTableResConfig == null)
        {
            Debug.LogError("本地游戏资源配置数据异常!");
            return null;
        }

        GameResConfig gameResConfig = localTableResConfig.gameResConfig;
        return gameResConfig.FindProductRes(productId);
    }
    #endregion
}

