local LUserManager = LUserManager or {}

function LUserManager:Init( )
	self.userBean = UserBean:New();
	self.userBean:Init();
end

function LUserManager:GetUserCoin( )
	return self.userBean:GetCoin();
end

function LUserManager:BuyCoin()
	self.userBean:SetCoin(self.userBean:GetCoin() + 100);
end

function LUserManager:UpdateScores()
    self.userBean:CleanScores();

    --math.randomseed(os.time());
    local baseScore = math.random(100);

    for i=1, 10 do
        local playerScore = {}
        playerScore.name = "Player "..i;
        playerScore.score = baseScore + i;

        self.userBean:AddScore(playerScore);
    end

    --这里测试下打开窗口，通知消息的调用顺序是否正常
    LuaHelper.ShowWindow(GameWindowID.WINDOWID_PLAYER_SCORE);

    ViewEvent:Brocast(EventConst.NotifyUpdateScores, self.userBean);
end

LUserManager:Init();

return LUserManager;