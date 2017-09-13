using UnityEngine;

public class CSMsgBox : UIWindowBase 
{
    protected override void InitWindowData()
    {
        this.WindowID = GameWindowID.WINDOWID_MESSAGE_BOX;
		this.windowData.forceClearNavigation = false;
        this.windowData.windowType = UIWindowType.Custom;
        this.windowData.showMode = UIWindowShowMode.DoNothing;
    }

    protected override void InitWindowOnAwake()
    {
        GameObject okBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/OkBtn").gameObject;

        UIEventListener.Get(okBtn).onClick = OnOkBtnClick;
    }

    private void OnOkBtnClick(GameObject sender)
    {
        // 自定义窗口需要手动关闭
        UIManager.Instance.CloseWindow(GameWindowID.WINDOWID_MESSAGE_BOX);
    }
}
