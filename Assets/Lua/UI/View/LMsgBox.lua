local LMsgBox = {}
LMsgBox.__index = LMsgBox

function LMsgBox.New(cls)
	print ("LMsgBox:New", cls)
	
	local a = {}
	setmetatable(a, cls)
	return a
end

function LMsgBox:InitWindowData( )
	self.windowId = GameWindowID.WINDOWID_MESSAGE_BOX;
	self.forceClearNavigation = false;
    self.windowType = UIWindowType.Custom;
    self.showMode = UIWindowShowMode.DoNothing;
end

function LMsgBox:InitWindowOnAwake()

	local okBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/OkBtn").gameObject;

    LuaHelper.AddButtonClick(okBtn, self.OnOkBtnClick);
end

LMsgBox.OnOkBtnClick = function( )
	--自定义窗口需要手动关闭
	LuaHelper.CloseWindow(GameWindowID.WINDOWID_MESSAGE_BOX);
end

return LMsgBox