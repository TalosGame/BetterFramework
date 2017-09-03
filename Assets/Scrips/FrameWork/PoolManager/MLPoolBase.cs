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

public abstract class MLPoolBase<T> where T : class
{
    public string itemName;

    protected Stack<T> freeObjects = new Stack<T>();
	protected LinkedList<T> usedObjects = new LinkedList<T>();

    protected int preloadAmount;
    protected int limitAmount;
    protected bool limitInstances;

    public abstract void Init<T>(T poolItem = null, int preloadAmount = 10, Transform parent = null,
                                 bool isLimit = true) where T : class;

    public abstract void CreatePoolItems<T>() where T : class;

    public T Spawn<T>(Transform parent = null) where T : class
    {
        int freeObjCnt = freeObjects.Count;
        int useObjCnt = usedObjects.Count;

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

        item = GetFreeIetm<T>();
		MarkItemUsed(item, parent);
		return item as T;
    }

    public abstract T SpawnNewItem<T>() where T : class;

    private T GetFreeIetm<T>() where T : class
    {
        return freeObjects.Pop() as T;
    }

    public abstract void MarkItemUsed<T>(T item, Transform parent = null) where T : class;

	public abstract bool Despawn<T>(T item) where T : class;

    public abstract void DespawnAll();
}