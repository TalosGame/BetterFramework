//
// MLPrefabPool.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.通用预制池
//      2.通过可用和已用到堆栈集合来管理
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

public class MLPrefabPool : MLPoolBase
{
	private Transform defaultParent;

	private Transform prefabTrans;
	private Stack<Transform> freeObjects = new Stack<Transform> ();
    private LinkedList<Transform> usedObjects = new LinkedList<Transform> ();

    public override void Init<T>(T poolItem, int preloadAmount = 10, Transform parent = null, bool isLimit = true)
    {
        this.prefabTrans = poolItem as Transform;
        this.itemName = this.prefabTrans.name;

        this.defaultParent = parent;

		this.preloadAmount = preloadAmount;
		this.limitAmount = this.preloadAmount << 1;
		this.limitInstances = isLimit;
    }

	public override void CreatePoolItems()
	{
		if (prefabTrans == null)
			return;

		for (int i = 0; i < preloadAmount; i++) 
		{
			Transform trans = SpawnNewInstance ();
			trans.gameObject.SetActive(false);
			freeObjects.Push (trans);
		}
	}

	private Transform SpawnNewInstance()
	{
		Transform trans = GameObject.Instantiate (prefabTrans, defaultParent);

		trans.name = trans.name.Replace("(Clone)", "");
		trans.position = Vector3.zero;
		trans.rotation = Quaternion.identity;
		return trans;
	}

    public override T Spawn<T>(Transform parent = null)
	{
		int freeObjCnt = freeObjects.Count;
		int useObjCnt = usedObjects.Count;

		if (freeObjCnt + useObjCnt >= limitAmount && limitInstances)
		{
			Debug.LogWarning("Can't Spawn new Instance. Cause not have enough free Object in pool!");
			return null;
		}

		Transform trans = null;
		if (freeObjCnt == 0)
		{
			trans = SpawnNewInstance();
            MarkItemUsed(trans, parent);
			return trans as T;
		}

		trans = freeObjects.Pop();
        MarkItemUsed(trans, parent);
		return trans as T;
	}

    private void MarkItemUsed(Transform item, Transform parent)
    {
        if(parent != null)
        {
            item.parent = parent;
        }

        item.gameObject.SetActive (true);
        usedObjects.AddFirst(item);
    }

	public override bool Despawn<T>(T item)
	{
        if(typeof(T) != typeof(Transform))
        {
            return false;
        }

        Transform transform = item as Transform;
        if(transform.name != this.itemName)
        {
            return false;
        }

        if (!usedObjects.Contains(transform))
		{
			return false;
		}

        usedObjects.Remove(transform);

		// recycle used object
		transform.parent = defaultParent;
		transform.gameObject.SetActive (false);
		freeObjects.Push (transform);
		return true;
	}

	public void DespawnAll()
	{

	}
}

