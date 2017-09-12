using System.Collections;
using System;
using UnityEngine;
using LuaInterface;

public class UILuaWindow : UIWindowBase 
{
	private const string INIT_WINDOW_ON_AWAKE = "InitWindowOnAwake";
	private const string INIT_WINDOW_DATA = "InitWindowData";
	private const string RESET_WINDOW = "ResetWindow";
	private const string ON_SHOW_WINDOW = "OnShowWindow";
	private const string ON_HIDE_WINDOW = "OnHideWindow";
	private const string BEFORE_DESTROY_WINDOW = "BeforeDestroyWindow";

	private const string LUA_VIEW_PATH = "UI/View/";

	public string luaFileName;

	private LuaState luaState;
	private LuaTable self;

	protected override void InitWindowOnAwake ()
	{
		string luaFilePath = string.Format ("{0}{1}", LUA_VIEW_PATH, luaFileName);
		if(string.IsNullOrEmpty(luaFileName))
		{
			Debugger.LogError("The scriptName must be set.");
			return;
		}

		luaState = LuaManager.Instance.LuaState;
#if UNITY_EDITOR
        if (!luaState.BeStart)
        {
            return;
        }
#endif

		self = LuaManager.Instance.LoadLuaScript (luaFilePath, this);
		if (self == null)
		{
			return;
		}

		self ["transform"] = transform;
		self ["gameObject"] = gameObject;

		CallMethod(INIT_WINDOW_ON_AWAKE);
	}

	protected override void InitWindowData ()
	{
#if UNITY_EDITOR
        if (!luaState.BeStart)
        {
            return;
        }
#endif

		CallMethod(INIT_WINDOW_DATA);

        object windowId = self["windowId"];
        if (windowId != null)
        {
            this.WindowID = Convert.ToInt32(windowId);    
        }

        object forceClearNavigation = self["forceClearNavigation"];
        if (forceClearNavigation != null)
        {
            this.windowData.forceClearNavigation = Convert.ToBoolean(forceClearNavigation);    
        }

        object windowType = self["windowType"];
        if (windowType != null)
        {
            this.windowData.windowType = (UIWindowType)Enum.Parse(typeof(UIWindowType), windowType.ToString());    
        }

        object showMode = self["showMode"];
        if (showMode != null)
        {
            this.windowData.showMode = (UIWindowShowMode)Enum.Parse(typeof(UIWindowShowMode), showMode.ToString());    
        }

		//Debugger.Log ("windowId===" + this.WindowID);
	}

    protected override void OnShowWindow(ShowWindowData? data = null)
    {
        CallMethod(ON_SHOW_WINDOW);
    }

	protected override void OnHideWindow ()
	{
		CallMethod (ON_HIDE_WINDOW);
	}

	protected override void BeforeDestroyWindow ()
	{
		CallMethod(BEFORE_DESTROY_WINDOW);

		//销毁脚本对象
		if(self != null)
		{
			self.Dispose();
			self = null;
		}
	}

	protected object[] CallMethod(string func, params object[] args)
	{
		if (self == null)
		{
			return null;
		}

		LuaFunction lfunc = (LuaFunction)self[func];
		if(lfunc == null)
		{
			return null;
		}

		//等价于lua语句: self:func(...)
		int oldTop = lfunc.BeginPCall();
		lfunc.Push(self);
		lfunc.PushArgs(args);
		lfunc.PCall();
		object[] objs = luaState.CheckObjects(oldTop);
		lfunc.EndPCall();
		return objs;
	}
}
