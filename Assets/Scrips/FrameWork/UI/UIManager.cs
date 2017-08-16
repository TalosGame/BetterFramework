using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

public class UIWindowID
{
	public const int WINDOWID_INVAILD = 0;                              // 不合法窗口
	public const int WINDOWID_HOT_RES_LOADING = WINDOWID_INVAILD + 1;   // 热更新资源loading界面
}

/// <summary>
/// UI管理基类
/// </summary>
public class UIManager : MonoSingleton<UIManager>
{
	// 层级分离depth
	private const int FIXED_WINDOW_DEPTH = 100;     // 界面固定window起始depth
	private const int POPUP_WINDOW_DEPTH = 150;     // PopUp类型window起始depth
	private const int NORMAL_WINDOW_DEPTH = 2;      // Normal类型window起始depth

	/// <summary>
	/// UI 根节点
	/// </summary>
	[SerializeField]
	protected Transform uiRoot;

	/// <summary>
	/// 正常推出窗口挂点
	/// </summary>
	[SerializeField]
	protected Transform uiNormalWindowRoot;
	public Transform UiNormalWindowRoot
	{
		get { return uiNormalWindowRoot; }
	}

	/// <summary>
	/// 弹出窗口挂点
	/// </summary>
	[SerializeField]
	protected Transform uiPopUpWindowRoot;
	public Transform UiPopUpWindowRoot
	{
		get { return uiPopUpWindowRoot; }
	}

	/// <summary>
	/// 固定窗口挂点
	/// </summary>
	[SerializeField]
	protected Transform uiFixedWidowRoot;
	public Transform UiFixedWidowRoot
	{
		get { return uiFixedWidowRoot; }
	}

    [SerializeField]
    protected Transform uiCustomWindowRoot;
    public Transform UiCustomWindowRoot
    {
        get { return uiCustomWindowRoot; }
    }

	// 所有窗口
	protected Dictionary<int, UIWindowBase> allWindows = new Dictionary<int, UIWindowBase>();
	// 所有显示的窗口
	protected Dictionary<int, UIWindowBase> showWindows = new Dictionary<int, UIWindowBase>();

	// 当前显示活跃窗口
	protected UIWindowBase curShownWindow = null;
	// 上一活跃窗口
	protected UIWindowBase lastShownWindow = null;
	// 窗口队列
	protected Stack<BackWindowSequenceData> backSequence = new Stack<BackWindowSequenceData>();

	// 管理的界面ID
	protected List<int> managedWindowIds = new List<int>();
	// 移除的界面ID
	private List<int> removedKey = new List<int> ();

	// 界面按MinDepth排序
	protected class CompareBaseWindow : IComparer<UIWindowBase>
	{
		public int Compare(UIWindowBase left, UIWindowBase right)
		{
			return left.MinDepth - right.MinDepth;
		}
	}
	protected CompareBaseWindow compareWindowFun = new CompareBaseWindow();

	void Awake()
	{
		if (uiRoot == null)
		{
			Debugger.LogError("UI Root must not be null!!!!");
			return;
		}

		// ui节点添加不能被销毁脚本
		uiRoot.GetOrAddComponent<DontDestroyOnLoad>();

		InitWindowManager();
	}

	/// <summary>
	/// 初始化当前界面管理类
	/// </summary>
	public void InitWindowManager()
	{
        removedKey.Clear();
        var enumerator = allWindows.GetEnumerator();
        while (enumerator.MoveNext())
        {
            UIWindowBase window = enumerator.Current.Value;
            UIWindowData data = window.windowData;

            // 自定义窗口不销毁
            if (data.windowType == UIWindowType.Custom)
                continue;

            removedKey.Add(window.WindowID);
        }

        for (int i = 0; i < removedKey.Count; i++)
        {
            allWindows.Remove(removedKey[i]);
            showWindows.Remove(removedKey[i]);
        }

	    backSequence.Clear();
	}

