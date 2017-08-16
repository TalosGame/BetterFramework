using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 资源类型
public abstract class ResourceType
{
	public const int RES_ASSETBUNDLE = 0;               // ab资源
	public const int RES_UI = RES_ASSETBUNDLE + 1;      // UI
	public const int RES_DATAS = RES_UI + 1;            // 数据文件
	public const int RES_AUDIO = RES_DATAS + 1;         // 声音资源
	public const int RES_LUA = RES_AUDIO + 1;           // lua资源
}

/// <summary>
/// 资源定义类
/// </summary>
public class ResourceDefine
{
	// 窗口预设id和预设名字典
	private Dictionary<int, string> windowPrefabNames = new Dictionary<int, string>();

	// 资源
	private Dictionary<int, string> resourcePath = new Dictionary<int, string>();

	/// <summary>
	/// 初始资源定义
	/// </summary>
	public virtual void Init()
	{

	}

	protected void RegisterWindow(int windowId)
	{
		UIManager.Instance.RegisterWindow(windowId);
	}

	/// <summary>
	/// 添加扩展window
	/// </summary>
	/// <param name="windowID"></param>
	/// <param name="prefabName"></param>
	public void AddExUIWindow(int windowID, string prefabName)
	{
		string ret = string.Empty;
		if (windowPrefabNames.TryGetValue(windowID, out ret))
		{
			Debug.Log("Same window prefab key==" + windowID);
			return;
		}

		windowPrefabNames.Add(windowID, prefabName);
	}

	/// <summary>
	/// 获取UI预设路径
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public string GetUIPrefabName(int id)
	{
		string ret = string.Empty;
		if (!windowPrefabNames.TryGetValue(id, out ret))
		{
			Debug.LogWarning("Get UI Prefab Name error! window id==" + id);
			return null;
		}

		return ret;
	}

	/// <summary>
	/// 添加扩展资源路径
	/// </summary>
	/// <param name="type"></param>
	/// <param name="path"></param>
	public void AddExResourcePath(int type, string path)
	{
		resourcePath.Add(type, path);
	}

	/// <summary>
	/// 获取Resoucres目录下资源路径
	/// </summary>
	/// <param name="type"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	public string GetResourcePath(int type, string name)
	{
		string ret = string.Empty;
		if (!resourcePath.TryGetValue(type, out ret))
		{
			Debug.LogWarning("Get Resource Path error! type==" + type);
			return null;
		}

		string path = string.Format("{0}/{1}", ret, name);
		//Debug.Log("Resources path===" + path);
		return path;
	}
}
