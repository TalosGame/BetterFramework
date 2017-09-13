using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 排行榜示例界面
/// 1. 给出了Scroll View控件，循环复用的例子
/// 2. OnContentUpdate 控件内容有更新会回调这个接口
/// 3. OnContentClick 控件内的Item有点击事件的情况会回调这个接口
/// </summary>
public class CSPlayerScore : UIWindowBase
{
    private UILoopScrollView loopScrollView;

    protected override void InitWindowData()
    {
		this.WindowID = GameWindowID.WINDOWID_PLAYER_SCORE;
        this.windowData.forceClearNavigation = false;
		this.windowData.windowType = UIWindowType.Normal;
        this.windowData.showMode = UIWindowShowMode.HideOtherWindow;
    }

    protected override void InitWindowOnAwake()
    {
        loopScrollView = MonoExtendUtil.FindDeepChild<UILoopScrollView>(this.gameObject, "DynamicPanel/ScorePanel");
        GameObject backBtn = MonoExtendUtil.FindDeepChild(this.gameObject, "DynamicPanel/BackBtn").gameObject;

		loopScrollView.onContentUpdate = OnContentUpdate;
		loopScrollView.onContentClick = OnContentClick;
        UIEventListener.Get(backBtn).onClick = OnBackBtnClick;
    }

    protected override void OnShowWindow(ShowWindowData? data = null)
    {
        NotificationCenter.Instance.AddObserver(this, GameConst.NOTIFY_HANDLE_GET_SCORE, HandleGetScore);
    }

    protected override void OnHideWindow()
    {
        NotificationCenter.Instance.RemoveObserver(this);
    }

    private void HandleGetScore(Notification notify)
    {
        List<object> scores = (List<object>)notify.Object;
        foreach(PlayerScore score in scores)
        {
            Debug.Log("Player socre:" + score);
        }

        loopScrollView.SetData(scores);
    }

    public void OnContentUpdate(GameObject scrollView, GameObject go, object data)
    {
        PlayerScore itemData = data as PlayerScore;

		UILabel label = MonoExtendUtil.FindDeepChild<UILabel>(go, "ItemLab");
        label.text = itemData.name + ": " + itemData.score;

		Debug.Log("item text:" + label.text);
    }

    public void OnContentClick(GameObject scrollView, GameObject item, GameObject target)
    {
		UILabel label = item.GetComponentInChildren<UILabel>();
		Debug.Log("Item name:" + label.text);
    }

    private void OnBackBtnClick(GameObject sender)
    {
        UIManager.Instance.ReturnWindow();
    }
}
