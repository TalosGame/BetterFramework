// 作者：wangquan
// 邮箱：wangquancomi@gmail.com
// QQ ：408310416
// 时间：2017/8/16/15:24
// 类名：Notification
//
// 功能说明：
// 1.具体的消息类
// 2.可以获取消息传递的数据
// 3.包装了获取多个参数的方法

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notification : IEnumerable<KeyValuePair<string, object>>
{
    private string _name;      // 消息名称
	public string name
	{
		get{ return _name; }
	}

    private object _object;    // 单个数据对象
    public object Object
    {
        get { return _object; }
    }

	// 用户相关数据 已键值对的形式存放
    private Dictionary<string, object> _userInfo; 

	#region 索引取多个参数
	/// <summary>
	/// Gets or sets the <see cref="Notification"/> with the specified key.
	/// </summary>
	/// <param name="key">Key.</param>
	public object this[string key]
	{
		get
		{
			if (_userInfo == null) 
			{
				return null;
			}


			object ret = null;
			if (!_userInfo.TryGetValue (key, out ret)) 
			{
				return null;
			}

			return ret;
		}

		set
		{ 
			if (_userInfo == null) 
			{
				_userInfo = new Dictionary<string, object> ();
			}

			object ret = null;
			if (_userInfo.TryGetValue (key, out ret)) 
			{
				return;
			}

			_userInfo.Add (key, value);
		}
	}
	#endregion

	#region IEnumerable implementation
	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator ()
	{
		if (_userInfo == null) 
		{
			yield break;
		}

		foreach (KeyValuePair<string, object> kv in _userInfo) 
		{
			yield return kv;
		}
	}

	#endregion

	#region IEnumerable implementation

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return _userInfo.GetEnumerator ();
	}

	#endregion

    public Notification(string name, object obj, params object[] userInfo)
    {
        this._name = name;
        this._object = obj;

		if(userInfo == null)
		{
			return;
		}

		for (int i = 0; i < userInfo.Length; i++) 
		{
			object useInf = userInfo [i];
			if (useInf.GetType () != typeof(Dictionary<string, object>)) 
			{
				Debug.Log ("user info params must dictionary!");
				continue;
			}

			foreach (KeyValuePair<string, object> kv in useInf as Dictionary<string, object>) 
			{
				this [kv.Key] = kv.Value;
			}
		}	    
	}
}
