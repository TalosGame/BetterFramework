using System;
using System.Collections.Generic;

/// <summary>
/// UI窗口数据类
/// 1. 窗口类型
/// 2. 显示模式
/// </summary>
public struct UIWindowData
{
	// 是否清理导航窗口(到该界面需要重置导航信息)
	public bool forceClearNavigation;
	// 窗口类型
	public UIWindowType windowType;
	// 窗口显示类型
	public UIWindowShowMode showMode;

	static public UIWindowData Create()
	{
		UIWindowData data = new UIWindowData ();
		data.forceClearNavigation = false;
		data.windowType = UIWindowType.Normal;
		data.showMode = UIWindowShowMode.DoNothing;

		return data;
	}
}

/// <summary>
/// 显示窗口数据
/// </summary>
public struct ShowWindowData
{
	// Reset窗口
	public bool forceResetWindow;
	// Clear导航信息
	public bool forceClearBackSeqData;
	// 是否执行窗口层级管理逻辑
	public bool executeNavLogic;
	// 是否检查层级
	public bool checkNavigation;
	// 是否隐藏所有窗口
	public bool hideAllOtherWindow;
	// 额外数据
	public object param;

	static public ShowWindowData Create()
	{
		ShowWindowData data = new ShowWindowData();
		data.forceResetWindow = false;
		data.forceClearBackSeqData = false;
		data.executeNavLogic = true;
		data.checkNavigation = false;
		data.hideAllOtherWindow = true;
        data.param = null;

		return data;
	}
}

public class BackWindowSequenceData
{
	public UIWindowBase hideTargetWindow;
	public List<int> backShowTargets;
}
