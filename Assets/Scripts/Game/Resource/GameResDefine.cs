// Author：wangquan
// Mail  ：wangquancomi@gmail.com
// QQ    ：408310416
// Date  ：2017/8/16/16:21
// Class ：GameResDefine

/// <summary>
/// 游戏中UI窗口定义
/// </summary>
public class GameWindowID : UIWindowID
{
    public const int WINDOWID_MAIN_MENU = WINDOWID_HOT_RES_LOADING + 1;
    public const int WINDOWID_MAIN_GAME = WINDOWID_MAIN_MENU + 1;
    public const int WINDOWID_COIN_SHOP = WINDOWID_MAIN_GAME + 1;
    public const int WINDOWID_USER_INFO_BAR = WINDOWID_COIN_SHOP + 1;
}

/// <summary>
/// 游戏中的资源类型定义
/// </summary>
public class GameResourceType : ResourceType
{
    // 人物贴图资源
    public const int RES_ROLE = RES_LUA + 1;
}

public class GameResDefine : ResourceDefine
{
    public override void InitUIWindows()
    {
		//注册窗口ID
		RegisterWindow(UIWindowID.WINDOWID_HOT_RES_LOADING);
        RegisterWindow(GameWindowID.WINDOWID_MAIN_MENU);
        RegisterWindow(GameWindowID.WINDOWID_MAIN_GAME);
        RegisterWindow(GameWindowID.WINDOWID_COIN_SHOP);
        RegisterWindow(GameWindowID.WINDOWID_USER_INFO_BAR);

		// 注册窗口资源路径
		AddExUIWindow(UIWindowID.WINDOWID_HOT_RES_LOADING, "NormalWindow/UIHotUpLoading");
        AddExUIWindow(GameWindowID.WINDOWID_MAIN_MENU, "NormalWindow/CSMainMenu");
        AddExUIWindow(GameWindowID.WINDOWID_MAIN_GAME, "NormalWindow/CSMainGame");
        AddExUIWindow(GameWindowID.WINDOWID_COIN_SHOP, "PopUpWindow/CSCoinShop");
        AddExUIWindow(GameWindowID.WINDOWID_USER_INFO_BAR, "FixedWidow/CSUserInfoBar");
	}

    public override void InitResPaths()
    {
		// 添加资源搜索路径
		AddExResourcePath(ResourceType.RES_UI, "UI");
        AddExResourcePath(ResourceType.RES_META_DATAS, "Metas");
		AddExResourcePath(ResourceType.RES_AUDIO, "Audios");
		AddExResourcePath(ResourceType.RES_LUA, "lua");

        AddExResourcePath(GameResourceType.RES_ROLE, "Role");

        // TODO add other res path
	}


}

