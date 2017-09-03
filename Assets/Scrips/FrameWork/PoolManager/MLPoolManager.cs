//
// MLPoolManager.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.同时管理预制池和对象池
//      2.统一从池生成和回收接口
//      3.管理类为类使接口统一，在对象池管理这里有装
//        箱拆箱牺牲了一点性能
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
    private Dictionary<string, MLPoolBase> pools = new Dictionary<string, MLPoolBase>();

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

    public void CreatePool<TP, TI>(TI poolItem = null, int preloadAmount = 10, bool isLimit = true) 
        where TP : MLPoolBase
        where TI : class
    {
        MLPoolBase pool = CreatePoolClass<TP>() as MLPoolBase;
        if(pool == null)
        {
            return;
        }

        pool.Init<TI>(poolItem, preloadAmount, transform, isLimit);
        pool.CreatePoolItems<TI>();

        pools.Add(pool.itemName, pool);
    }

    private object CreatePoolClass<T>()
	{
        Type type = typeof(T);
		if (type == null)
		{
            Debug.Log("get class reflect error! type name:" + type.Name);
			return null;
		}

		return Activator.CreateInstance(type);
	}

    public MLPoolBase GetPool<T>(string poolItem) where T : class
	{
        MLPoolBase cachePool = null;
        if(!pools.TryGetValue(poolItem, out cachePool))
        {
			Debug.LogWarning("Can't find item in any pool error! ItemName:" + poolItem);
			return null;
        }

        return cachePool;
	}

    public T Spawn<T>(string poolItem) where T : class
	{
		return Spawn<T>(poolItem, null);
	}

    public T Spawn<T>(string poolItem, Transform parent) where T : class
	{
        MLPoolBase pool = GetPool<T>(poolItem);
		if(pool == null)
		{
            return default(T);
		}

		return pool.Spawn<T>(parent);
	}

    public void Despawn<T>(string poolItem, T item) where T : class
    {
		MLPoolBase pool = GetPool<T>(poolItem);
        if (pool == null)
        {
            return;
        }

        if (!pool.Despawn<T>(item))
        {
            Debug.LogError("Don't Despawn duplicate Object!");
        }
    }

    public void DespawnAll<T>(string poolItem) where T : class
    {
		MLPoolBase pool = GetPool<T>(poolItem);
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
            MLPoolBase pool = enumera.Current.Value;
            pool.DespawnAll();
        }
    }
}