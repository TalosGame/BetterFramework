// Author：wangquan
// Mail：wangquancomi@gmail.com
// QQ ：408310416
// Date：2017/8/16/15:20
// Class：NotificationCenter
//
// Desc：
// 1.模拟Object-c事件通知管理器
// 2.方便添加和移除事件监听
// 3.能方便传递监听事件单个或多个参数

using UnityEngine;
using System.Collections.Generic;

public class NotificationCenter : SingletonBase<NotificationCenter>
{
    public delegate void NotificationHandler(Notification notify);  // 消息处理中间件

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
