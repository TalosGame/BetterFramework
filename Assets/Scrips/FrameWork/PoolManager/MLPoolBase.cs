using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 池当前状态
/// </summary>
public enum PoolStatus
{
    overLimitAmount = 0,
    noFreeItem,
}

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

		if (freeObjCnt + useObjCnt >= limitAmount && limitInstances)
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