	#region 显示窗口功能
    public bool IsWindowShow(int windowId)
    {
        if (curShownWindow == null)
            return false;

        if (curShownWindow.WindowID == windowId)
            return true;

        return false;
    }

	/// <summary>
	/// 延迟显示界面
	/// </summary>
	/// <param name="windowID"></param>
	/// <param name="delay"></param>
	/// <param name="data"></param>
	public void ShowWindowDelay(int windowID, float delay, ShowWindowData ?data = null)
	{
		StartCoroutine(_ShowWindowDelay(windowID, delay, data));
	}

	private IEnumerator _ShowWindowDelay(int windowID, float delay, ShowWindowData ?data = null)
	{
		yield return new WaitForSeconds(delay);

		ShowWindow(windowID, data);
	}

	// 直接打开窗口
	private void ShowWindowForBack(int id)
	{
		// 检测控制权限
		if (!IsRegisterWindow(id))
		{
			Debugger.Log("UIManager has no control power of " + id.ToString());
			return;
		}

        if (showWindows.ContainsKey(id)) 
            return;

		UIWindowBase baseWindow = GetWindow(id);
		baseWindow.ShowWindowDirectly();
		showWindows[baseWindow.WindowID] = baseWindow;
	}

	/// <summary>
	/// 显示界面
	/// </summary>
	/// <param name="id">界面ID</param>
	public virtual void ShowWindow(int id, ShowWindowData ?data = null)
	{
		UIWindowBase baseWindow = ReadyToShowWindow(id, data);
		if (baseWindow != null)
		{
			RealShowWindow(baseWindow);
		}
	}

	/// <summary>
	/// 清空导航信息
	/// </summary>
	public void ClearBackSequence()
	{
		if (backSequence != null)
			backSequence.Clear();
	}

	private void RealShowWindow(UIWindowBase baseWindow)
	{
		baseWindow.ShowWindow();
		showWindows [baseWindow.WindowID] = baseWindow;

		if (baseWindow.IsNavigationWindow)
		{
			lastShownWindow = curShownWindow;
			curShownWindow = baseWindow;

			UIWindowData windowData = curShownWindow.windowData;
			if (lastShownWindow != null && lastShownWindow != curShownWindow && !windowData.forceClearNavigation)
                curShownWindow.PreWindowID = lastShownWindow.WindowID;

			Debugger.Log("<color=magenta>### current Navigation window </color>" + baseWindow.WindowID.ToString());
		}
	}

	protected UIWindowBase ReadyToShowWindow(int id, ShowWindowData ?showData = null)
	{
		// 检测控制权限
		if (!IsRegisterWindow(id))
		{
			Debugger.Log("UIManager has no control power of " + id.ToString());
			return null;
		}

		UIWindowBase showWindow = null;
		if (showWindows.TryGetValue (id, out showWindow)) {
			curShownWindow = showWindow;			
			return null;
		}

		UIWindowBase baseWindow = GetWindow(id);
		bool newAdded = false;
		if (baseWindow == null)
		{
			newAdded = true;

			// 判断窗口资源路径是否正确
			string prefabPath = MLResourceManager.Instance.GetWindowPrefabPath(id);
			if (string.IsNullOrEmpty(prefabPath))
				return null;

			GameObject uiObject = MLResourceManager.Instance.LoadInstance(prefabPath, ResourceType.RES_UI) as GameObject;
			if(uiObject != null)
			{
				//NGUITools.SetActive(uiObject, false);
				baseWindow = uiObject.GetComponent<UIWindowBase>();

				// 检查窗口id
				if (baseWindow.WindowID != id)
				{
					Debugger.LogError(string.Format("<color=cyan>[BaseWindowId :{0} != shownWindowId :{1}]</color>", 
                        baseWindow.WindowID, id));

					return null;
				}

				// 查询window类型
				Transform targetRoot = GetTargetRoot(baseWindow.windowData.windowType);

				// 挂在到相应的根节点上
				MonoExtendUtil.AddChildToTarget(targetRoot, uiObject.transform);

				allWindows[id] = baseWindow;
			}
		}

		if (baseWindow == null)
		{
			Debugger.LogError("[window instance is null.]" + id.ToString());
			return null;
		}

		// 重置界面(第一次添加，强制Reset)
		if (newAdded || (showData != null && showData.Value.forceResetWindow))
			baseWindow.ResetWindow();

		if (showData == null || (showData != null && showData.Value.executeNavLogic))
		{
			// 执行窗口层级逻辑
			ExecuteNavigationLogic(baseWindow, showData);
		}

		// 调整层级depth
		AdjustBaseWindowDepth(baseWindow);

		return baseWindow;
	}

