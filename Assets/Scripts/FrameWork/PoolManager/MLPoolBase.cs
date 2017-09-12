//
// MLPoolBase.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.池基类接口定义
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

public abstract class MLPoolBase
{
    public string itemName;

    protected int preloadAmount;
    protected int limitAmount;
    protected bool limitInstances;

    public abstract int FreeObjectsCount
    {
        get;
    }

    public abstract int UsedObjectsCount
    {
        get;
    }

    public abstract void Init<T>(T poolItem = null, int preloadAmount = 10, Transform parent = null,
                                 bool isLimit = true) where T : class;

    public abstract void CreatePoolItems<T>() where T : class;

    public T Spawn<T>(Transform parent = null) where T : class
    {
        int freeObjCnt = FreeObjectsCount;
        int useObjCnt = UsedObjectsCount;

		if (useObjCnt >= limitAmount && limitInstances)
		{
			Debug.LogWarning("Can't Spawn new Instance. Cause not have enough free Object in pool!");
			return null;
		}

        T item = null;
		if (freeObjCnt == 0)
		{
			item = SpawnNewItem<T>();
			MarkItemUsed(item, parent);
			return item as T;
		}

        item = GetFreeItem<T>();
		MarkItemUsed(item, parent);
		return item as T;
    }

    public abstract T SpawnNewItem<T>() where T : class;

    public abstract T GetFreeItem<T>() where T : class;

    public abstract void MarkItemUsed<T>(T item, Transform parent = null) where T : class;

	public abstract bool Despawn<T>(T item) where T : class;

    public abstract void DespawnAll();
}