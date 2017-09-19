local LUserInfoBar = {}
LUserInfoBar.__index = LUserInfoBar;

function LUserInfoBar.New(cls)
	print ("LUserInfoBar:New", cls);
	
	local a = {};
	setmetatable(a, cls);
	return a;
end

function LUserInfoBar:InitWindowData( )
	self.windowId = GameWindowID.WINDOWID_USER_INFO_BAR;
	self.forceClearNavigation = false;
    self.windowType = UIWindowType.Fixed;
	self.showMode = UIWindowShowMode.DoNothing;
end

function LUserInfoBar:InitWindowOnAwake( )
	self.coinLab = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/CoinNumLab").gameObject;
end

function LUserInfoBar:OnShowWindow(param)
	if param ~= nil then
		print("current coin:"..param)
		self:RefreshCoin(param);
	end

	ViewEvent:AddListener(EventConst.NotifyUpdatePlayerInfo, self.HandleBuyCoin, self);
end

function LUserInfoBar:OnHideWindow()
    ViewEvent:RemoveListener(EventConst.NotifyUpdatePlayerInfo, self.HandleBuyCoin, self)
end

function LUserInfoBar:HandleBuyCoin( userBean ) 
    self:RefreshCoin(userBean.coin);
end

function LUserInfoBar:RefreshCoin(coin)
    LuaHelper.SetUILabelText(self.coinLab, tostring(coin));
end

return LUserInfoBar