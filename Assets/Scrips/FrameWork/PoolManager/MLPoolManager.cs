//
// MLPoolManager.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.同时管理预制池和对象池
//      2.统一从池生成和回收接口
//      3.后续想扩展新的对象池类只需要继承池基类即可
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

    public void CreatePool<TP, TI>(TI poolItem, int preloadAmount = 10, bool isLimit = true) 
        where TP : MLPoolBase
        where TI : class
    {
        MLPoolBase pool = CreatePoolClass<TP>() as MLPoolBase;
        if(pool == null)
        {
            return;
        }

        pool.Init(poolItem, preloadAmount, transform, isLimit);
        pool.CreatePoolItems();

        AddPool(pool);
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

	public void AddPool(MLPoolBase pool)
	{
		MLPoolBase cachePool = GetPool(pool.itemName);
		if (cachePool != null)
		{
			return;
		}

		pools.Add(pool.itemName, pool);
	}

    public MLPoolBase GetPool(string poolItem)
	{
		MLPoolBase cachePool = null;
		if (pools.TryGetValue(poolItem, out cachePool))
		{
			return cachePool;
		}

		Debug.LogWarning("Get prefab pool error! prefabName:" + poolItem);
		return null;
	}

    public T Spawn<T>(string poolItem) where T : class
	{
		return Spawn<T>(poolItem, null);
	}

    public T Spawn<T>(string poolItem, Transform parent) where T : class
	{
        MLPoolBase pool = GetPool(poolItem);
		if(pool == null)
		{
            return default(T);
		}

		return pool.Spawn<T>(parent);
	}

    public void Despawn<T>(T item) where T : class
    {
		bool sucess = false;
		var enumera = pools.GetEnumerator ();
		while (enumera.MoveNext ()) 
		{
            MLPoolBase pool = enumera.Current.Value;
			if(!pool.Despawn<T>(item))
				Debug.LogError ("Don't Despawn duplicate Object!");

			sucess = true;
			break;
		}

		// If still false, then the instance wasn't found anywhere in the pool
		if (!sucess)
		{
			Debug.LogError("Prefab pool not found in SpawnPool");
			return;
		}
    }

    public void DespawnAll()
    {

    }
}


