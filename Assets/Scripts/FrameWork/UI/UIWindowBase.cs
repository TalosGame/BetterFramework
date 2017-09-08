using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class UIWindowBase : MonoBehaviour
{
	protected Transform windowTrans;

	public UIWindowData windowData = UIWindowData.Create();

	// 如果需要可以添加一个BoxCollider屏蔽事件
	private bool isLock = false;
	protected bool isShown = false;

	/// <summary>
	/// 窗口层级
	/// </summary>
	private int minDepth = 1;

	/// <summary>
	/// 窗口ID
	/// </summary>
	private int windowID = UIWindowID.WINDOWID_INVAILD;

	/// <summary>
	/// 指向上一级界面ID(BackSequence无内容，返回上一级)
	/// </summary>
	private int preWindowID = UIWindowID.WINDOWID_INVAILD;

	// 窗口返回逻辑回调
	private event BoolDelegate returnWindowLogic = null;

	#region 窗口打开方式
	// 窗口打开方式
	public WindowShowStyle showStyle = WindowShowStyle.Normal;

	// 窗口打开或关闭Date
	public float duration = 0.2f;

	// 移动到某个点
	public Vector3 moveToPoint = Vector3.zero;

	// 窗口动画曲线
	public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0, 0, 0, 1f), new Keyframe(1f, 1f, 1f, 0f));
	#endregion

	private static Vector3 OUT_SCREEN_POSITION = new Vector3 (0f, 10000f, 0f);

	void Awake()
	{
		InitWindowOnAwake();
		InitWindowData();

		windowTrans = this.gameObject.transform;
		windowTrans.localPosition = OUT_SCREEN_POSITION;
	}

	// 子类需要强制设置窗口ID
	protected abstract void InitWindowData();

	/// <summary>
	/// 初始化窗口
	/// </summary>
	protected virtual void InitWindowOnAwake()
	{

	}

	public bool IsLock
	{
		get { return isLock; }
		set { isLock = value; }
	}

	public int MinDepth
	{
		get { return minDepth; }
		set { minDepth = value; }
	}

	public int WindowID
	{
		get
		{
			if (this.windowID == UIWindowID.WINDOWID_INVAILD)
				Debug.LogError("window id is " + UIWindowID.WINDOWID_INVAILD);

			return windowID;
		}

		protected set { windowID = value; }
	}

	public int PreWindowID
	{
		get { return preWindowID; }
		set { preWindowID = value; }
	}

	/// <summary>
	/// 重置窗口
	/// </summary>
	public virtual void ResetWindow()
	{

	}

	/// <summary>
	/// 是否是导航窗口
	/// 注：除开固定窗口类型，都是需要导航管理
	/// </summary>
	/// <value><c>true</c> if this instance is navigation window; otherwise, <c>false</c>.</value>
	public bool IsNavigationWindow
	{
		get 
        {
            UIWindowType windowType = this.windowData.windowType;
            return windowType != UIWindowType.Fixed && windowType != UIWindowType.Custom; 
        }
	}

	// On Add Collider bg to window
	// Add collider bg click event
	public virtual void OnAddMaskColliderEvent(GameObject obj)
	{

	}

	#region 窗口关闭
	public void DestroyWindow()
	{
        OnHideWindow();
		BeforeDestroyWindow();

        this.transform.parent = null;
		GameObject.Destroy(this.gameObject);
	}

	protected virtual void BeforeDestroyWindow()
	{

	}
	#endregion

	#region 窗口打开
	/// <summary>
	/// 直接打开窗口
	/// </summary>
	public void ShowWindowDirectly()
	{
		IsLock = false;
		isShown = true;

		OnShowWindow ();

		windowTrans.localPosition = Vector3.zero;
	}

	/// <summary>
	/// 打开窗口
	/// </summary>
	public void ShowWindow()
	{
		isShown = true;
		IsLock = true;

		StartShowWindow (true, () => {
			isLock = false;

			OnShowWindow();
		});
	}

	protected virtual void OnShowWindow()
	{

	}

	#endregion

	#region 关闭窗口
	/// <summary>
	/// 直接隐藏
	/// </summary>
	public void HideWindowDirectly()
	{
		IsLock = true;
		isShown = false;

		OnHideWindow ();

		windowTrans.localPosition = OUT_SCREEN_POSITION;
	}

	/// <summary>
	/// 隐藏窗口并有回调函数
	/// </summary>
	/// <param name="action"></param>
	public void HideWindow(Action action)
	{
		IsLock = false;
		isShown = false;

		StartShowWindow(false, ()=>
			{
				IsLock = true;
				if(action != null)
				{
					action();
				}

				OnHideWindow();
			});
	}

	protected virtual void OnHideWindow(){

	}
	#endregion

	#region 返回按钮逻辑注册
	/// <summary>
	/// 界面在退出或者用户点击返回之前都可以注册执行逻辑
	/// </summary>
	protected void RegisterReturnLogic(BoolDelegate returnLogic)
	{
		returnWindowLogic = returnLogic;
	}

	public bool ExecuteReturnLogic()
	{
		if (returnWindowLogic == null)
			return false;

		return returnWindowLogic();
	}
	#endregion

	#region 打开窗口的方式
	private void StartShowWindow(bool isOpen, Action callBack)
	{
		switch (showStyle)
		{
		case WindowShowStyle.Normal:
			ShowNormal(isOpen, callBack);
			break;
		case WindowShowStyle.CenterToBig:
			ShowCenterToBig(isOpen, callBack);
			break;
		case WindowShowStyle.FromTop:
			ShowFromDir(0, isOpen, callBack);
			break;
		case WindowShowStyle.FromDown:
			ShowFromDir(1, isOpen, callBack);
			break;
		case WindowShowStyle.FromLeft:
			ShowFromDir(2, isOpen, callBack);
			break;
		case WindowShowStyle.FromRight:
			ShowFromDir(3, isOpen, callBack);
			break;
		}
	}

	private void ShowNormal(bool isOpen, Action callBack)
	{
		if (isOpen) 
		{
			windowTrans.localPosition = Vector3.zero;
		} else 
		{
			windowTrans.localPosition = OUT_SCREEN_POSITION;
		}

		if (callBack != null)
			callBack();    
	}

	private void ShowCenterToBig(bool isOpen, Action callBack)
	{
		TweenScale tween = gameObject.GetOrAddComponent<TweenScale>();

		tween.animationCurve = animationCurve;
		tween.from = Vector3.zero;
		tween.to = Vector3.one;
		tween.duration = duration;

		tween.SetOnFinished(() =>
		{
			if (!isOpen)
			{
				windowTrans.localPosition = OUT_SCREEN_POSITION;
			}

			if (callBack != null)
				callBack();
		});

		windowTrans.localPosition = Vector3.zero;
		tween.Play(isOpen);
	}

	/// <summary>
	/// 从不同方向显示效果
	/// </summary>
	/// <param name="uiWin"></param>
	/// <param name="dirType">0: 从上 1：从下 2: 从左 3:从右</param>
	/// <param name="isOpen"></param>
	private void ShowFromDir(int dirType, bool isOpen, Action callBack)
	{
		TweenPosition tween = gameObject.GetOrAddComponent<TweenPosition>();
		tween.animationCurve = animationCurve;

		Vector3 from = Vector3.zero;
		switch (dirType)
		{
		case 0:
			from = new Vector3(0, 1000, 0);
			break;
		case 1:
			from = new Vector3(0, -1000, 0);
			break;
		case 2:
			from = new Vector3(-1400, 0, 0);
			break;
		case 3:
			from = new Vector3(1400, 0, 0);
			break;
		}

		tween.from = from;
		tween.to = moveToPoint;
		tween.duration = duration;

		tween.SetOnFinished(() =>
		{
			if (!isOpen)
			{
				windowTrans.localPosition = OUT_SCREEN_POSITION;
			}

			if (callBack != null)
				callBack();
		});

		windowTrans.localPosition = Vector3.zero;
		tween.Play(isOpen);
	}
	#endregion

	public void Update()
	{
		if (IsLock)
			return;

		OnUpdate();
	}

	public virtual void PlayMoveFocusSound()
	{
		//SoundManager.Instance.PlayMoveFocusSound();
	}

	public virtual void PlayButtonClickSound()
	{
		//SoundManager.Instance.PlayMoveFocusSound();
	}

	public virtual void OnUpdate()
	{

	}
}