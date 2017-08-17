using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

/// <summary>
/// 游戏配置表
/// </summary>
[XmlRootAttribute("TableGameConfig")]
public class TableGameConfig
{
    [XmlIgnore]
    public const string FILE_NAME = "TableGameConfig.xml";

    // 大厅资源版本号
    [XmlElementAttribute("LobbyVer")]
    public string version;

    // 服务器ip地址
    [XmlElementAttribute("ServerIp")]
    public string serverIP;

    // 服务器端口号
    [XmlElementAttribute("ServerPort")]
    public int serverPort;

    // 开关热更新
    [XmlElementAttribute("OpenHotUpdate")]
    public bool openHotUpdate;

    // 大厅是否热更新
    [XmlElementAttribute("OpenLobbyHotUpdate")]
    public bool openLobbyHp;

    // 游戏内是否热更新
    [XmlElementAttribute("OpenGameHotUpdate")]
    public bool openGameHp;

    // 是否打开debug
    [XmlElementAttribute("OpenDebugLog")]
    public bool openDebugLog;

    // 是否打开网络debug
    [XmlElementAttribute("OpenNetDebugLog")]
    public bool openNetDebugLog;

    // 具体每个游戏对应的版本号
    [XmlElementAttribute("GameVer")]
    public XMLDictionary<int, string> gameVersion;

    // 是否打开多点触控
    [XmlElementAttribute("MultyTouchEnabled")]
    public bool multyTouchEnabled;

    // 是否打开多点触控
    [XmlElementAttribute("TargetFrameRate")]
    public int targetFrameRate;

    // 是否打开多点触控
    [XmlElementAttribute("KeepScreenLight")]
    public bool keepScreenLight;

    public TableGameConfig()
    {
        gameVersion = new XMLDictionary<int, string>();
    }
    
    /// <summary>
    /// 获取游戏产品版本号
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public string GetGameVersion(int productId)
    { 
        string version = string.Empty;
        if (!gameVersion.TryGetValue(productId, out version))
        {
            return null;
        }

        return version;
    }
}

/// <summary>
/// 大厅资源配置表
/// </summary>
[XmlRootAttribute("TableGameRes")]
[Serializable]
public class TableGameRes
{
    [XmlIgnore]
    public const string FILE_NAME = "TableGameRes.xml";

    [XmlElementAttribute("LobbyRes")]
    public LobbyResConfig lobbyResConfig;

    [XmlElementAttribute("GameRes")]
    public GameResConfig gameResConfig;

    public TableGameRes()
    {
        lobbyResConfig = new LobbyResConfig();
        gameResConfig = new GameResConfig();
    }
}

/// <summary>
/// 大厅资源配置表
/// </summary>
[Serializable]
public class LobbyResConfig
{
    // 版本号
    [XmlAttribute("Version")]
    public string version;

    // lua脚本
    [XmlElementAttribute("Scripts")]
    public ScriptRes scripts;

    // shader
    [XmlElementAttribute("Shader")]
    public ResData shader;

    // datas
    [XmlElementAttribute("Datas")]
    public ResData datas;

    // 公有资源
    [XmlElementAttribute("Common")]
    public BaseRes common;

    // 热更新资源
    [XmlElementAttribute("HotUpdate")]
    public BaseRes hotUpdate;

    // 登录资源
    [XmlElementAttribute("Login")]
    public BaseRes login;

    // 大厅资源
    [XmlElementAttribute("Lobby")]
    public BaseRes lobby;

    // 游戏公有资源
    [XmlElementAttribute("GameCmmon")]
    public BaseRes game;

    public LobbyResConfig()
    {
        scripts = new ScriptRes();

        common = new BaseRes();
        hotUpdate = new BaseRes();
        login = new BaseRes();
        lobby = new BaseRes();
        game = new BaseRes();
    }
}

/// <summary>
/// 具体资源配置表
/// </summary>
[Serializable]
public class GameResConfig
{
    [XmlElementAttribute("Products")]
    public XMLDictionary<int, BaseRes> products;

    public GameResConfig()
    {
        products = new XMLDictionary<int, BaseRes>();
    }

    /// <summary>
    /// 获取产品资源
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BaseRes GetOrAddProductRes(int id)
    {
        BaseRes res = null;
        if (!products.TryGetValue(id, out res))
        {
            res = new BaseRes();
            products.Add(id, res);
        }

        return res;
    }

    /// <summary>
    /// 查找产品资源
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BaseRes FindProductRes(int id)
    {
        BaseRes res = null;
        if (!products.TryGetValue(id, out res))
        {
            return null;
        }

        return res;
    }

    /// <summary>
    /// 重建游戏资源配置
    /// </summary>
    /// <param name="res"></param>
    /// <param name="productId"></param>
    public void RebuildGameRes(BaseRes res, int productId)
    {
        if (!products.ContainsKey(productId))
        {
            return;
        }

        products.Remove(productId);
        products.Add(productId, res);
    }
}

// 脚本资源
[Serializable]
public class ScriptRes
{
    // lua文件文件路径
    [XmlArrayAttribute("Lua")]
    public List<ResData> luaFiles;

    public ScriptRes()
    {
        luaFiles = new List<ResData>();
    }

    public void AddLuaFile(ResData data)
    {
        luaFiles.Add(data);
    }
}

[Serializable]
public class BaseRes
{
    // 版本号
    [XmlAttribute("Version")]
    public string version;

    // 大厅图集
    [XmlArrayAttribute("Atlas")]
    public List<ResData> atlas;
    // 贴图
    [XmlArrayAttribute("Textures")]
    public List<ResData> textures;
    // 字体
    [XmlArrayAttribute("Fonts")]
    public List<ResData> fonts;
    // 声音
    [XmlArrayAttribute("Audios")]
    public List<ResData> audios;
    // UI预制
    [XmlArrayAttribute("Prefabs")]
    public List<ResData> prefabs;

    public BaseRes()
    {
        atlas = new List<ResData>();
        textures = new List<ResData>();
        fonts = new List<ResData>();
        audios = new List<ResData>();
        prefabs = new List<ResData>();
    }

    public void AddAtlas(ResData data)
    {
        atlas.Add(data);
    }

    public void AddTexture(ResData texture)
    {
        textures.Add(texture);
    }

    public void AddFonts(ResData data)
    {
        fonts.Add(data);
    }

    public void AddAudio(ResData data)
    {
        audios.Add(data);
    }

    public void AddPrefabs(ResData prefab)
    {
        prefabs.Add(prefab);
    }
}
