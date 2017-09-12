using UnityEngine;

public class CSMainGame : UIWindowBase 
{
    protected override void InitWindowData()
    {
        this.WindowID = GameWindowID.WINDOWID_MAIN_GAME;
		this.windowData.forceClearNavigation = true;
		this.windowData.windowType = UIWindowType.Normal;
		this.windowData.showMode = UIWindowShowMode.DestoryOtherWindow;
    }

    protected override void InitWindowOnAwake()
    {
        GameObject buyCoinBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/BuyCoinBtn").gameObject;
        GameObject backMenuBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/BackMenuBtn").gameObject;

		UIEventListener.Get(buyCoinBtn).onClick = OnBuyCoinBtnClick;
        UIEventListener.Get(backMenuBtn).onClick = OnBackMenuBtnClick;
    }

    private void OnBuyCoinBtnClick(GameObject sender)
    {
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_COIN_SHOP);
    }

    private void OnBackMenuBtnClick(GameObject sender)
    {
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_MAIN_MENU);
    }
}