	private void ExecuteNavigationLogic(UIWindowBase baseWindow, ShowWindowData ?showData)
	{
		UIWindowData windowData = baseWindow.windowData;

		if (baseWindow.IsNavigationWindow)
		{
			RefreshBackSequenceData(baseWindow);
		}
		else if (windowData.showMode == UIWindowShowMode.HideOtherWindow)
		{
			bool includeFixed = (showData == null ? true : showData.Value.hideAllOtherWindow);
			HideAllShownWindow(includeFixed);
		}

		// 强制清除窗口导航堆栈
		if (baseWindow.windowData.forceClearNavigation 
			|| (showData != null && showData.Value.forceClearBackSeqData))
		{
			Debugger.Log("<color=cyan>## [Enter the start window, reset the backSequenceData for the navigation system.]##</color>");
			ClearBackSequence();
		}
		else
		{
			if ((showData != null && showData.Value.checkNavigation))
				CheckBackSequenceData(baseWindow);
		}
	}

	// 刷新窗口队列
	private void RefreshBackSequenceData(UIWindowBase targetWindow)
	{
		UIWindowData windowData = targetWindow.windowData;

		if (showWindows == null || windowData.showMode == UIWindowShowMode.DoNothing)
			return;

        if (windowData.showMode == UIWindowShowMode.HideOtherWindow) 
        {
            HideOtherWindow(targetWindow);
            return;
        }

        if (windowData.showMode == UIWindowShowMode.DestoryOtherWindow)
        {
            DestoryOtherWindow(targetWindow);
            return;
        }
	}

    private void HideOtherWindow(UIWindowBase targetWindow) 
    {
        UIWindowData windowData = targetWindow.windowData;

        List<UIWindowBase> sortedHiddenWindows = new List<UIWindowBase>();
        removedKey.Clear();

        var enumerator = showWindows.GetEnumerator();
        while (enumerator.MoveNext())
        {
            UIWindowBase window = enumerator.Current.Value;
            int windowId = enumerator.Current.Key;

            removedKey.Add(windowId);

            window.HideWindowDirectly();
            sortedHiddenWindows.Add(window);
        }

        // 从显示窗口集合移除
        for (int i = 0; i < removedKey.Count; i++)
        {
            showWindows.Remove(removedKey[i]);
        }

        // 根据窗口depath排序
        sortedHiddenWindows.Sort(compareWindowFun);

        List<int> navHiddenWindows = new List<int>();
        for (int i = 0; i < sortedHiddenWindows.Count; i++)
        {
            int pushWindowId = sortedHiddenWindows[i].WindowID;
            navHiddenWindows.Add(pushWindowId);
        }

        BackWindowSequenceData backData = new BackWindowSequenceData();
        backData.hideTargetWindow = targetWindow;
        backData.backShowTargets = navHiddenWindows;
        backSequence.Push(backData);
        Debugger.Log("<color=cyan>### !!!Push new Navigation data!!! ###</color>");
    }

