using UnityEngine;

public class CSUserInfoBar : UIWindowBase 
{
    private UILabel coinLab;

    protected override void InitWindowData()
    {
		this.WindowID = GameWindowID.WINDOWID_USER_INFO_BAR;
		this.windowData.forceClearNavigation = false;
        this.windowData.windowType = UIWindowType.Fixed;
		this.windowData.showMode = UIWindowShowMode.DoNothing;
    }

    protected override void InitWindowOnAwake()
    {
        coinLab = MonoExtendUtil.FindDeepChild<UILabel>(this.gameObject, "DynamicPanel/CoinNumLab");
    }

    protected override void OnShowWindow(ShowWindowData? data = null)
    {
        if(data != null)
        {
            ShowWindowData winData = data.Value;
            RefreshCoin((int)winData.param);
        }

        NotificationCenter.Instance.AddObserver(this, GameConst.NOTIFY_HANDLE_BUY_COIN, HandleBuyCoin);
    }

    protected override void OnHideWindow()
    {
        NotificationCenter.Instance.RemoveObserver(this);
    }

    private void HandleBuyCoin(Notification notify)
    {
        UserInfoBean userInfoBean = (UserInfoBean)notify.Object;
        RefreshCoin(userInfoBean.Coin);
    }

    private void RefreshCoin(int coin)
    {
        coinLab.text = "" + coin;
    }
}
