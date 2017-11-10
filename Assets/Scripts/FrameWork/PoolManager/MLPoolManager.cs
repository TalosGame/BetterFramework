//
// Class:	MLPoolManager.cs
// Date:	2017/8/28 20:31
// Author: 	Miller
// Email:	wangquan <wangquancomi@gmail.com>
// QQ:		408310416
// Desc:
// 1.统一通过池管理器来管理池
// 2.可以同时管理预制池和对象池
//
// History:
// 2017/11/10 
//   1. 通过自定义池字典维护池
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
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class MLPoolManager : MonoSingleton<MLPoolManager>
{
	private MLPoolDictionary pools = new MLPoolDictionary();

    [SerializeField]
    private bool dontDestroyOnLoad = false;
    public bool DontDestroyOnLoad
    {
        get
        {
            return this.dontDestroyOnLoad;
        }

        set
        {
            this.dontDestroyOnLoad = value;

            if(this.dontDestroyOnLoad)
            {
                this.gameObject.AddToDDOLRoot();
                return;
            }

            SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
        }
    }

    public void CreatePool<TP, T>(T poolItem = null, int preloadAmount = 10, bool isLimit = true) 
		where TP : MLPoolBase<T>
        where T  : class
    {
		Type type = typeof(TP);
		MLPoolBase<T> pool = pools.CreatePool (type) as MLPoolBase<T>;
		if (pool == null) 
		{
			return;
		}

        pool.Init(poolItem, preloadAmount, transform, isLimit);
        pool.CreatePoolItems();

		pools.AddPool (pool.ItemName, pool);
    }

	public MLPoolBase<T> GetPool<T>(string poolItem) where T : class
	{
		IMLPool pool = pools.GetPool(poolItem);
		if (pool == null)
		{
			return null;
		}

		return pool as MLPoolBase<T>;
	}

	public T Spawn<T>(string poolItem) where T : class
	{
		return Spawn<T>(poolItem, null);
	}

	public T Spawn<T>(string poolItem, Transform parent) where T : class
	{
		MLPoolBase<T> pool = pools.GetPool(poolItem) as MLPoolBase<T>;
		if(pool == null)
		{
			return null;
		}

		return pool.Spawn(parent);
	}

	public void Despawn<T>(string poolItem, T item) where T : class
    {
		MLPoolBase<T> pool = pools.GetPool(poolItem) as MLPoolBase<T>;
        if (pool == null)
        {
            return;
        }

        if (!pool.Despawn(item))
        {
            Debug.LogError("Don't Despawn duplicate Object!");
        }
    }

	public void DespawnAll(string poolItem)
    {
		IMLPool pool = pools.GetPool(poolItem);
		if (pool == null)
		{
			return;
		}

        pool.DespawnAll();
    }

	public void DespawnAll()
    {
        var enumera = pools.GetEnumerator();
        while (enumera.MoveNext())
        {
			IMLPool pool = enumera.Current.Value;
            pool.DespawnAll();
        }
    }
}