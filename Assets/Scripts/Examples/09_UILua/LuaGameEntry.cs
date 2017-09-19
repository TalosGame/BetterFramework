using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaGameEntry : MonoBehaviour 
{
	void Awake()
	{
		MLResourceManager resMgr = MLResourceManager.Instance;
		resMgr.InitResourceDefine(new GameResDefine());
		resMgr.CreateResourceMgr(new ResourcesManager());
		resMgr.ChangeResourceMgr(ResManagerType.resourceMgr);
	}

	void Start () 
	{
		// 初始lua管理器
		LuaManager luaMgr = LuaManager.Instance;
		luaMgr.LoadMainLua();
		luaMgr.StartMain();
	}
}
