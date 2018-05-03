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

/// <summary>
/// 继承该接口，有接受消息能力
/// </summary>
public interface INotification 
{
    void NotificationHandler(Notification notify);
}

public class NotificationCenter : SingletonBase<NotificationCenter>
{
    private Dictionary<string, List<INotification>> notifications = new Dictionary<string, List<INotification>>();
    private List<string> removeKeys = new List<string>();

    #region 添加观察监听事件
    public void AddObserver(string name, INotification observer)
    {
        if(string.IsNullOrEmpty(name))
        {
            Debug.LogError("Notification name can't be null!");
            return;
        }

        if (observer == null)
        {
            Debug.LogError("Notification handle can't be null!");
            return;
        }

        List<INotification> observers = null;
        if(!notifications.TryGetValue(name, out observers))
        {
            observers = new List<INotification>();
            notifications.Add(name, observers);
        }

        if(observers.Contains(observer))
        {
            return;
        }

        observers.Add(observer);
    }
	#endregion

	#region 移除观察者
    /// <summary>
    /// 移除所有该事件的所有监听者
    /// </summary>
    /// <param name="name"></param>
    public void RemoveObserver(string name) 
    {
        List<INotification> observers = null;
        if (!notifications.TryGetValue(name, out observers)) 
        {
            return;
        }

        observers.Clear();
    }

	/// <summary>
	/// 移除观察者中所有的监听事件
	/// </summary>
	/// <param name="observer">Observer.</param>
	public void RemoveObserver(INotification observer)
    {
        removeKeys.Clear();

        var enumerator = notifications.GetEnumerator();
        while(enumerator.MoveNext())
        {
            List<INotification> observers = enumerator.Current.Value;
            if(!observers.Remove(observer))
            {
                continue;
            }

            if(observers.Count <= 0)
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
	public void RemoveObserver(INotification observer, string name)
    {
        List<INotification> observers = null;
        if(!notifications.TryGetValue(name, out observers))
        {
            return;
        }

        if(!observers.Remove(observer))
        {
            return;
        }

        if(observers.Count <= 0)
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
        List<INotification> observers = null;
        if(!notifications.TryGetValue(name, out observers))
        {
            return;
        }

        Notification notify = new Notification(name, obj, userInfo);

        for (int i = 0; i < observers.Count; i++) 
        {
            INotification observer = observers[i];
            observer.NotificationHandler(notify);
        }
    }
    #endregion
}
