using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPool : MonoBehaviour 
{
	private const string SPAWN_PREFAB_OBJECT_TEXT = "从池取";
	private const string DESPAWN_PREFAB_OBJECT_TEXT = "放回池";
    private const string DESPAWN_ALL_PREFAB_OBJECT_TEXT = "全部放回池";

    private const string POOL_ITEM_NAME = "TerryRole";

    public Transform prefabTrans;

    private MLPoolManager poolMgr;
    private GUIStyle btnStyle;
    private List<Transform> liveObjects = new List<Transform>();

    void Awake()
    {
        poolMgr = MLPoolManager.Instance;
        poolMgr.DontDestroyOnLoad = true;

		btnStyle = new GUIStyle("button");
		btnStyle.fontSize = 24;
    }

    void Start () 
    {
        poolMgr.CreatePool<MLPrefabPool, Transform>(prefabTrans, 10);
	}

	void OnGUI () 
    {
        if (GUI.Button(new Rect(10, 10, 120, 30), SPAWN_PREFAB_OBJECT_TEXT, btnStyle))
        {
            SpawnPrefabObject();
            return;
        }

		if (GUI.Button(new Rect(10, 50, 120, 30), DESPAWN_PREFAB_OBJECT_TEXT, btnStyle))
		{
            DespawnPrefaObject();
			return;
		}

        if (GUI.Button(new Rect(10, 90, 120, 30), DESPAWN_ALL_PREFAB_OBJECT_TEXT, btnStyle))
		{
			DespawnAllPrefaObject();
			return;
		}
	}

    private void SpawnPrefabObject()
    {
        Transform terryTrans = poolMgr.Spawn<Transform>(POOL_ITEM_NAME);
        terryTrans.localPosition = MathUtil.RandomVec2();
        terryTrans.localScale = new Vector3(0.5f, 0.5f, 1f);

        liveObjects.Add(terryTrans);
    }

    private void DespawnPrefaObject()
    {
        if (liveObjects.Count <= 0)
            return;

        Transform terryTrans = liveObjects[0];
        poolMgr.Despawn<Transform>(POOL_ITEM_NAME, terryTrans);
        liveObjects.Remove(terryTrans);
    }

    private void DespawnAllPrefaObject()
    {
		if (liveObjects.Count <= 0)
			return;

        poolMgr.DespawnAll<Transform>(POOL_ITEM_NAME);
        liveObjects.Clear();
    }
}
