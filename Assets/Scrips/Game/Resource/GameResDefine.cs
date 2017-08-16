// 作者：wangquan
// 邮箱：wangquancomi@gmail.com
// QQ ：408310416
// 时间：2017/8/16/15:08
// 类名：GameResDefine

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
    public const int RES_LUA = RES_AUDIO + 1;           // lua资源
}

public class GameResDefine : ResourceDefine
{
    public override void Init()
    {
        // 注册窗口ID
        RegisterWindow(UIWindowID.WINDOWID_HOT_RES_LOADING);

        // 注册窗口资源路径
        AddExUIWindow(UIWindowID.WINDOWID_HOT_RES_LOADING, "NormalWindow/UIHotUpLoading");

        // 添加资源搜索路径
        AddExResourcePath(ResourceType.RES_UI, "UI");
        AddExResourcePath(ResourceType.RES_DATAS, "Datas");
        AddExResourcePath(ResourceType.RES_AUDIO, "Sounds");
        AddExResourcePath(GameResourceType.RES_LUA, "lua");
    }
}

