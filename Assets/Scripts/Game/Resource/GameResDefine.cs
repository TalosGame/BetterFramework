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
    public const int WINDOWID_PLAYER_SCORE = WINDOWID_USER_INFO_BAR + 1;
    public const int WINDOWID_MESSAGE_BOX = WINDOWID_PLAYER_SCORE + 1;
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
        RegisterWindow(GameWindowID.WINDOWID_PLAYER_SCORE);
        RegisterWindow(GameWindowID.WINDOWID_MESSAGE_BOX);

		// 注册窗口资源路径
		#region normal window
		AddExUIWindow(UIWindowID.WINDOWID_HOT_RES_LOADING, "NormalWindow/CSharp/UIHotUpLoading");
		AddExUIWindow(GameWindowID.WINDOWID_MAIN_MENU, "NormalWindow/CSharp/CSMainMenu");
		AddExUIWindow(GameWindowID.WINDOWID_MAIN_GAME, "NormalWindow/CSharp/CSMainGame");
		AddExUIWindow(GameWindowID.WINDOWID_PLAYER_SCORE, "NormalWindow/CSharp/CSPlayerScore");
		#endregion

		#region pop up window
		AddExUIWindow(GameWindowID.WINDOWID_COIN_SHOP, "PopUpWindow/CSharp/CSCoinShop");
		#endregion

		#region fixed window
		AddExUIWindow(GameWindowID.WINDOWID_USER_INFO_BAR, "FixedWidow/CSharp/CSUserInfoBar");
		#endregion

		#region custom window
		AddExUIWindow(GameWindowID.WINDOWID_MESSAGE_BOX, "CustomWindow/CSharp/CSMsgBox");
		#endregion
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

