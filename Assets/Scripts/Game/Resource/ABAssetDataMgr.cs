using System.Collections.Generic;
using LuaInterface;

/// <summary>
/// 游戏AB资源数据
public class GameAbAssets
{
    /// <summary>
    /// 每个产品ID， 对应一些资源
    /// </summary>
    public Dictionary<int, Dictionary<string, ResData>> gameAssetsInfo = new Dictionary<int, Dictionary<string, ResData>>();

    public Dictionary<string, ResData> CreateGameAsset(int productId)
    {
        Dictionary<string, ResData> ret = null;
        if (gameAssetsInfo.TryGetValue(productId, out ret))
        {
            Debugger.LogError("Game asset already exist!!!!!");
            return ret;
        }

        ret = new Dictionary<string, ResData>();
        gameAssetsInfo.Add(productId, ret);
        return ret;
    }

    /// <summary>
    /// 查询游戏资源
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public Dictionary<string, ResData> FindGameAsset(int productId)
    {
        Dictionary<string, ResData> ret = null;
        if (!gameAssetsInfo.TryGetValue(productId, out ret))
        {
            return null;
        }

        return ret;
    }

    /// <summary>
    /// 清除产品资源配置
    /// </summary>
    /// <param name="productId"></param>
    public void Clean(int productId)
    { 
        if(!gameAssetsInfo.ContainsKey(productId))
        {
            Debugger.LogError("Game asset don't have this product! id==" + productId);
            return;
        }

        gameAssetsInfo.Remove(productId);
    }
}

/// <summary>
/// AB资源信息数据管理器
/// </summary>
public class ABAssetDataMgr : SingletonBase<ABAssetDataMgr>
{
    private Dictionary<string, ResData> luaAssetsInfo = new Dictionary<string, ResData>();
    private Dictionary<string, ResData> lobbyAssetsInfo = new Dictionary<string, ResData>();
    private GameAbAssets gameAssetsInfo = new GameAbAssets();

    #region 数据加载模块
    public void LoadLobbyAssetInfo(TableGameRes resConfig)
    {
        LoadLuaAssetsInfo(resConfig);
        LoadLobbyAssetsInfo(resConfig);
    }
    
    // 同步大厅资源信息
    public void SyncLobbyAssetInfo(TableGameRes resConfig)
    {
        luaAssetsInfo.Clear();
        lobbyAssetsInfo.Clear();

        LoadLobbyAssetInfo(resConfig);
    }

    private void LoadLuaAssetsInfo(TableGameRes resConfig)
    {
        List<ResData> datas = resConfig.lobbyResConfig.scripts.luaFiles;
        foreach (ResData data in datas)
        {
            luaAssetsInfo.Add(data.name, data);
        }
    }

    private void LoadLobbyAssetsInfo(TableGameRes resConfig)
    {
        LobbyResConfig lobbyResConfig = resConfig.lobbyResConfig;

        ResData data = lobbyResConfig.shader;
        if (data != null)
        {
            lobbyAssetsInfo.Add(data.name, data);
        }

        data = lobbyResConfig.datas;
        if (data != null)
        {
            lobbyAssetsInfo.Add(data.name, data);
        }

        LoadAssetsInfo(lobbyResConfig.common, lobbyAssetsInfo);
        LoadAssetsInfo(lobbyResConfig.hotUpdate, lobbyAssetsInfo);
        LoadAssetsInfo(lobbyResConfig.login, lobbyAssetsInfo);
        LoadAssetsInfo(lobbyResConfig.lobby, lobbyAssetsInfo);
        LoadAssetsInfo(lobbyResConfig.game, lobbyAssetsInfo);
    }

    /// <summary>
    /// 同步游戏资源配置
    /// </summary>
    /// <param name="resConfig"></param>
    /// <param name="productId"></param>
    public void SyncGameAssetInfo(TableGameRes resConfig, int productId)
    {
        gameAssetsInfo.Clean(productId);
        LoadGameAssetsInfo(resConfig, productId);
    }

    /// <summary>
    /// 加载游戏资源配置
    /// </summary>
    /// <param name="resConfig"></param>
    /// <param name="productId">产品ID</param>
    public void LoadGameAssetsInfo(TableGameRes resConfig, int productId)
    {
        GameResConfig gameResConfig = resConfig.gameResConfig;
        BaseRes gameRes = gameResConfig.FindProductRes(productId);
        if (gameRes == null)
            return;


        Dictionary<string, ResData> assetInfo = gameAssetsInfo.FindGameAsset(productId);
        if (assetInfo != null)
        {
            return;
        }

        assetInfo = gameAssetsInfo.CreateGameAsset(productId);
        LoadAssetsInfo(gameRes, assetInfo);
    }

    private void LoadAssetsInfo(BaseRes res, Dictionary<string, ResData> assetInfo)
    {
        List<ResData> datas = res.atlas;
        foreach (ResData atlas in datas)
            assetInfo.Add(atlas.name, atlas);

        datas = res.audios;
        foreach (ResData audio in datas)
            assetInfo.Add(audio.name, audio);

        datas = res.prefabs;
        foreach (ResData prefab in datas)
            assetInfo.Add(prefab.name, prefab);

        datas = res.textures;
        foreach (ResData texture in datas)
            assetInfo.Add(texture.name, texture);

        datas = res.fonts;
        foreach (ResData font in datas)
            assetInfo.Add(font.name, font);
    }
    #endregion

    #region 数据查询
    private int CurrentProductId {
        get {
            return LocalResConfigMgr.Instance.ProductId;
        }
    }

    public ResData FindResData(string name, int resType = -1)
    {
        return FindResData(name, CurrentProductId, resType);
    }

    public ResData FindResData(string name, int productId, int resType = -1)
    {
        ResData data = null;

        if (resType == ResourceType.RES_LUA)
        {
            if (luaAssetsInfo.TryGetValue(name, out data))
            {
                return data;
            }

            return null;
        }

        // 查询是否是大厅资源
        if (lobbyAssetsInfo.TryGetValue(name, out data))
        {
            return data;
        }

        // 查询具体产品资源
        if (productId == -1)
            return null;

        Dictionary<string, ResData> gameResData = gameAssetsInfo.FindGameAsset(productId);
        if (gameResData == null)
            return null;

        if (gameResData.TryGetValue(name, out data))
        {
            return data;
        }

        return null;
    }

    #endregion
}

