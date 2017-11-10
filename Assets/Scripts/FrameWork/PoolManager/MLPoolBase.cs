//
// Class:	MLPoolBase.cs
// Date:	2017/8/28 20:31
// Author: 	Miller
// Email:	wangquan <wangquancomi@gmail.com>
// QQ:		408310416
// Desc:
// 1.池基类接口定义
// 
// History:
// 2017/11/10
//    1. 对象池基类修改为范型模式,避免在对象池有装包和拆包操作.
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

public interface IMLPool
{
	void CreatePoolItems ();

	void DespawnAll();
}

public abstract class MLPoolBase<T> : IMLPool where T : class
{
	protected Stack<T> freeObjects = new Stack<T>();
	protected List<T> usedObjects = new List<T>();

	protected string itemName;

    protected int preloadAmount;
    protected int limitAmount;
    protected bool limitInstances;

	public int FreeObjectsCount
	{
		get
		{
			return freeObjects.Count;
		}
	}

	public int UsedObjectsCount
	{
		get
		{
			return usedObjects.Count;
		}
	}

	public string ItemName
	{
		get
		{
			return itemName;	
		}
	}

	public abstract void Init(T poolItem = default(T), int preloadAmount = 10, Transform parent = null,
		bool isLimit = true);

	public abstract void CreatePoolItems();

	public T Spawn(Transform parent = null)
    {
        int freeObjCnt = FreeObjectsCount;
        int useObjCnt = UsedObjectsCount;

		if (useObjCnt >= limitAmount && limitInstances)
		{
			Debug.LogWarning("Can't Spawn new Instance. Cause not have enough free Object in pool!");
			return default(T);
		}

		T item = default(T);
		if (freeObjCnt == 0)
		{
			item = SpawnNewItem() as T;
			MarkItemUsed(item, parent);
			return item;
		}

		item = GetFreeItem() as T;
		MarkItemUsed(item, parent);
		return item;
    }

	public abstract T SpawnNewItem();

	public T GetFreeItem()
	{
		return freeObjects.Pop ();
	}

	protected virtual void MarkItemUsed(T item, Transform parent = null)
	{
		usedObjects.Add (item);
	}

	public abstract bool Despawn (T item);

	public virtual void DespawnAll()
	{
		for (int i = usedObjects.Count - 1; i >= 0; i--)
		{
			var node = usedObjects[i];
			Despawn(node);
		}
	}
}