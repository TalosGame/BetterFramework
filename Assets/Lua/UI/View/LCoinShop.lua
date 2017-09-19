local LCoinShop = {}
LCoinShop.__index = LCoinShop

function LCoinShop.New(cls)
	print ("LCoinShop:New", cls)
	
	local a = {}
	setmetatable(a, cls)
	return a
end

function LCoinShop:InitWindowData( )
	self.windowId = GameWindowID.WINDOWID_COIN_SHOP;
    self.forceClearNavigation = false;
    self.windowType = UIWindowType.PopUp;
    self.showMode = UIWindowShowMode.DoNothing;
end

function LCoinShop:InitWindowOnAwake()
	local buyBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/BuyBtn").gameObject;
    local cancelBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/CancelBtn").gameObject;

    LuaHelper.AddButtonClick(buyBtn, self.OnBuyBtnClick);
    LuaHelper.AddButtonClick(cancelBtn, self.OnCancelBtnClick);
end

LCoinShop.OnBuyBtnClick = function ( )
	LUserManager:BuyCoin();
    LuaHelper.ReturnWindow();
end

LCoinShop.OnCancelBtnClick = function ( )
	LuaHelper.ReturnWindow();
end

return LCoinShop