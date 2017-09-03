using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MLObjectPool : MLPoolBase<object>
{
    public override void Init<T>(T poolItem = null, int preloadAmount = 10, Transform parent = null, bool isLimit = true)
    {
        //this.itemName = this.prefabTrans.name;

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
            
            freeObjects.Push(item);
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

    public override void MarkItemUsed<T>(T item, Transform parent = null)
    {
        usedObjects.AddFirst(item);
    }

    public override bool Despawn<T>(T item)
    {
        Type t = typeof(T);
        if (t.FullName != this.itemName)
		{
			return false;
		}

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
		var node = usedObjects.First;
		while (node != null)
		{
			var next = node.Next;
            Despawn<object>(node.Value);
			node = next;
		}
    }
}
