using UnityEngine;

public abstract class MLPoolBase
{
    public string itemName;

    protected int preloadAmount;
    protected int limitAmount;
    protected bool limitInstances;

    public abstract void Init<T>(T poolItem, int preloadAmount = 10, Transform parent = null, 
                                 bool isLimit = true) where T : class;

    public abstract void CreatePoolItems();

    public abstract T Spawn<T>(Transform parent = null) where T : class;

    public abstract bool Despawn<T>(T item) where T : class;
}