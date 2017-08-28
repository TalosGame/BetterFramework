//
// MLResourceManager.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.同时管理Resources和AB资源
//      2.统一接口调用，方便前期快速开发与后期AB资源打包
//      3.能同时使用2种不同资源
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

public enum ResManagerType
{
    assetBundleMgr = 0,
    resourceMgr,
}

public class MLResourceManager : DDOLSingleton<MLResourceManager>
{
    private Dictionary<ResManagerType, ResManagerBase> resManagers = new Dictionary<ResManagerType, ResManagerBase>();

    // 当前资源管理类型
    private ResManagerType resMgrType;

    // 资源管理
    private ResManagerBase resManager;
    public ResManagerBase ResManager
    {
        get { return resManager; }
    }

    // 资源定义
    protected ResourceDefine resourceDefine;
    public ResourceDefine ResourceDefine
    {
        get { return resourceDefine; }
    }

    // 根据资源管理类型初始化
    //public void CreateResourceMgr(ResourceDefine resDefine)
    //  {
    //resourceDefine = resDefine;
    //    resourceDefine.Init();

    //    foreach (ResManagerType type in Enum.GetValues(typeof(ResManagerType)))
    //    {
    //        ResManagerBase resManager = null;
    //        switch (type)
    //        {
    //            case ResManagerType.resourceMgr:
    //                resManager = new ResourcesManager();
    //                break;
    //            case ResManagerType.assetBundleMgr:
    //                resManager = new AssetBundleManager();
    //                break;
    //        }

    //        resManager.Init(resourceDefine);
    //        resManagers.Add(type, resManager);
    //    }
    //}

    public void InitResourceDefine(ResourceDefine resDefine)
    {
		resourceDefine = resDefine;
        resourceDefine.InitResPaths();
    }

    public void CreateResourceMgr(ResManagerBase resManager)
    {
        if(resourceDefine == null)
        {
            Debug.LogError("Resource define must init first!");
            return;
        }

        ResManagerType type = resManager.ManagerType();
        if(resManagers.ContainsKey(type))
        {
            Debug.LogError("Resource manager already exist! type:" + type.ToString());
            return;
        }

        this.resMgrType = type;

		resManager.Init(resourceDefine);
		resManagers.Add(type, resManager);
    }

    public void ChangeResourceMgr(ResManagerType type)
    {
        this.resMgrType = type;
        resManager = resManagers[type];
    }

    public string GetWindowPrefabPath(int windowId)
    {
        return resManager.GetWindowPrefabPath(windowId);
    }

    public void AddExUIWindow(int windowID, string prefabName)
    {
        resourceDefine.AddExUIWindow(windowID, prefabName);
    }

    public void AddExResourcePath(int type, string path)
    {
        resourceDefine.AddExResourcePath(type, path);
    }

    /// <summary>
    /// 获取加载的资源
    /// </summary>
    /// <returns>The loaded resource.</returns>
    /// <param name="name">Name.</param>
    public ResourceInfo GetLoadedResource(string name)
    {
        return resManager.GetLoadedResource(name);
    }

    /// <summary>
    /// 获取资源名称
    /// 如果是AssetBundle资源， 资源名称需要都是小写
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string GetResName(string name)
    {
        if(resMgrType == ResManagerType.assetBundleMgr)
        {
            int stIdx = name.LastIndexOf("/");
            stIdx = stIdx < 0 ? 0 : stIdx + 1;
            return name.Substring(stIdx, name.Length - stIdx).ToLower();
        }

        return name;
    }

    #region 同步克隆&实例
    /// <summary>
    /// 同步加载并克隆生成对象
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源类型</param>
    /// <param name="unloadAllLoadObject">是否卸载已加载好的资源</param>
    /// <returns></returns>
    public UnityEngine.Object LoadInstance(string name, int type = ResourceType.RES_UI, 
        bool unloadObject = false, bool isPermanent = false)
    {
        return LoadInstance(name, null, type, unloadObject, isPermanent);
    }

