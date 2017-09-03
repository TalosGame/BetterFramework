using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MLObjectPool : MLPoolBase
{
    protected Stack<object> freeObjects = new Stack<object>();
    protected List<object> usedObjects = new List<object>();

    public override int FreeObjectsCount
    {
        get
        {
            return freeObjects.Count;
        }
    }

    public override int UsedObjectsCount
    {
        get
        {
            return usedObjects.Count;
        }
    }

    public override void Init<T>(T poolItem = null, int preloadAmount = 10, Transform parent = null, bool isLimit = true)
    {
        Type t = typeof(T);
        this.itemName = t.FullName;

		this.preloadAmount = preloadAmount;
		this.limitAmount = this.preloadAmount << 1;
		this.limitInstances = isLimit;
    }

    public override void CreatePoolItems<T>()
    {
		for (int i = 0; i < preloadAmount; i++)
		{
            T item = SpawnNewItem<T>();
            if (item == null)
                break;
            
            freeObjects.Push(item as T);
		}
    }

    public override T SpawnNewItem<T>()
    {
		T item = Activator.CreateInstance<T>();
		if (item == null)
		{
            Debug.LogError("Create instance error!");
            return null;
		}

        return item;
    }

	public override T GetFreeItem<T>()
	{
		return freeObjects.Pop() as T;
	}

    public override void MarkItemUsed<T>(T item, Transform parent = null)
    {
        usedObjects.Add(item);
    }

    public override bool Despawn<T>(T item)
    {
		if (!usedObjects.Contains(item))
		{
			return false;
		}

		usedObjects.Remove(item);

		// recycle used object
        freeObjects.Push(item);
		return true;
    }

    public override void DespawnAll()
    {
		for (int i = usedObjects.Count - 1; i >= 0; i--)
		{
			var node = usedObjects[i];
			Despawn(node);
		}
    }
}
