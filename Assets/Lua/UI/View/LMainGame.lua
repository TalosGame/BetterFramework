local LMainGame = {}
LMainGame.__index = LMainGame

function LMainGame.New(cls)
	print ("LMainGame:New", cls)
	
	local a = {}
	setmetatable(a, cls)
	return a
end

function LMainGame:InitWindowData( )
	self.windowId = GameWindowID.WINDOWID_MAIN_GAME;
	self.forceClearNavigation = true;
	self.windowType = UIWindowType.Normal;
	self.showMode = UIWindowShowMode.DestoryOtherWindow;
end

function LMainGame:InitWindowOnAwake()
	local buyCoinBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/BuyCoinBtn").gameObject;
    local scoreBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/PlayerScoreBtn").gameObject;
    local msgBoxBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/MsgBoxBtn").gameObject;
    local backMenuBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/BackMenuBtn").gameObject;

    LuaHelper.AddButtonClick(buyCoinBtn, self.OnBuyCoinBtnClick);
    LuaHelper.AddButtonClick(scoreBtn, self.OnScoreBtnClick);
    LuaHelper.AddButtonClick(msgBoxBtn, self.OnMsgBoxBtnClick);
    LuaHelper.AddButtonClick(backMenuBtn, self.OnBackMenuBtnClick);
end

LMainGame.OnBuyCoinBtnClick = function( )
    LuaHelper.ShowWindow(GameWindowID.WINDOWID_COIN_SHOP);
end

LMainGame.OnScoreBtnClick = function( )
    LUserManager:UpdateScores();
end

LMainGame.OnMsgBoxBtnClick = function( )
	LuaHelper.ShowWindow(GameWindowID.WINDOWID_MESSAGE_BOX);
end

LMainGame.OnBackMenuBtnClick = function( )
	LuaHelper.ShowWindow(GameWindowID.WINDOWID_MAIN_MENU);
end

return LMainGame

