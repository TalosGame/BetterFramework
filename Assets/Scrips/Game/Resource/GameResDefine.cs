using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class GameWindowID : UIWindowID
{
    // 转菊花通用窗口ID
    public const int WINDOWID_CONNECT = 20;
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
        AddExResourcePath(ResourceType.RES_LUA, "lua");
    }
}

