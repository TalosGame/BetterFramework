//
// Class:	MLPoolDictionary.cs
// Date:	2017/11/9 11:31
// Author: 	Miller
// Email:	wangquan <wangquancomi@gmail.com>
// QQ:		408310416
// Desc:
// 1.为了使对象池能实现范型通用设计目标, 这里定义一个池接口，
// 	 便于池管理器中维护各种对象池.
//
// Copyright (c) 2017 - 2018
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLPoolDictionary
{
	private Dictionary<string, IMLPool> pools = new Dictionary<string, IMLPool>();

	public IMLPool CreatePool(Type type)
	{
		if (type == null)
		{
			Debug.LogError("Get class reflect error! type name:" + type.Name);
			return null;
		}

		return Activator.CreateInstance(type) as IMLPool;
	}

	public void AddPool(string key, IMLPool pool)
	{
		if (pools.ContainsKey (key)) 
		{
			Debug.LogError ("Pool is already exist! key:" + key);
			return;
		}

		pools.Add (key, pool);
	}

	public IMLPool GetPool(string poolItem)
	{
		IMLPool cachePool = null;
		if(!pools.TryGetValue(poolItem, out cachePool))
		{
			Debug.LogWarning("Can't find item in any pool error! ItemName:" + poolItem);
			return null;
		}

		return cachePool;
	}

	public Dictionary<string, IMLPool>.Enumerator GetEnumerator()
	{
		return pools.GetEnumerator ();
	}
}


