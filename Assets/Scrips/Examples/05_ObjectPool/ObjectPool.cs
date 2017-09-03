using UnityEngine;
using System.Collections;

public class TestObjectItem
{
    public string name;
    public int num;
}

public class ObjectPool : MonoBehaviour
{
    private MLPoolManager poolMgr;

    private GUIStyle btnStyle;

    void Awake()
    {
		poolMgr = MLPoolManager.Instance;
		poolMgr.DontDestroyOnLoad = true;

		btnStyle = new GUIStyle("button");
		btnStyle.fontSize = 24;
    }

    void Start()
    {
        //poolMgr.CreatePool<MLObjectPool, TestObjectItem>(preloadAmount:10);
    }


}
