using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestObjectItem
{
    public string name;
    public int num;
}

public class TObjectPool : MonoBehaviour
{
	private const string SPAWN_PREFAB_OBJECT_TEXT = "从池取";
	private const string DESPAWN_PREFAB_OBJECT_TEXT = "放回池";
	private const string DESPAWN_ALL_PREFAB_OBJECT_TEXT = "全部放回池";

    private const string POOL_ITEM_NAME = "TestObjectItem";

    private MLPoolManager poolMgr;
    private GUIStyle btnStyle;
    private GUIStyle labStyle;

    private List<TestObjectItem> liveObjects = new List<TestObjectItem>();

    void Awake()
    {
		poolMgr = MLPoolManager.Instance;
		poolMgr.DontDestroyOnLoad = true;

		btnStyle = new GUIStyle("button");
		btnStyle.fontSize = 24;

        labStyle = new GUIStyle();
        labStyle.fontSize = 32;
    }

    void Start()
    {
        poolMgr.CreatePool<MLObjectPool, TestObjectItem>(preloadAmount:10);
    }

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 120, 30), SPAWN_PREFAB_OBJECT_TEXT, btnStyle))
		{
			SpawnObject();
			return;
		}

		if (GUI.Button(new Rect(10, 50, 120, 30), DESPAWN_PREFAB_OBJECT_TEXT, btnStyle))
		{
			DespawnObject();
			return;
		}

		if (GUI.Button(new Rect(10, 90, 120, 30), DESPAWN_ALL_PREFAB_OBJECT_TEXT, btnStyle))
		{
			DespawnAllObject();
			return;
		}

        DebugInfo();
	}

    private void DebugInfo()
    {
		MLPoolBase pool = MLPoolManager.Instance.GetPool<TestObjectItem>(POOL_ITEM_NAME);

		GUI.Label(new Rect(400, 10, 200, 32), "Object free num:" + pool.FreeObjectsCount
				  + " used num:" + pool.UsedObjectsCount, labStyle);
		for (int i = 0; i < liveObjects.Count; i++)
		{
			TestObjectItem item = liveObjects[i];
			GUI.Label(new Rect(200, 50 + i * 40, 200, 32), "Name:" + item.name + " num:" + item.num, labStyle);
		}
    }

    private void SpawnObject()
    {
		TestObjectItem objectItem = poolMgr.Spawn<TestObjectItem>(POOL_ITEM_NAME);
        if(objectItem == null)
        {
            Debug.LogWarning("Can't spawn object item!!");
            return;
        }

        int itemNum = 0;
        if(liveObjects.Count != 0)
        {
            TestObjectItem item = liveObjects[liveObjects.Count - 1];
            itemNum = item.num + 1;
        }

        objectItem.name = "objectItem" + itemNum;
        objectItem.num = itemNum;

		liveObjects.Add(objectItem);
    }

    private void DespawnObject()
    {
		if (liveObjects.Count <= 0)
			return;

		TestObjectItem terryTrans = liveObjects[liveObjects.Count - 1];
		poolMgr.Despawn<TestObjectItem>(POOL_ITEM_NAME, terryTrans);
		liveObjects.Remove(terryTrans);
    }

    private void DespawnAllObject()
    {
		if (liveObjects.Count <= 0)
			return;

		poolMgr.DespawnAll<TestObjectItem>(POOL_ITEM_NAME);
		liveObjects.Clear();
    }


}