    private void DestoryOtherWindow(UIWindowBase targetWindow)
    {
        UIWindowData windowData = targetWindow.windowData;

        removedKey.Clear();

        var enumerator = allWindows.GetEnumerator();
        while (enumerator.MoveNext())
        {
            UIWindowBase window = enumerator.Current.Value;
            int windowId = enumerator.Current.Key;

            if (windowId == targetWindow.WindowID 
                || window.windowData.windowType == UIWindowType.Custom)
                continue;

            removedKey.Add(windowId);
        }

        for (int i = 0; i < removedKey.Count; i++)
        {
            showWindows.Remove(removedKey[i]);
        }

        ClearBackSequence();
        curShownWindow = null;
        lastShownWindow = null;

        if (removedKey.Count > 0)
        {
            StartCoroutine(DestoryOtherWindowCor());
        }

        Debugger.Log("<color=red>## [Destory Other Window, reset the backSequenceData for the navigation system.]##</color>");
    }

    private IEnumerator DestoryOtherWindowCor()
    {
        yield return new WaitForEndOfFrame();

        for(int i = 0; i < removedKey.Count; i++)
        {
            UIWindowBase window = allWindows[removedKey[i]];
            window.DestroyWindow();

            allWindows.Remove(removedKey[i]);
        }
    }

	private void CheckBackSequenceData(UIWindowBase baseWindow)
	{
		if (baseWindow.IsNavigationWindow)
		{
			if (backSequence.Count > 0)
			{
				BackWindowSequenceData backData = backSequence.Peek();
				if (backData.hideTargetWindow != null)
				{
					if (backData.hideTargetWindow.WindowID != baseWindow.WindowID)
					{
						Debugger.Log("<color=cyan>## UICenterMasterManager : clear sequence data ##</color>");
						Debugger.Log("## UICenterMasterManager : Hide target window and show window id is " 
							+ backData.hideTargetWindow.WindowID + " != " + baseWindow.WindowID);

						ClearBackSequence();
					}
				}
				else
					Debugger.LogError("Back data hide target window is null!");
			}
		}
	}

	/// <summary>
	/// 调整窗口层级
	/// </summary>
	/// <param name="windowBase"></param>
	private void AdjustBaseWindowDepth(UIWindowBase windowBase)
	{
		UIWindowType type = windowBase.windowData.windowType;
        if (type == UIWindowType.Custom)
            return;

		int needDepth = 1;
        UIWindowShowMode windowMode = windowBase.windowData.showMode;
		switch(type)
		{
		case UIWindowType.Normal:
			{
                if (windowMode == UIWindowShowMode.DestoryOtherWindow)
                {
                    needDepth = NORMAL_WINDOW_DEPTH;
                }
                else
                {
                    needDepth = Mathf.Clamp(MonoExtendUtil.GetMaxTargetDepth(uiNormalWindowRoot.gameObject) + 1, NORMAL_WINDOW_DEPTH, int.MaxValue);
                }
				
				//Debugger.Log("[UIWindowType.Normal] maxDepth is " + needDepth);
			}

			break;
		case UIWindowType.Fixed:
			{
                if (windowMode == UIWindowShowMode.DestoryOtherWindow)
                {
                    needDepth = FIXED_WINDOW_DEPTH;
                }
                else
                {
                    needDepth = Mathf.Clamp(MonoExtendUtil.GetMaxTargetDepth(uiFixedWidowRoot.gameObject) + 1, FIXED_WINDOW_DEPTH, int.MaxValue);    
                }
				
				//Debugger.Log("[UIWindowType.Fixed] max depth is " + needDepth);
			}
			break;
		case UIWindowType.PopUp:
			{
                if (windowMode == UIWindowShowMode.DestoryOtherWindow)
                {
                    needDepth = POPUP_WINDOW_DEPTH;
                }
                else
                {
                    needDepth = Mathf.Clamp(MonoExtendUtil.GetMaxTargetDepth(uiPopUpWindowRoot.gameObject) + 1, POPUP_WINDOW_DEPTH, int.MaxValue);
                }
				//Debugger.Log("[UIWindowType.PopUp] maxDepth is " + needDepth);
			}
			break;
		}

		if (windowBase.MinDepth != needDepth)
			MonoExtendUtil.SetTargetMinPanel(windowBase.gameObject, needDepth);

		windowBase.MinDepth = needDepth;
	}
	#endregion

