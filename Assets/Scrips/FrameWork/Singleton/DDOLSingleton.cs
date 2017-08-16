using UnityEngine;
using System.Collections;
using LuaInterface;

public abstract class DDOLSingleton<T> : MonoBehaviour where T : DDOLSingleton<T>
{
	private static T _instance;

    private const string DDOL_MANAGER_TAG = "DDOLManagerRoot";

    public static T Instance
	{
		get 
		{
			if (_instance == null)
			{
                _instance = GameObject.FindObjectOfType<T>();

                if(_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();

                    AddToDDLRoot(go);
                }

                _instance.Init();
			}
			return _instance;
		}
	}

    protected virtual void Init(){}

    /// <summary>
    /// 添加到不销毁根节点管理
    /// </summary>
    /// <param name="childObj">子物体对象</param>
    private static void AddToDDLRoot(GameObject childObj)
    {
        GameObject ddolMgr =  GameObject.FindGameObjectWithTag(DDOL_MANAGER_TAG);
        if(ddolMgr == null)
        {
            ddolMgr = new GameObject("DDOLManagerRoot");
            ddolMgr.tag = DDOL_MANAGER_TAG;

            DontDestroyOnLoad(ddolMgr);
        }

        childObj.transform.parent = ddolMgr.transform;
    }

	void OnDestroy()
	{
		_instance = null;
		Destroy();
        Debugger.Log(this.gameObject.name + " OnDestroy!");
	}

	protected virtual void Destroy() { }
}

