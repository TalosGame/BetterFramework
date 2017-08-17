//
// NotificationCenter.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.模拟Object-c事件通知管理器
//      2.方便添加和移除事件监听
//      3.能方便传递监听事件单个或多个参数
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

public class NotificationCenter : SingletonBase<NotificationCenter>
{
	// 消息处理中间件
	public delegate void NotificationHandler(Notification notify);

    private Dictionary<string, Dictionary<object, NotificationHandler>> notifications = new Dictionary<string, Dictionary<object, NotificationHandler>>();
    private List<string> removeKeys = new List<string>();

    #region 添加观察监听事件
    public void AddObserver(object observer, string name, NotificationHandler handler)
    {
        if(string.IsNullOrEmpty(name))
        {
            Debug.LogError("Notification name can't be null!");
            return;
        }

        if(handler == null)
        {
            Debug.LogError("Notification handle can't be null!");
            return;
        }

        Dictionary<object, NotificationHandler> handlers = null;
        if(!notifications.TryGetValue(name, out handlers))
        {
            handlers = new Dictionary<object, NotificationHandler>();
            notifications.Add(name, handlers);
        }

        if(handlers.ContainsKey(observer))
        {
            return;
        }

        handlers.Add(observer, handler);
    }
	#endregion

	#region 移除观察者
	/// <summary>
	/// 移除观察者中所有的监听事件
	/// </summary>
	/// <param name="observer">Observer.</param>
	public void RemoveObserver(object observer)
    {
        removeKeys.Clear();

        var enumerator = notifications.GetEnumerator();
        while(enumerator.MoveNext())
        {
            Dictionary<object, NotificationHandler> handlers = enumerator.Current.Value;
            if(!handlers.ContainsKey(observer))
            {
                continue;
            }

            handlers.Remove(observer);

            if(handlers.Count <= 0)
            {
                removeKeys.Add(enumerator.Current.Key);
            }
        }

        foreach(string name in removeKeys)
        {
            notifications.Remove(name);
        }
    }

	/// <summary>
	/// 移除观察者中单个监听方法
	/// </summary>
	/// <param name="observer">Observer.</param>
	/// <param name="name">Name.</param>
	public void RemoveObserver(object observer, string name)
    {
        Dictionary<object, NotificationHandler> handlers = null;
        if(!notifications.TryGetValue(name, out handlers))
        {
            return;
        }

        if(!handlers.ContainsKey(observer))
        {
            return;
        }

        handlers.Remove(observer);
        if(handlers.Count <= 0)
        {
            notifications.Remove(name);
        }
    }
    #endregion

    #region 通知观察者消息
    public void PostNotification(string name)
    {
        this.PostNotification(name, null, null);
    }
    public void PostNotification(string name, object obj)
    {
        this.PostNotification(name, obj, null);
    }

    public void PostNotification(string name, object obj, params object []userInfo)
    {
        Dictionary<object, NotificationHandler> handlers = null;
        if(!notifications.TryGetValue(name, out handlers))
        {
            return;
        }

        Notification notify = new Notification(name, obj, userInfo);

        var enumerator = handlers.GetEnumerator();
        while(enumerator.MoveNext())
        {
            NotificationHandler handle = enumerator.Current.Value;
            handle(notify);
        }
    }
    #endregion
}
