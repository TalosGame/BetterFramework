local LPlayerScore = {}
LPlayerScore.__index = LPlayerScore

function LPlayerScore.New(cls)
	print ("LPlayerScore:New", cls)
	
	local a = {}
	setmetatable(a, cls)
	return a
end

function LPlayerScore:InitWindowData( )
	self.windowId = GameWindowID.WINDOWID_PLAYER_SCORE;
    self.forceClearNavigation = false;
	self.windowType = UIWindowType.Normal;
    self.showMode = UIWindowShowMode.HideOtherWindow;
end

function LPlayerScore:InitWindowOnAwake()

	self.loopScrollView = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/ScorePanel").gameObject;
    local backBtn = MonoExtendUtil.FindDeepChild(self.gameObject, "DynamicPanel/BackBtn").gameObject;

    local contentUpdateCallBack = function ( loopScrollView, item, data )
		self:ContentUpdate(loopScrollView, item, data);
	end
	LuaHelper.AddScrollViewContentUpdate(self.loopScrollView, contentUpdateCallBack)

	local contentClickCallBack = function ( loopScrollView, item, data )
		self:ContentClick(loopScrollView, item, data);
	end
	LuaHelper.AddScrollViewItemClick(self.loopScrollView, contentClickCallBack)

	LuaHelper.AddButtonClick(backBtn, self.OnBackBtnClick);
end

function LPlayerScore:OnShowWindow( param )
    ViewEvent:AddListener(EventConst.NotifyUpdateScores, self.HandleGetScore, self);
end

function LPlayerScore:OnHideWindow()
    ViewEvent:RemoveListener(EventConst.NotifyUpdateScores, self.HandleGetScore, self)
end

function LPlayerScore:ContentUpdate( scrollView, item, data )
	local label = MonoExtendUtil.FindDeepChild(item, "ItemLab").gameObject;
    LuaHelper.SetUILabelText(label, data.name..": "..data.score);
end

function LPlayerScore:ContentClick( scrollView, item, data )
	print("Item name: "..data.name);
end

function LPlayerScore:HandleGetScore( userBean )
	local scores = userBean:GetScores();
	-- for i,v in ipairs(scores) do
	-- 	print("Player socre:" .. v.score);
	-- end

	LuaHelper.SetScrollViewData(self.loopScrollView, scores);
end

LPlayerScore.OnBackBtnClick = function ( )
	LuaHelper.ReturnWindow();
end

return LPlayerScore