	#region 关闭窗口
	public void CloseWindow(int windowId)
	{
		if (!IsRegisterWindow(windowId))
		{
			Debugger.LogError("## Current UI Manager has no control power of " + windowId.ToString());
			return;
		}

		UIWindowBase window = null;
		if (!showWindows.TryGetValue (windowId, out window)) {
			return;
		}

		if (this.backSequence.Count > 0)
		{
			BackWindowSequenceData seqData = this.backSequence.Peek();
			if (seqData != null && seqData.hideTargetWindow == window)
			{
				ReturnWindow();
				Debugger.Log("<color=magenta>## close window use PopNavigationWindow() ##</color>");
				return;
			}
		}

		HideWindow(windowId, delegate
        {
            ShowWindowData showData = new ShowWindowData();
            showData.executeNavLogic = false;

            int preWindowId = window.PreWindowID;
            if (preWindowId == UIWindowID.WINDOWID_INVAILD)
            {
                Debugger.LogWarning("## CurrentShownWindow " + window.WindowID + " preWindowId is " + UIWindowID.WINDOWID_INVAILD);
                return;
            }

            ShowWindow(preWindowId, null);
        });

		Debugger.Log("<color=magenta>## close window without PopNavigationWindow() ##</color>");
	}

	public void CloseAllWindow()
	{
		if (allWindows != null)
		{
			foreach (KeyValuePair<int, UIWindowBase> window in allWindows)
			{
				UIWindowBase baseWindow = window.Value;
                UIWindowData windowData = baseWindow.windowData;
                // 自定义窗口自己处理
                if (windowData.windowType == UIWindowType.Custom)
                    continue;

				baseWindow.DestroyWindow();
			}
		}

		if(uiNormalWindowRoot.childCount > 0)
		{
			uiNormalWindowRoot.DestroyChildren();
		}

		if(uiPopUpWindowRoot.childCount > 0)
		{
			uiPopUpWindowRoot.DestroyChildren();
		}

		if(uiFixedWidowRoot.childCount > 0)
		{
			uiFixedWidowRoot.DestroyChildren();
		}

		InitWindowManager();

		curShownWindow = null;
		lastShownWindow = null;
	}
	#endregion

	#region 隐藏窗口
	private void HideWindow(int id, Action onCompleted = null)
	{
		if(!IsRegisterWindow(id))
		{
			return;
		}

		if(!showWindows.ContainsKey(id))
		{
			return;
		}

		if (onCompleted != null)
		{
			onCompleted += delegate
			{
				showWindows.Remove(id);
			};

			showWindows[id].HideWindow(onCompleted);
			return;
		}

		showWindows[id].HideWindow(null);
		showWindows.Remove(id);
	}

	/// <summary>
	/// 隐藏所有显示的窗口
	/// </summary>
	/// <param name="includeFixed"></param>
	public void HideAllShownWindow(bool includeFixed)
	{
		if (showWindows.Count <= 0)
		{
			return;
		}

		List<int> removedKey = new List<int>();
		foreach (KeyValuePair<int, UIWindowBase> window in showWindows)
		{
			UIWindowData windowData = window.Value.windowData;
			if (!includeFixed && windowData.windowType == UIWindowType.Fixed)
			{
				continue;
			}

			removedKey.Add(window.Key);
			window.Value.HideWindowDirectly();
		}

		for (int i = 0; i < removedKey.Count; i++)
		{
			showWindows.Remove(removedKey[i]);
		}
	}
	#endregion

