local LUIMainMenu = {}
LUIMainMenu.__index = LUIMainMenu

function LUIMainMenu.New(cls)
	print ("LUIMainMenu:New", cls)
	
	local a = {}
	setmetatable(a, cls)
	return a
end

function LUIMainMenu:InitWindowData( )
	self.windowId = GameWindowID.WINDOWID_MAIN_MENU;
	self.forceClearNavigation = true;
	self.windowType = UIWindowType.Normal;
	self.showMode = UIWindowShowMode.DestoryOtherWindow;
end

function LUIMainMenu:InitWindowOnAwake()
	local enterGameBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/EnterGameBtn").gameObject;

	LuaHelper.AddButtonClick(enterGameBtn, self.OnEnterBtnClick);
end

LUIMainMenu.OnEnterBtnClick = function ( )	
	LuaHelper.ShowWindow(GameWindowID.WINDOWID_MAIN_GAME);

    -- DestoryOtherWindow 包括固定窗口
    -- 这里尝试给另外一个窗口传递参数
    local winData = CreateWindowData();
    winData.param = LUserManager:GetUserCoin();

	LuaHelper.ShowWindow(GameWindowID.WINDOWID_USER_INFO_BAR, winData);
end

return LUIMainMenu