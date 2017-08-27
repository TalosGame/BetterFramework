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
    // 转菊花通用窗口ID
    public const int WINDOWID_CONNECT = 20;
}

/// <summary>
/// 游戏中的资源类型定义
/// </summary>
public class GameResourceType : ResourceType
{
    public const int RES_LEVEL_OBJECTS = RES_LUA + 1;           // 场景对象

}

public class GameResDefine : ResourceDefine
{
    public override void InitUIWindows()
    {
		//注册窗口ID
		RegisterWindow(UIWindowID.WINDOWID_HOT_RES_LOADING);

		// 注册窗口资源路径
		AddExUIWindow(UIWindowID.WINDOWID_HOT_RES_LOADING, "NormalWindow/UIHotUpLoading");
	}

    public override void InitResPaths()
    {
		// 添加资源搜索路径
		AddExResourcePath(ResourceType.RES_UI, "UI");
		AddExResourcePath(ResourceType.RES_DATAS, "Datas");
		AddExResourcePath(ResourceType.RES_AUDIO, "Sounds");
		AddExResourcePath(ResourceType.RES_LUA, "lua");
		AddExResourcePath(GameResourceType.RES_LEVEL_OBJECTS, "LevelObjs");
	}
}

