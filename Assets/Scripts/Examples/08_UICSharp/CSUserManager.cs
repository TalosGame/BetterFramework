using System.Collections;
using System.Collections.Generic;

public class PlayerScore
{
    public string name;
    public int score;
}

public class UserInfoBean
{
    private int coin;
    public int Coin
    {
        get{ return coin; }
        set{ coin = value; }
    }

    private List<object> scores = new List<object>();
	public List<object> Scores
	{
		get{ return scores; }
		set{ scores = value; }
	}
}

public class CSUserManager : SingletonBase<CSUserManager>
{
    private UserInfoBean userInfoBean;

    protected override void Init()
    {
        userInfoBean = new UserInfoBean();
    }

    public int GetUserCoin()
    {
        return userInfoBean.Coin;
    }

    public void BuyCoin()
    {
        userInfoBean.Coin += 100;
        NotificationCenter.Instance.PostNotification(GameConst.NOTIFY_HANDLE_BUY_COIN, userInfoBean);
    }

    public void UpdateScores()
    {
        List<object> scores = userInfoBean.Scores;
        scores.Clear();

        int baseScore = MathUtil.RandomInt(100);
        for (int i = 0; i < 10; i++)
        {
            PlayerScore playerScore = new PlayerScore();
            playerScore.name = string.Format("Player[{0}]", i);
            playerScore.score = baseScore + i;

            scores.Add(playerScore);
        }

        // 这里测试下打开窗口，通知消息的调用顺序是否正常
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_PLAYER_SCORE);
        NotificationCenter.Instance.PostNotification(GameConst.NOTIFY_HANDLE_GET_SCORE, scores);
    }
}
