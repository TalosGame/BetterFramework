using UnityEngine;
using System.Collections;
using System.IO;
using LuaInterface;

public class LuaManager : DDOLSingleton<LuaManager>
{
	public LuaState LuaState
	{
		get;
		private set;
	}

    private LuaLooper looper;

    private LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }

    protected override void Init()
    {
        InitLoader();
        LuaState = new LuaState();
        OpenLibs();
        LuaState.LuaSetTop(0);
        LuaBinder.Bind(LuaState);
        //LuaState.Start();
        //StartLooper();
    }

    private void OpenLibs()
    {
        LuaState.OpenLibs(LuaDLL.luaopen_pb);
        LuaState.OpenLibs(LuaDLL.luaopen_struct);
        LuaState.OpenLibs(LuaDLL.luaopen_lpeg);

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        LuaState.OpenLibs(LuaDLL.luaopen_bit);
#endif
        OpenCJson();
    }

    //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
    private void OpenCJson()
    {
        LuaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        LuaState.OpenLibs(LuaDLL.luaopen_cjson);
        LuaState.LuaSetField(-2, "cjson");

        LuaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
        LuaState.LuaSetField(-2, "cjson.safe");
    }

    private void StartLooper()
    {
        looper = MonoExtendUtil.GetOrAddComponent<LuaLooper>(gameObject);
        looper.luaState = LuaState;
    }

    public void InitGameConfig()
    {
        LuaState.DoFile("GameDefine.lua");
        LuaFunction initGameConfig = LuaState.GetFunction("InitGameConfig");
        initGameConfig.Call();
        initGameConfig.Dispose();
        initGameConfig = null;
    }

    public void SetLobbyServerIpAdress(string ip, int port)
    {
        LuaState.DoFile("GameDefine.lua");
        LuaFunction initGameConfig = LuaState.GetFunction("SetLobbyIpAddress");
        initGameConfig.Call<string, int>(ip, port);
        initGameConfig.Dispose();
        initGameConfig = null;
    }

    public void LoadMainLua()
    {
        LuaState.DoFile("Main.lua");
    }

    public void StartMain()
    {
        LuaState.Start();

        LuaFunction main = LuaState.GetFunction("Main");
        main.Call();
        main.Dispose();
        main = null;

        StartLooper();
    }

	//自己实现一个lua require函数，可以得到require的返回值。
	public object Require(string fileName)
	{
		int top = LuaState.LuaGetTop();
		string error = null;
		object result = null;

		if (LuaState.LuaRequire(fileName) != 0)
		{
			error = LuaState.LuaToString(-1);
		}
		else
		{
			if(LuaState.LuaGetTop() > top)
			{
				result = LuaState.ToVariant(-1);
			}
		}

		LuaState.LuaSetTop(top);

		if (error != null)
		{
			throw new LuaException(error);
		}
		return result;
	}

	public LuaTable LoadLuaScript(string scriptName, UILuaWindow behavior)
	{
		if (string.IsNullOrEmpty (scriptName)) 
		{
			return null;
		}

		LuaTable metaTable = Require (scriptName) as LuaTable;
		if(metaTable == null)
		{
			Debugger.LogError("Invalid script file '" + scriptName + "', metatable needed as a result.");
			return null;
		}

		//从类中找到New函数
		LuaFunction lnew = (LuaFunction)metaTable["New"];
		if(lnew == null)
		{
			Debugger.LogError("Invalid metatable of script '" + scriptName + "', function 'New' needed.");
			return null;
		}

		LuaTable result = lnew.Invoke<LuaTable, UILuaWindow, LuaTable>(metaTable, behavior);
		if (result == null) 
		{
			Debugger.LogError("Invalid 'New' method of script '" + scriptName + "', a return value needed.");
			return null;
		}

		return result;
	}

    public LuaFunction GetLuaFunction(string name)
    {
        LuaFunction luaFunc = LuaState.GetFunction(name);
        if (luaFunc == null)
        {
            Debugger.LogError("Call lua function error! name==" + name);
            return null;
        }

        return luaFunc;
    }

    protected override void Destroy()
    {
        if (LuaState != null)
        {
            if (looper != null)
            {
                looper.Destroy();
                looper = null;
            }

            LuaState.Dispose();
            LuaState = null;
        }
    }
}
