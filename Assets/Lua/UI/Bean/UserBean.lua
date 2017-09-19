local UserBean = Baseclass:New();

function UserBean:Init( )
	-- Init property here
	self.coin = 0;
	self.scores = {};
end

function UserBean:GetCoin( )
	return self.coin;
end

function UserBean:SetCoin( coin )
	self.coin = coin;
	
	ViewEvent:Brocast(EventConst.NotifyUpdatePlayerInfo, self);
end

function UserBean:GetScores( )
	return self.scores;
end

function UserBean:CleanScores( )
	self.scores = {};
end

function UserBean:AddScore( score )
	table.insert(self.scores, score);
end

return UserBean;

-- public class PlayerScore
-- {
--     public string name;
--     public int score;
-- }

-- public class UserInfoBean
-- {
--     private int coin;
--     public int Coin
--     {
--         get{ return coin; }
--         set{ coin = value; }
--     }

--     private List<object> scores = new List<object>();
-- 	public List<object> Scores
-- 	{
-- 		get{ return scores; }
-- 		set{ scores = value; }
-- 	}
-- }