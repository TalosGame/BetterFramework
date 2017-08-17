//
// ResManagerBase.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
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
using System;
using System.Collections;
using System.Collections.Generic;

public class ResourceInfo
{
    /// <summary>
    /// 资源对象
    /// </summary>
    public UnityEngine.Object _assetObj;

    /// <summary>
    /// 资源类型
    /// </summary>
    public int ResType { get; set; }

    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 引用个数
    /// </summary>
    public int RefCount { get; set; }

    /// <summary>
    /// 是否常驻内存
    /// </summary>
    public bool isPermanent { get; set; }

	public ResourceInfo()
	{

    }

	public ResourceInfo(string name, UnityEngine.Object assetObj)
	{
		Name = name;
		_assetObj = assetObj;
    }
}

public abstract class ResManagerBase
{
    protected Dictionary<string, ResourceInfo> resourceDic;

    protected ResourceDefine resourceDefine;

    public virtual void Init(ResourceDefine resourceDefine)
    {
        if (resourceDefine == null)
        {
            Debug.LogError("Need define resource object!");
            return;
        }

        this.resourceDefine = resourceDefine;

        resourceDic = new Dictionary<string, ResourceInfo>();
    }

    public string GetWindowPrefabPath(int windowID)
    {
        if (resourceDefine == null)
        {
            return null;
        }

        return resourceDefine.GetUIPrefabName(windowID);
    }

    #region 同步加载资源
    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源类型</param>
    /// <param name="isPermanent">是否常驻内存</param>
    /// <returns></returns>
    public UnityEngine.Object Load(string name, int type, bool isPermanent = false)
    {
        ResourceInfo resInfo = GetResourceInfo(name, type, isPermanent);
        if (resInfo._assetObj != null)
        {
            return resInfo._assetObj;
        }

        return Load(resInfo);
    }

    protected abstract UnityEngine.Object Load(ResourceInfo info);

    public void LoadBundle(string name, int type, bool isPermanent = false)
    {
        ResourceInfo resInfo = GetResourceInfo(name, type, isPermanent);
        if (resInfo._assetObj != null)
        {
            return;
        }

        LoadBundle(resInfo);
    }

    protected virtual void LoadBundle(ResourceInfo info) { }
    #endregion

    #region 异步加载资源
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="type">资源类型</param>
    /// <returns></returns>
    public void LoadAsync(string name, Action<UnityEngine.Object> load, Action<float> progress, int type, bool isPermanent = false)
    {
        ResourceInfo resInfo = GetResourceInfo(name, type, isPermanent);
        if (resInfo._assetObj != null)
        {
            load(resInfo._assetObj);
            return;
        }

        CoroutineManger.Instance.StartCoroutine(LoadAsync(resInfo, load, progress));
    }

    public abstract IEnumerator LoadAsync(ResourceInfo info, Action<UnityEngine.Object> load, Action<float> progress);
    #endregion

    /// <summary>
    /// 获取资源信息
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源类型</param>
    /// <returns></returns>
    public ResourceInfo GetResourceInfo(string name, int type, bool isPermanent)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Asset path is null or Empty!!!");
            return null;
        }

        ResourceInfo resourceInfo = null;
        if (!resourceDic.TryGetValue(name, out resourceInfo))
        {
            resourceInfo = new ResourceInfo();
            resourceInfo.Name = name;
            resourceInfo.ResType = type;
            resourceInfo.isPermanent = isPermanent;
            resourceDic.Add(name, resourceInfo);
        }

        resourceInfo.RefCount++;
        return resourceInfo;
    }

    public ResourceInfo GetLoadedResource(string name)
    {
        ResourceInfo resourceInfo = null;
        if (!resourceDic.TryGetValue(name, out resourceInfo))
        {
            return null;
        }

        return resourceInfo;
    }

    public void UnloadResource(string name, bool unloadObject = false)
    {
        ResourceInfo resInfo = GetLoadedResource(name);
        if (resInfo == null)
            return;

        if (!resInfo.isPermanent)
        {
            UnloadResource(resInfo, unloadObject:unloadObject);
        }

        // 如果不是释放已加载的资源，资源缓存不需要清空
        if (!unloadObject)
        {
            return;
        }

        //Debugger.Log(resourceDic.Count + " resource(s) in memory before unloading " + name);
        if (--resInfo.RefCount == 0)
        {
            resourceDic.Remove(name);
            Debug.Log("Resource " + name + " has been unloaded successfully");
        }
        //Debugger.Log(resourceDic.Count + " resource(s) in memory after unloading " + name);
    }

    public void UnloadResourceAndDepend(string name, bool unloadObject = false)
    {
        ResourceInfo resInfo = GetLoadedResource(name);
        if (resInfo == null || resInfo.isPermanent)
            return;

        UnloadAllResource(resInfo, unloadObject);

        // 如果不是释放已加载的资源，资源缓存不需要清空
        if (!unloadObject)
        {
            return;
        }

        Debug.Log(resourceDic.Count + " resource(s) in memory before unloading " + name);
        if (--resInfo.RefCount == 0)
        {
            resourceDic.Remove(name);
            Debug.Log("Resource " + name + " has been unloaded successfully");
        }

        Debug.Log(resourceDic.Count + " resource(s) in memory after unloading " + name);
    }

    public void UnloadAllResource(bool unloadPermanent = false, bool unloadObject = false)
    {
        List<string> keys = new List<string>(resourceDic.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            ResourceInfo resInfo = GetLoadedResource(key);
            if (resInfo == null)
                continue;

            if (!unloadPermanent && resInfo.isPermanent)
                continue;

            UnloadResource(resInfo, unloadObject);

            resourceDic.Remove(key);
        }

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public virtual void UnloadResource(ResourceInfo info, bool unloadObject = false)
    {
        
    }

    public virtual void UnloadAllResource(ResourceInfo info, bool unloadObject = false)
    { 
    
    }
}
