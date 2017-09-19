local ResDefine = ResDefine or {}

-- 资源常量定义
GameWindowID = 
{
	WINDOWID_MAIN_MENU = 100,		-- 主菜单
	WINDOWID_MAIN_GAME = 101;		-- 游戏
    WINDOWID_COIN_SHOP = 102;
    WINDOWID_USER_INFO_BAR = 103;
    WINDOWID_PLAYER_SCORE = 104;
    WINDOWID_MESSAGE_BOX = 105;
}

-- 窗口类型
UIWindowType =
{
	Normal = 0,	  -- 普通窗口
	Fixed = 1,    -- 固定窗口
	PopUp = 2,    -- 弹出窗口
	Custom = 3,	  -- 自定义窗口
}

-- 窗口显示类型
UIWindowShowMode =
{
	DoNothing = 0,
	HideOtherWindow = 1,
	DestoryOtherWindow = 2,
}

function CreateWindowData( )
	local data = {}
	data.forceResetWindow = false;
	data.forceClearBackSeqData = false;
	data.executeNavLogic = true;
	data.checkNavigation = false;
	data.hideAllOtherWindow = true;
	data.param = nil;

	return data;
end

function ResDefine:Init( )
	Debugger.Log("ResDefine:Init!");

	self:RegisterWindows();
	self:InitWindowResourcePath();
	self:InitGameResourcePath();
end

function ResDefine:RegisterWindows(  )

	LuaHelper.RegisterWindow(GameWindowID.WINDOWID_MAIN_MENU);
	LuaHelper.RegisterWindow(GameWindowID.WINDOWID_MAIN_GAME);
    LuaHelper.RegisterWindow(GameWindowID.WINDOWID_COIN_SHOP);
    LuaHelper.RegisterWindow(GameWindowID.WINDOWID_USER_INFO_BAR);
    LuaHelper.RegisterWindow(GameWindowID.WINDOWID_PLAYER_SCORE);
    LuaHelper.RegisterWindow(GameWindowID.WINDOWID_MESSAGE_BOX);
end

function ResDefine:InitWindowResourcePath( )

	-- normal window
	LuaHelper.AddWindowResPath(GameWindowID.WINDOWID_MAIN_MENU, "NormalWindow/Lua/LuaMainMenu");
	LuaHelper.AddWindowResPath(GameWindowID.WINDOWID_MAIN_GAME, "NormalWindow/Lua/LMainGame");
	LuaHelper.AddWindowResPath(GameWindowID.WINDOWID_PLAYER_SCORE, "NormalWindow/Lua/LPlayerScore");

	-- pop up window
	LuaHelper.AddWindowResPath(GameWindowID.WINDOWID_COIN_SHOP, "PopUpWindow/Lua/LCoinShop");

	-- fixed window
	LuaHelper.AddWindowResPath(GameWindowID.WINDOWID_USER_INFO_BAR, "FixedWidow/Lua/LUserInfoBar");

	-- custom window
	LuaHelper.AddWindowResPath(GameWindowID.WINDOWID_MESSAGE_BOX, "CustomWindow/Lua/LMsgBox");
end

function ResDefine:InitGameResourcePath( )


end

return ResDefine;

