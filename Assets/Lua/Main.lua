require "Define"

--主入口函数。从这里开始lua逻辑
function Main()
	print("===Main===");

	ResDefine:Init();

	LuaHelper.ShowWindow(GameWindowID.WINDOWID_MAIN_MENU);

end