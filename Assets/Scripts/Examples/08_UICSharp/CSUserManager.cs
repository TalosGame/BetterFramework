using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfoBean
{
    private int coin;
    public int Coin
    {
        get{ return coin; }
        set{ coin = value; }
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
}