    /// <summary>
    /// 同步加载并克隆生成对象
    /// 扩展：
    /// 1.获取该对象或者是子节点对象上的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public T LoadInstance<T>(string name, int type = ResourceType.RES_UI, 
        bool unloadObject = false, bool isPermanent = false) where T : Component
    {
        GameObject obj = LoadInstance(GetResName(name), type, unloadObject, isPermanent) as GameObject;
        if(obj == null)
        {
            return null;
        }

        T ret = obj.GetComponent<T>();
        if (ret == null)
        {
            ret = obj.GetComponentInChildren<T>();
        }

        return ret;
    }

    /// <summary>
    /// 同步加载并克隆生成对象
    /// 扩展：
    /// 1. 该对象挂在父节点下
    /// 2. 获取该对象或者是子节点对象上的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public T LoadInstance<T>(string name, GameObject parent, int type = ResourceType.RES_UI, 
        bool unloadObject = false, bool isPermanent = false) where T : Component
    {
        GameObject obj = LoadInstance(name, parent, type, unloadObject, isPermanent) as GameObject;
        if (obj == null)
        {
            return null;
        }

        T ret = obj.GetComponent<T>();
        if(ret == null)
        {
            ret = obj.GetComponentInChildren<T>();
        }

        return ret;
    }

    /// <summary>
    /// 同步加载核心接口方法
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadInstance(string name, GameObject parent, int type = ResourceType.RES_UI, 
        bool unloadObject = false, bool isPermanent = false)
    {
        string resName = GetResName(name);

        UnityEngine.Object prefabObj = LoadResource(resName, type, isPermanent);

        GameObject ret = MonoExtendUtil.CreateChild(parent, prefabObj as GameObject);

        StartCoroutine(UnloadResourceCoroutine(resName, unloadObject));
        return ret;
    }
    #endregion

    #region 异步克隆&实例
    /// <summary>
    /// 异步加载并克隆生成对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="load"></param>
    /// <param name="type"></param>
    public void LoadAsyncInstance(string name, Action<UnityEngine.Object> load, int type = ResourceType.RES_UI,
        bool unloadObject = false, bool isPermanent = false)
    {
        LoadAsyncInstance(name, null, load, null, type, unloadObject, isPermanent);
    }

    public void LoadAsyncInstance(string name, Action<UnityEngine.Object> load, Action<float> progress, int type = ResourceType.RES_UI,
        bool unloadObject = false, bool isPermanent = false)
    {
        LoadAsyncInstance(name, null, load, progress, type, unloadObject, isPermanent);
    }

    public void LoadAsyncInstance<T>(string name, GameObject parent, Action<T> load, Action<float> progress,
        int type = ResourceType.RES_UI, bool unloadObject = false, bool isPermanent = false) where T : Component
    {
        LoadAsyncInstance(GetResName(name), parent, (_obj) =>
        {
            if(_obj == null || load == null)
            {
                load(null);
                return;
            }

            GameObject obj = _obj as GameObject;
            T ret = obj.GetComponent<T>();
            if (ret == null)
            {
                ret = obj.GetComponentInChildren<T>();
            }

            load(ret);

        }, progress, type, unloadObject, isPermanent);
    }

    public void LoadAsyncInstance(string name, GameObject parent, Action<UnityEngine.Object> load, Action<float> progress,
        int type = ResourceType.RES_UI, bool unloadObject = false, bool isPermanent = false)
    {
        string resName = GetResName(name);

        LoadResourceAsync(resName, (_obj) =>
        {
            if (_obj == null)
            {
                Debug.LogError("resource load prefab object is null!");
                return;
            }

            GameObject ret = MonoExtendUtil.CreateChild(parent, _obj as GameObject);
            if (ret == null)
            {
                Debug.LogError("Instantiate object is null!");
                load(null);
                return;
            }

            UnloadResource(resName, unloadObject);

            if (load != null)
                load(ret);
        }
        , progress, type, isPermanent);
    }
    #endregion

    #region 克隆功能函数
    /// <summary>
    ///  1.AssetBunldeManager使用LoadResource同步或异步的方法
    ///    预设克隆后，需要手动调用卸载资源方法，避免镜像内存
    ///    没有释放。
    /// 
    ///  2.ResourcesManager使用LoadResource同步或异步的方法，
    ///    如果有清资源缓存的需求，需要手动调用卸载资源方法
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="isPermanent"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadResource(string name, int type = ResourceType.RES_UI, bool isPermanent = false)
    {
        return resManager.Load(GetResName(name), type, isPermanent);
    }

    public void LoadResourceAsync(string name, Action<UnityEngine.Object> load, int type = ResourceType.RES_UI, 
        bool isPermanent = false)
    {
        resManager.LoadAsync(GetResName(name), load, null, type, isPermanent);
    }

    public void LoadResourceAsync(string name, Action<UnityEngine.Object> load, Action<float> progress,
        int type = ResourceType.RES_UI, bool isPermanent = false)
    {
        resManager.LoadAsync(GetResName(name), load, progress, type, isPermanent);
    }
    #endregion

    public void LoadBundle(string name, bool isPermanent = false)
    {
        resManager.LoadBundle(name, ResourceType.RES_ASSETBUNDLE, isPermanent);
    }

    #region 释放功能函数
    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="name">资源名称</param>
    public void UnloadResource(string name, bool unloadObject = false)
    {
        resManager.UnloadResource(GetResName(name), unloadObject);
    }

    /// <summary>
    /// 卸载依赖和自己的资源
    /// </summary>
    /// <param name="name"></param>
    /// <param name="unloadObject"></param>
    public void UnloadResourceAndDepend(string name, bool unloadObject = false)
    {
        resManager.UnloadResourceAndDepend(GetResName(name), unloadObject);
    }

    /// <summary>
    /// 携程释放资源
    /// 注: 等一帧目的是为了防止bundle unload(false)的情况，
    /// 如果克隆UI预制的时候在NGUI渲染帧中进行，会出现错误。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="unloadObject"></param>
    /// <returns></returns>
    private IEnumerator UnloadResourceCoroutine(string name, bool unloadObject)
    {
        yield return null;
        UnloadResource(GetResName(name), unloadObject);
    }

    /// <summary>
    /// 卸载所有资源
    /// </summary>
    /// <param name="unloadPermanent">是否卸载常驻资源</param>
    public void UnloadAllResource(bool unloadPermanent = false, bool unloadObject = false)
    {
        resManager.UnloadAllResource(unloadPermanent, unloadObject);
    }
    #endregion
}
