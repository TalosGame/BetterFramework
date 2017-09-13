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
        GameObject scoreBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/PlayerScoreBtn").gameObject;
        GameObject msgBoxBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/MsgBoxBtn").gameObject;
        GameObject backMenuBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/BackMenuBtn").gameObject;

		UIEventListener.Get(buyCoinBtn).onClick = OnBuyCoinBtnClick;
        UIEventListener.Get(scoreBtn).onClick = OnScoreBtnClick;
        UIEventListener.Get(msgBoxBtn).onClick = OnMsgBoxBtnClick;
        UIEventListener.Get(backMenuBtn).onClick = OnBackMenuBtnClick;
    }

    private void OnBuyCoinBtnClick(GameObject sender)
    {
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_COIN_SHOP);
    }

    private void OnScoreBtnClick(GameObject sender)
    {
        CSUserManager.Instance.UpdateScores();
    }

    private void OnMsgBoxBtnClick(GameObject sender)
    {
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_MESSAGE_BOX);
    }

    private void OnBackMenuBtnClick(GameObject sender)
    {
        UIManager.Instance.ShowWindow(GameWindowID.WINDOWID_MAIN_MENU);
    }
}