	#region 执行返回
	/// <summary>
	/// 导航返回
	/// </summary>
	public bool ReturnWindow()
	{
		if (curShownWindow != null)
		{
			bool needReturn = curShownWindow.ExecuteReturnLogic();
			if (needReturn)
				return false;
		}

		return RealReturnWindow();
	}

	private bool RealReturnWindow()
	{
		if (backSequence.Count == 0)
		{
			// 如果当前BackSequenceData 不存在返回数据
			// 检测当前Window的preWindowId是否指向上一级合法菜单
			if (curShownWindow == null)
				return false;

			int preWindowId = curShownWindow.PreWindowID;
			if (preWindowId == UIWindowID.WINDOWID_INVAILD)
			{
				Debugger.LogWarning("## CurrentShownWindow " + curShownWindow.WindowID + " preWindowId is " + UIWindowID.WINDOWID_INVAILD);
				return false;
			}

			Debugger.LogWarning(string.Format("## Current nav window {0} need show pre window {1}.", 
				curShownWindow.WindowID.ToString(), preWindowId.ToString()));

			ShowWindowForBack(preWindowId);
			HideWindow(curShownWindow.WindowID);
            curShownWindow = GetWindow(preWindowId);
			return true;
		}

		BackWindowSequenceData backData = backSequence.Peek();
		if (backData != null)
		{
			int hideId = backData.hideTargetWindow.WindowID;
			if (backData.hideTargetWindow != null && showWindows.ContainsKey(hideId))
			{
				if (backData.backShowTargets != null)
				{
					for (int i = 0; i < backData.backShowTargets.Count; i++)
					{
						int backId = backData.backShowTargets[i];
						ShowWindowForBack(backId);
						if (i == backData.backShowTargets.Count - 1)
						{
							Debugger.Log("change currentShownNormalWindow : " + backId);

							// 改变当前活跃Normal窗口
							this.lastShownWindow = this.curShownWindow;
							this.curShownWindow = GetWindow(backId);
						}
					}
				}

				HideWindow(hideId, delegate
				{
					// 隐藏当前界面
					backSequence.Pop();
				});

				return true;
			}
		}

		return true;
	}
	#endregion

	#region 注册 & 注销窗口管理
	/// <summary>
	/// 判断窗口是否有注册过
	/// </summary>
	/// <param name="windowID"></param>
	/// <returns></returns>
	public bool IsRegisterWindow(int windowID)
	{
		bool isRegister = this.managedWindowIds.Contains (windowID);

		return this.managedWindowIds.Contains(windowID);
	}

	/// <summary>
	/// 注册窗口ID
	/// </summary>
	/// <param name="windowID"></param>
	public void RegisterWindow(int windowID)
	{
		this.managedWindowIds.Add(windowID);
	}

	public void UnRegisterWindow(int windowID)
	{
		this.managedWindowIds.Remove (windowID);
	}
	#endregion

	public virtual UIWindowBase GetWindow(int id)
	{
		if (!IsRegisterWindow(id))
			return null;

		if (allWindows.ContainsKey(id))
			return allWindows[id];
		else
			return null;
	}

	public virtual T GetWindowScript<T>(int id) where T : UIWindowBase
	{
		UIWindowBase baseWindow = GetWindow(id);
		if(baseWindow == null)
		{
			return null;
		}

		return baseWindow as T;
	}

	private Transform GetTargetRoot(UIWindowType type)
	{
		if (type == UIWindowType.Normal)
		{
			return uiNormalWindowRoot;
		}

		if (type == UIWindowType.Fixed)
		{
			return uiFixedWidowRoot;
		}

		if (type == UIWindowType.PopUp)
		{
			return uiPopUpWindowRoot;
		}

        if (type == UIWindowType.Custom)
        {
            return uiCustomWindowRoot;
        }

		return null;
	}
}
