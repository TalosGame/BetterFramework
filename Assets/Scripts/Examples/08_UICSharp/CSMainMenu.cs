using UnityEngine;

public class CSMainMenu : UIWindowBase 
{
    protected override void InitWindowData()
    {
        this.WindowID = GameWindowID.WINDOWID_MAIN_MENU;
        this.windowData.forceClearNavigation = true;
        this.windowData.windowType = UIWindowType.Normal;
        this.windowData.showMode = UIWindowShowMode.DestoryOtherWindow;
    }

    protected override void InitWindowOnAwake()
    {
        GameObject enterGameBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/EnterGameBtn").gameObject;

        UIEventListener.Get(enterGameBtn).onClick = OnEnterBtnClick;
    }

    private void OnEnterBtnClick(GameObject sender)
    {
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_MAIN_GAME);

        // DestoryOtherWindow 包括固定窗口
        // 这里尝试给另外一个窗口传递参数
        ShowWindowData winData = ShowWindowData.Create();
        winData.param = CSUserManager.Instance.GetUserCoin();
		UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_USER_INFO_BAR, winData);
    }
}
