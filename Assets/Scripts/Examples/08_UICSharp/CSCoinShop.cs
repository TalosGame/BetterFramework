using UnityEngine;

public class CSCoinShop : UIWindowBase 
{
    protected override void InitWindowData()
    {
		this.WindowID = GameWindowID.WINDOWID_COIN_SHOP;
        this.windowData.forceClearNavigation = false;
        this.windowData.windowType = UIWindowType.PopUp;
        this.windowData.showMode = UIWindowShowMode.DoNothing;
    }

    protected override void InitWindowOnAwake()
    {
        GameObject buyBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/BuyBtn").gameObject;
        GameObject cancelBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/CancelBtn").gameObject;

        UIEventListener.Get(buyBtn).onClick = OnBuyBtnClick;
        UIEventListener.Get(cancelBtn).onClick = OnCancelBtnClick;
    }

    private void OnBuyBtnClick(GameObject sender)
    {
        CSUserManager.Instance.BuyCoin();
        UIManager.Instance.ReturnWindow();
    }

    private void OnCancelBtnClick(GameObject sender)
    {
        UIManager.Instance.ReturnWindow();
    }
}
