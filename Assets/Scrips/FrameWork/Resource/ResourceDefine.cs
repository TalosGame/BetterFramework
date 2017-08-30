//
// ResourceDefine.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.资源抽象基类
//      2.统一同步与异步资源加载，卸载接口
//
// Copyright (c) 2017 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
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

public abstract class ResourceDefine
{
	// 窗口预设id和预设名字典
	private Dictionary<int, string> windowPrefabNames = new Dictionary<int, string>();

	// 资源
	private Dictionary<int, string> resourcePath = new Dictionary<int, string>();

    /// <summary>
    /// 初始化窗口资源
    /// </summary>
    public abstract void InitUIWindows();

    /// <summary>
    /// 初始化资源路径
    /// </summary>
    public abstract void InitResPaths();

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
        // 如果ab资源
        if(type == ResourceType.RES_ASSETBUNDLE)
        {
            return name;
        }

		string ret = string.Empty;
		if (!resourcePath.TryGetValue(type, out ret))
		{
			Debug.LogWarning("Get Resource Path error! type==" + type);
			return null;
		}

		return string.Format("{0}/{1}", ret, name);
	}
}
