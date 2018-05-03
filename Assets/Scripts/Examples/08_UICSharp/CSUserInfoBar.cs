using UnityEngine;

public class CSUserInfoBar : UIWindowBase, INotification
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

	protected override void OnShowWindow(object data = null)
    {
        if(data != null)
        {
			RefreshCoin((int)data);
        }

        NotificationCenter.Instance.AddObserver(GameConst.NOTIFY_HANDLE_BUY_COIN, this);
    }

    protected override void OnHideWindow()
    {
        NotificationCenter.Instance.RemoveObserver(this);
    }

    public void NotificationHandler(Notification notify) 
    {
        var evtName = notify.name;
        if (evtName == GameConst.NOTIFY_HANDLE_BUY_COIN) 
        {
            HandleBuyCoin(notify);
            return;
        }
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
